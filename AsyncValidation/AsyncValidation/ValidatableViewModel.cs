using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AsyncValidation
{
    public abstract class ValidatableViewModel : INotifyDataErrorInfo, INotifyPropertyChanged
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

        private readonly Dictionary<string, List<string>> _validationErrors = new Dictionary<string, List<string>>();
        private readonly Dictionary<string, Guid> _lastValidationProcesses = new Dictionary<string, Guid>();
        private readonly Dictionary<string, Func<Task<List<string>>>> _validators = new Dictionary<string, Func<Task<List<string>>>>();

        protected ValidatableViewModel()
        {
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

        public List<string> GetErrors()
        {
            return _validationErrors.SelectMany(p => p.Value).ToList();
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

        protected void RegisterValidator<TProperty>(Expression<Func<TProperty>> propertyExpression, Func<Task<List<string>>> validatorFunc)
        {
            RegisterValidator(PropertyHelper.GetPropertyName(propertyExpression), validatorFunc);
        }

        protected void RegisterValidator(string propertyName, Func<Task<List<string>>> validatorFunc)
        {
            if (_validators.ContainsKey(propertyName))
            {
                _validators.Remove(propertyName);
            }

            _validators[propertyName] = validatorFunc;
        }

        protected async Task Validate(string property)
        {
            if (string.IsNullOrWhiteSpace(property))
            {
                throw new ArgumentException();
            }

            Func<Task<List<string>>> validator;
            if (!_validators.TryGetValue(property, out validator))
            {
                return;
            }

            var validationProcessKey = Guid.NewGuid();
            _lastValidationProcesses[property] = validationProcessKey;
            IsValidating = true;
            try
            {
                var errors = await validator();
                if (_lastValidationProcesses.ContainsKey(property) && 
                    _lastValidationProcesses[property] == validationProcessKey)
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
                _validationErrors[property] = new List<string>(new[] { ex.Message });
            }
            finally
            {
                if (_lastValidationProcesses.ContainsKey(property) && 
                    _lastValidationProcesses[property] == validationProcessKey)
                {
                    _lastValidationProcesses.Remove(property);
                }

                IsValidating = _lastValidationProcesses.Any();
                IsValid = !_lastValidationProcesses.Any() && !_validationErrors.Any();
                OnErrorsChanged(property);
            }
        }

        protected async Task ValidateAll()
        {
            var validators = _validators;
            foreach (var propertyName in validators.Keys)
            {
                await Validate(propertyName);
            }
        }

        private void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}