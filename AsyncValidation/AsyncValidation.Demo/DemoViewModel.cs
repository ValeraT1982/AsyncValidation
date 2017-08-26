using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncValidation.Demo
{
    class DemoViewModel : ValidatableViewModel
    {
        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                Set(ref _name, value);
            }
        }

        private string _description;
        public string Description
        {
            get
            {
                return _description;
            }

            set
            {
                Set(ref _description, value);
            }
        }

        private int _number;
        public int Number
        {
            get
            {
                return _number;
            }

            set
            {
                Set(ref _number, value);
            }
        }

        public List<int> AvailableNumbers => new List<int>(new[] { 1, 2, 3, 5, 7, 11 });

        public DemoViewModel()
        {
            RegisterValidator(() => Name, ValidateName);
            RegisterValidator(() => Description, ValidateDescription);
            RegisterValidator(() => Number, ValidateNumber);
            ValidateAll();
        }

        private async Task<List<string>> ValidateName()
        {
            await Task.Delay(3000);

            if (string.IsNullOrWhiteSpace(Name))
            {
                return new List<string> { "Name cannot be empty" };
            }

            if (Name.Length > 10)
            {
                return new List<string> { "Name cannot be more than 10 characters" };
            }

            return new List<string>();
        }

        private async Task<List<string>> ValidateDescription()
        {
            await Task.Delay(4000);

            if (string.IsNullOrWhiteSpace(Description))
            {
                return new List<string> { "Description cannot be empty" };
            }

            if (Description.Length > 50)
            {
                return new List<string> { "Name cannot be more than 50 characters" };
            }

            return new List<string>();
        }

        private async Task<List<string>> ValidateNumber()
        {
            await Task.Delay(2000);

            if (Number > 5)
            {
                return new List<string> { "Name cannot be more than 5" };
            }

            return new List<string>();
        }
    }
}