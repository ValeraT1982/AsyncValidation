using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using AsyncValidation.ProgramDispatcher;
using AsyncValidation.Tasks;
using Microsoft.Practices.Unity;

namespace AsyncValidation
{
    public abstract class ViewModel : INotifyDataErrorInfo, INotifyPropertyChanged
    {
        private bool _isValidating;
        public bool IsValidating
        {
            get { return _isValidating; }
            protected set { Set(ref _isValidating, value); }
        }

        private bool _isValid = true;
        public bool IsValid
        {
            get { return _isValid; }
            protected set { Set(ref _isValid, value); }
        }

        protected IUnityContainer UnityContainer { get; }

        protected ITaskFactory TaskFactory => UnityContainer.Resolve<ITaskFactory>();

        protected IProgramDispatcher Dispatcher => UnityContainer.Resolve<IProgramDispatcher>();

        protected readonly object Lock = new object();

        private readonly Dictionary<string, List<string>> _validationErrors = new Dictionary<string, List<string>>();

        private readonly Dictionary<Guid, string> _validationProcesses = new Dictionary<Guid, string>();

        private readonly Dictionary<string, Func<List<string>>> _validators = new Dictionary<string, Func<List<string>>>();


        protected ViewModel(IUnityContainer unityContainer)
        {
            UnityContainer = unityContainer;
            PropertyChanged += (sender, args) => Validate(args.PropertyName);
        }

        #region INotifyDataErrorInfo
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || !_validationErrors.ContainsKey(propertyName))
            {
                return new List<string>();
            }

            return _validationErrors[propertyName];
        }

        public bool HasErrors => _validationErrors.Count > 0;
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public virtual List<string> GetErrors()
        {
            return _validationErrors.SelectMany(p => p.Value).Distinct().ToList();
        }

        protected void Set<T>(ref T storage, T value, [CallerMemberName] string property = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;

            RaisePropertyChanged(property);
        }

        protected void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        protected void RegisterValidator<TProperty>(Expression<Func<TProperty>> propertyExpression, Func<List<string>> validatorFunc)
        {
            RegisterValidator(PropertyHelper.GetPropertyName(propertyExpression), validatorFunc);
        }

        protected void RegisterValidator(string propertyName, Func<List<string>> validatorFunc)
        {
            lock (Lock)
            {
                if (_validators.ContainsKey(propertyName))
                {
                    _validators.Remove(propertyName);
                }

                _validators[propertyName] = validatorFunc;
            }
        }

        protected void Validate(string property)
        {
            if (string.IsNullOrWhiteSpace(property))
            {
                throw new ArgumentException();
            }

            Func<List<string>> validator;

            if (!_validators.TryGetValue(property, out validator))
            {
                return;
            }

            TaskFactory.StartNew(() =>
            {
                var validationProcessKey = Guid.NewGuid();

                lock (Lock)
                {
                    _validationProcesses.Add(validationProcessKey, property);
                }

                Dispatcher.InvokeOnUI(() => IsValidating = true);

                try
                {
                    var errors = validator();

                    lock (Lock)
                    {
                        if (errors != null && errors.Any())
                        {
                            _validationErrors[property] = errors;
                        }
                        else if (_validationErrors.ContainsKey(property))
                        {
                            _validationErrors.Remove(property);
                        }
                    }
                }
                catch (Exception ex)
                {
                    lock (Lock)
                    {
                        _validationErrors[property] = new List<string>(new[]
                        {
                            ex.Message
                        });
                    }
                }
                finally
                {
                    bool localIsValidating;
                    bool localIsValid;

                    lock (Lock)
                    {
                        _validationProcesses.Remove(validationProcessKey);
                        localIsValidating = _validationProcesses.Any();
                        localIsValid = !_validationProcesses.Any() && !_validationErrors.Any();
                    }

                    Dispatcher.InvokeOnUI(() =>
                    {
                        IsValidating = localIsValidating;
                        IsValid = localIsValid;
                        OnErrorsChanged(property);
                    });
                }
            });
        }

        protected void ValidateAll()
        {
            Dictionary<string, Func<List<string>>> validators;

            lock (Lock)
            {
                validators = _validators;
            }

            foreach (var propertyName in validators.Keys)
            {
                Validate(propertyName);
            }
        }

        private void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}
