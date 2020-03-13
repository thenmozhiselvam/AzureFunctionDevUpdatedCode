using PublisherFunctionApp.Model;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace PublisherFunctionApp.Validation
{

    /// <summary>
    /// Validating model values for GetDataFromAzure_AuthAAD function
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ModelValidation
    {
        public static bool ValidateMessageModel(MessageModel messageModel, ref string message)
        {

            var context = new ValidationContext(messageModel, serviceProvider: null, items: null);
            var errorResults = new System.Collections.Generic.List<ValidationResult>();

            // carry out validation.
            var isValid = Validator.TryValidateObject(messageModel, context, errorResults);

            if (errorResults.Count > 0)
            {
                foreach (ValidationResult validationResult in errorResults)
                {
                    message = $"Error in validating Message Model {messageModel}. Error Message : {validationResult.ErrorMessage}";
                }
            }

            return isValid;
        }

        public static bool ValidateConfigurationModel(ConfigurationModel configurationModel, ref string message)
        {

            var context = new ValidationContext(configurationModel, serviceProvider: null, items: null);
            var errorResults = new System.Collections.Generic.List<ValidationResult>();

            // carry out validation.
            var isValid = Validator.TryValidateObject(configurationModel, context, errorResults);

            if (errorResults.Count > 0)
            {
                foreach (ValidationResult validationResult in errorResults)
                {
                    message = $"Error in validating ConfigurationModel Model {configurationModel}. Error Message : {validationResult.ErrorMessage}";
                }
            }

            return isValid;
        }

    }
}
