using System.Linq;
using System.Net.Http.Headers;

namespace PublisherFunctionApp.Validation
{
 
    /// <summary>
    ///  Validating model values for ImageConverterFunction function
    /// </summary>
    public class HeaderValidation
    {
        public static bool ValidateHeader(HttpRequestHeaders httprequesHeader, ref string message)
        {

            try
            {
                //Check if File Extension is not passed or empty
                if (!httprequesHeader.Contains(Constants.FILE_EXTENSION) || string.IsNullOrEmpty(httprequesHeader.GetValues(Constants.FILE_EXTENSION).First()))
                {
                    message = $"The given header {Constants.FILE_EXTENSION} was not found.";
                    return false;
                }
                //Check if File Type is not passed or empty
                if (!httprequesHeader.Contains(Constants.FILE_TYPE) || string.IsNullOrEmpty(httprequesHeader.GetValues(Constants.FILE_TYPE).First()))
                {
                    message = $"The given header {Constants.FILE_TYPE} was not found.";
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// Validating model values for Imagefile 
        /// </summary>
        public static bool ValidateImageHeader(HttpRequestHeaders httprequesHeader, ref string message)
        {
            try
            {
                //Check if Thumnail ImageHeight is not passed or empty
                if (!httprequesHeader.Contains(Constants.THUMBNAIL_HEIGHT) || string.IsNullOrEmpty(httprequesHeader.GetValues(Constants.THUMBNAIL_HEIGHT).First()))
                {
                    message = $"The given header {Constants.THUMBNAIL_HEIGHT} was not found.";
                    return false;
                }
                //Check if Inline Image Height is not passed or empty
                if (!httprequesHeader.Contains(Constants.INLINE_HEIGHT) || string.IsNullOrEmpty(httprequesHeader.GetValues(Constants.INLINE_HEIGHT).First()))
                {
                    message = $"The given header {Constants.INLINE_HEIGHT} was not found.";
                    return false;
                }
                //Check if Thumbnail Image Width is not passed or empty
                if (!httprequesHeader.Contains(Constants.THUMBNAIL_WIDTH) || string.IsNullOrEmpty(httprequesHeader.GetValues(Constants.THUMBNAIL_WIDTH).First()))
                {
                    message = $"The given header {Constants.THUMBNAIL_WIDTH} was not found.";
                    return false;
                }
                //Check if Inline Image Width is not passed or empty
                if (!httprequesHeader.Contains(Constants.INLINE_WIDTH) || string.IsNullOrEmpty(httprequesHeader.GetValues(Constants.INLINE_WIDTH).First()))
                {
                    message = $"The given header {Constants.INLINE_WIDTH} was not found.";
                    return false;
                }
                return true;
            }
            catch
            {
                return false;

            }

        }

        /// <summary>
        /// Validating model values for Pdffile 
        /// </summary>
        public static bool ValidatePdfFileHeader(HttpRequestHeaders httprequesHeader, ref string message)
        {

            try
            {
                //Check if height is not passed or empty
                if (!httprequesHeader.Contains(Constants.HEIGHT) || string.IsNullOrEmpty(httprequesHeader.GetValues(Constants.HEIGHT).First()))
                {
                    message = $"The given header {Constants.HEIGHT} was not found.";
                    return false;
                }
                //Check if width is not passed or empty
                if (!httprequesHeader.Contains(Constants.WIDTH) || string.IsNullOrEmpty(httprequesHeader.GetValues(Constants.WIDTH).First()))
                {
                    message = $"The given header {Constants.WIDTH} was not found.";
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }

        }

    }
}
