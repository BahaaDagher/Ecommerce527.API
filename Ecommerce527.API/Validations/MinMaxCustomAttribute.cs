using System.ComponentModel.DataAnnotations;

namespace Ecommerce527.API.Validations
{
    public class MinMaxCustomAttribute : ValidationAttribute
    {
        private int MinLength; 
        private int MaxLength; 
        public MinMaxCustomAttribute(int mnLength  , int mxLength)
        {
            this.MinLength = mnLength;
            this.MaxLength = mxLength;  
        }
        public override string FormatErrorMessage(string name)
        {
            return $"the {name} must be between {MinLength} and {MaxLength}"; 
        }
        public override bool IsValid(object? value)
        {
            if(value is string name)
            {
                return name.Length >= MinLength && name.Length <= MaxLength; 
            }
            return false; 
        }
    }
}
