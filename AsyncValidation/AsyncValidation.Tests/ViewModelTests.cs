using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AsyncValidation.ProgramDispatcher;
using Microsoft.Practices.Unity;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace AsyncValidation.Tests
{
    [TestFixture]
    public class ViewModelTests : TestsBase
    {
        private class ViewModelStub : ViewModel
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

            public new IProgramDispatcher Dispatcher => base.Dispatcher;

            public ViewModelStub(IUnityContainer unityContainer)
                : base(unityContainer)
            {

            }

            public new void RegisterValidator<T>(Expression<Func<T>> propertyExpression,
                Func<List<string>> validatorFunc) => base.RegisterValidator(propertyExpression, validatorFunc);

            public new void ValidateAll() => base.ValidateAll();

            public new void Validate(string property) => base.Validate(property);
        }

        private readonly string _prop1Error1 = "Property 1 Error 1";
        private readonly string _prop2Error1 = "Property 2 Error 1";
        private readonly string _prop2Error2 = "Property 2 Error 2";

        [Test]
        public void ConstructorTest()
        {
            var viewModel = new ViewModelStub(UnityContainerMock);

            Assert.AreEqual(DispatcherMock, viewModel.Dispatcher);
            UnityContainerMock.Received().Resolve<IProgramDispatcher>();
        }

        [Test]
        public void NoErrorsValidationTest()
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
        public void OneErrorValidationTest()
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
        public void TwoErrorsValidationTest()
        {
            var viewModel = CreateTestViewModel(new List<string> { _prop1Error1 }, new List<string> { _prop2Error1, _prop2Error2 });

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
        public void MultipleFieldsValidationTest()
        {
            var viewModel = CreateTestViewModel(
                new List<string> { "Message1" },
                new List<string> { "Message1" });

            viewModel.PropertyToValidate1 = "Test";
            viewModel.PropertyToValidate2 = "Test";
            var errors = viewModel.GetErrors();
            var prop1Errors = viewModel.GetErrors("PropertyToValidate1").Cast<string>().ToList();
            var prop2Errors = viewModel.GetErrors("PropertyToValidate2").Cast<string>().ToList();

            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(1, prop1Errors.Count);
            Assert.AreEqual(1, prop2Errors.Count);
        }

        [Test]
        public void RevalidateValidationTest()
        {
            var viewModel = CreateTestViewModel(new List<string> { _prop1Error1 }, new List<string>());
            viewModel.PropertyToValidate1 = "Test";
            viewModel.RegisterValidator(() => viewModel.PropertyToValidate1, () => new List<string>());
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
        public void ValidatorExceptionValidationTest()
        {
            var viewModel = CreateTestViewModel(new List<string> { _prop1Error1 }, new List<string>());
            var validatorProp1Mock = Substitute.For<Func<List<string>>>();
            validatorProp1Mock.Invoke().ThrowsForAnyArgs(new Exception("Test Exception"));
            viewModel.RegisterValidator(() => viewModel.PropertyToValidate1, validatorProp1Mock);

            viewModel.PropertyToValidate1 = "Test";
            var errors = viewModel.GetErrors();
            var prop1Errors = viewModel.GetErrors("PropertyToValidate1").Cast<string>().ToList();

            Assert.IsTrue(viewModel.HasErrors);
            Assert.IsFalse(viewModel.IsValid);
            Assert.IsFalse(viewModel.IsValidating);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Test Exception", errors[0]);
            Assert.AreEqual(1, prop1Errors.Count);
            CollectionAssert.DoesNotContain(errors, _prop1Error1);
            Assert.AreEqual("Test Exception", prop1Errors[0]);
        }

        [Test]
        public void ValidateAllValidationTest()
        {
            var viewModel = CreateTestViewModel(new List<string> { _prop1Error1 }, new List<string> { _prop2Error1, _prop2Error2 });

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

        [Test]
        public void SimilarErrorsTest()
        {
            var viewModel = CreateTestViewModel(new List<string> { _prop1Error1 }, new List<string> { _prop2Error1, _prop2Error2 });

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
        [TestCase("UnexistingProperty")]
        public void GetErrorsWrongPropertyNameTest(string propertyName)
        {
            var viewModel = CreateTestViewModel(new List<string> { _prop1Error1 }, new List<string> { _prop2Error1, _prop2Error2 });

            var errors = viewModel.GetErrors(propertyName);

            CollectionAssert.IsEmpty(errors);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void RunValidationEmptyPropertyTest(string propertyName)
        {
            var viewModel = CreateTestViewModel(new List<string> { _prop1Error1 }, new List<string> { _prop2Error1, _prop2Error2 });

            Assert.That(() => viewModel.Validate(propertyName), Throws.TypeOf<ArgumentException>());
        }

        private ViewModelStub CreateTestViewModel()
        {
            var viewModel = new ViewModelStub(UnityContainerMock);
            TaskFactoryMock.StartNew(Arg.Any<Action>())
                .Returns(new Task(() => { }))
                .AndDoes(info => info.Arg<Action>().Invoke());

            return viewModel;
        }

        private ViewModelStub CreateTestViewModel(List<string> property1Errors, List<string> property2Errors)
        {
            var viewModel = CreateTestViewModel();
            viewModel.RegisterValidator(() => viewModel.PropertyToValidate1, () => property1Errors);
            viewModel.RegisterValidator(() => viewModel.PropertyToValidate2, () => property2Errors);

            return viewModel;
        }
    }
}