using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace AsyncValidation.Tests
{
    [TestFixture]
    public class ValidatableViewModelTests
    {
        private class ValidatableViewModelStub : ValidatableViewModel
        {
            private string _propertyToValidate1;

            public string PropertyToValidate1
            {
                get { return _propertyToValidate1; }
                set { Set(ref _propertyToValidate1, value); }
            }

            private string _propertyToValidate2;

            public string PropertyToValidate2
            {
                get { return _propertyToValidate2; }
                set { Set(ref _propertyToValidate2, value); }
            }


            public new void RegisterValidator<T>(Expression<Func<T>> propertyExpression,
                Func<Task<List<string>>> validatorFunc) => base.RegisterValidator(propertyExpression, validatorFunc);

            public new Task ValidateAll() => base.ValidateAll();

            public new Task Validate(string property) => base.Validate(property);
        }

        private readonly string _prop1Error1 = "Property 1 Error 1";
        private readonly string _prop2Error1 = "Property 2 Error 1";
        private readonly string _prop2Error2 = "Property 2 Error 2";

        [Test]
        public void GetErrorsWhenNoErrors()
        {
            var viewModel = CreateTestViewModel(new List<string>(), new List<string>());

            viewModel.PropertyToValidate1 = "Test";
            viewModel.PropertyToValidate2 = "Test";
            var errors = viewModel.GetErrors();

            Assert.IsFalse(viewModel.HasErrors);
            Assert.IsTrue(viewModel.IsValid);
            Assert.IsFalse(viewModel.IsValidating);
            Assert.AreEqual(0, errors.Count);
        }

        [Test]
        public void GetErrorsWhenOnePropertyHasError()
        {
            var viewModel = CreateTestViewModel(new List<string> { _prop1Error1 }, new List<string>());

            viewModel.PropertyToValidate1 = "Test";
            var errors = viewModel.GetErrors();
            var prop1Errors = viewModel.GetErrors("PropertyToValidate1").Cast<string>().ToList();

            Assert.IsTrue(viewModel.HasErrors);
            Assert.IsFalse(viewModel.IsValid);
            Assert.IsFalse(viewModel.IsValidating);
            Assert.AreEqual(1, errors.Count);
            CollectionAssert.Contains(errors, _prop1Error1);
            Assert.AreEqual(1, prop1Errors.Count);
            CollectionAssert.Contains(prop1Errors, _prop1Error1);
        }

        [Test]
        public void GetErrorsWhenTwoPropertiesHaveErrors()
        {
            var viewModel = CreateTestViewModel(
                new List<string> { _prop1Error1 },
                new List<string> { _prop2Error1 });

            viewModel.PropertyToValidate1 = _prop1Error1;
            viewModel.PropertyToValidate2 = _prop2Error1;
            var errors = viewModel.GetErrors();

            var prop1Errors = viewModel.GetErrors("PropertyToValidate1").Cast<string>().ToList();
            var prop2Errors = viewModel.GetErrors("PropertyToValidate2").Cast<string>().ToList();
            Assert.AreEqual(2, errors.Count);
            Assert.AreEqual(1, prop1Errors.Count);
            Assert.AreEqual(1, prop2Errors.Count);
        }

        [Test]
        public void GetErrorsWhenTwoPropertiesHaveErrorsButOnlyOneWasValidated()
        {
            var viewModel = CreateTestViewModel(new List<string> { _prop1Error1 },
                new List<string> { _prop2Error1, _prop2Error2 });

            viewModel.PropertyToValidate2 = "Test";
            var errors = viewModel.GetErrors();
            var prop2Errors = viewModel.GetErrors("PropertyToValidate2").Cast<string>().ToList();

            Assert.IsTrue(viewModel.HasErrors);
            Assert.IsFalse(viewModel.IsValid);
            Assert.IsFalse(viewModel.IsValidating);
            Assert.AreEqual(2, errors.Count);
            CollectionAssert.Contains(errors, _prop2Error1);
            CollectionAssert.Contains(errors, _prop2Error2);
            Assert.AreEqual(2, prop2Errors.Count);
            CollectionAssert.Contains(prop2Errors, _prop2Error1);
            CollectionAssert.Contains(prop2Errors, _prop2Error1);
        }

        [Test]
        public void GetErrorsWhenValidatorException()
        {
            var viewModel = CreateTestViewModel(new List<string> { _prop1Error1 }, new List<string>());
            var validatorProp1Mock = Substitute.For<Func<Task<List<string>>>>();
            validatorProp1Mock.Invoke().Returns(Task.FromException<List<string>>(new Exception("Exception Message")));
            viewModel.RegisterValidator(() => viewModel.PropertyToValidate1, validatorProp1Mock);

            viewModel.PropertyToValidate1 = "Test";
            var errors = viewModel.GetErrors();
            var prop1Errors = viewModel.GetErrors("PropertyToValidate1").Cast<string>().ToList();

            Assert.IsTrue(viewModel.HasErrors);
            Assert.IsFalse(viewModel.IsValid);
            Assert.IsFalse(viewModel.IsValidating);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Exception Message", errors[0]);
            Assert.AreEqual(1, prop1Errors.Count);
            CollectionAssert.DoesNotContain(errors, _prop1Error1);
            Assert.AreEqual("Exception Message", prop1Errors[0]);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("UnexistingProperty")]
        public void GetErrorsWhenWrongPropertyName(string propertyName)
        {
            var viewModel = CreateTestViewModel(new List<string> { _prop1Error1 },
                new List<string> { _prop2Error1, _prop2Error2 });

            var errors = viewModel.GetErrors(propertyName);

            CollectionAssert.IsEmpty(errors);
        }

        [Test]
        public void UseLastRegisteredValidator()
        {
            var viewModel = CreateTestViewModel(new List<string> { _prop1Error1 }, new List<string>());
            viewModel.PropertyToValidate1 = "Test";
            viewModel.RegisterValidator(() => viewModel.PropertyToValidate1, () => Task.FromResult(new List<string>()));
            viewModel.PropertyToValidate1 = "Test 2";

            var errors = viewModel.GetErrors();
            var prop1Errors = viewModel.GetErrors("PropertyToValidate1");

            Assert.IsFalse(viewModel.HasErrors);
            Assert.IsTrue(viewModel.IsValid);
            Assert.IsFalse(viewModel.IsValidating);
            CollectionAssert.IsEmpty(errors);
            CollectionAssert.IsEmpty(prop1Errors);
        }

        [Test]
        public void ValidateAll()
        {
            var viewModel = CreateTestViewModel(new List<string> { _prop1Error1 },
                new List<string> { _prop2Error1, _prop2Error2 });

            viewModel.ValidateAll();
            var errors = viewModel.GetErrors();
            var prop1Errors = viewModel.GetErrors("PropertyToValidate1").Cast<string>().ToList();
            var prop2Errors = viewModel.GetErrors("PropertyToValidate2").Cast<string>().ToList();

            Assert.IsTrue(viewModel.HasErrors);
            Assert.IsFalse(viewModel.IsValid);
            Assert.IsFalse(viewModel.IsValidating);
            Assert.AreEqual(3, errors.Count);
            CollectionAssert.Contains(errors, _prop1Error1);
            CollectionAssert.Contains(errors, _prop2Error1);
            CollectionAssert.Contains(errors, _prop2Error2);
            Assert.AreEqual(1, prop1Errors.Count);
            CollectionAssert.Contains(prop1Errors, _prop1Error1);
            Assert.AreEqual(2, prop2Errors.Count);
            CollectionAssert.Contains(prop2Errors, _prop2Error1);
            CollectionAssert.Contains(prop2Errors, _prop2Error1);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void ValidateWhenEmptyProperty(string propertyName)
        {
            var viewModel = CreateTestViewModel(new List<string> { _prop1Error1 },
                new List<string> { _prop2Error1, _prop2Error2 });

            Assert.That(() => viewModel.Validate(propertyName), Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void IgnorePreviousValidationResult()
        {
            var viewModel = new ValidatableViewModelStub();
            var isFirstCall = true;
            var task = Task.Run(async () =>
            {
                await Task.Delay(1000);

                return new List<string> { "First Error!!!!" };
            });
            viewModel.RegisterValidator(() => viewModel.PropertyToValidate1, () =>
            {
                if (isFirstCall)
                {
                    isFirstCall = false;

                    return task;
                }

                return Task.FromResult(new List<string> { "Second Error!!!!" });
            });

            viewModel.Validate("PropertyToValidate1");
            viewModel.Validate("PropertyToValidate1");
            task.Wait();
            var errors = viewModel.GetErrors();

            Assert.AreEqual("Second Error!!!!", errors[0]);
        }

        private ValidatableViewModelStub CreateTestViewModel(List<string> property1Errors, List<string> property2Errors)
        {
            var viewModel = new ValidatableViewModelStub();
            viewModel.RegisterValidator(() => viewModel.PropertyToValidate1, () => Task.FromResult(property1Errors));
            viewModel.RegisterValidator(() => viewModel.PropertyToValidate2, () => Task.FromResult(property2Errors));

            return viewModel;
        }
    }
}