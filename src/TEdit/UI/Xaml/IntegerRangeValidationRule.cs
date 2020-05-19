using System;
using System.Globalization;
using System.Windows.Controls;

namespace TEdit.UI.Xaml
{
    /// <summary>
    /// Integer Range Binding Validation Rule
    /// http://msdn.microsoft.com/en-us/library/system.windows.data.binding.validationrules.aspx
    /// </summary>
    public class IntegerRangeValidationRule : ValidationRule
    {
        /// <summary>
        /// Maximum Integer Value
        /// </summary>
        public int Max { get; set; }

        /// <summary>
        /// Minimum Integer Value
        /// </summary>
        public int Min { get; set; }

        #region Overrides of ValidationRule

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int val = 0;
            try
            {
                if (((string)value).Length > 0)
                    val = Int32.Parse((String)value);
            }
            catch (Exception e)
            {
                return new ValidationResult(false, "Illegal characters or " + e.Message);
            }

            if ((val < Min) || (val > Max))
            {
                return new ValidationResult(false,
                  "Please enter an integer in the range: " + Min + " - " + Max + ".");
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }

        #endregion
    }
}