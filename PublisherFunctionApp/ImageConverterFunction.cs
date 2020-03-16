using Aspose.Pdf.Facades;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PublisherFunctionApp.Model;
using PublisherFunctionApp.Validation;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PublisherFunctionApp
{


    public class ImageConverterFunction
    {

        /// <summary>
        /// Convert the pdf or image into thumbnail image
        /// For pdf one thumbnail image will be generated using aspose library.
        /// For Images, one thumbnail image and one inline image will be generated using .net Bitmap class.
        /// </summary>
        /// <param name="httpRequestMessage"></param>
        /// <param name="log"></param>
        /// <returns>httpResponseMessage</returns>
        [FunctionName(Constants.IMAGE_CONVERTER_FUNCTION)]

        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage httpRequestMessage, ILogger log)
        {
            log.LogInformation($"ImageConverterFunction :HTTP trigger function processed request at : { DateTime.Now}");
            //Binding file content from httpRequestMessage
            var fileContent = httpRequestMessage.Content;
            string jsonContent = fileContent.ReadAsStringAsync().Result;
            //Get headers from httpRequestMessage
            var headers = httpRequestMessage.Headers;
            var jsonToReturn = string.Empty;
            string errormessage = string.Empty;
            HeaderModel headerModel = new HeaderModel();
            try
            {
                //Validating headers from httprequestMessage
                if (HeaderValidation.ValidateHeader(headers, ref errormessage))
                {
                    headerModel.FileExtension = headers.GetValues(Constants.FILE_EXTENSION).First();
                    headerModel.FileType = headers.GetValues(Constants.FILE_TYPE).First();
                    log.LogInformation($"HeaderValidation success for File extension and FileType");
                }
                else
                {
                    //Header values has empty or null return badrequest response
                    log.LogInformation($"HeaderValidation Failure for File extension and FileType :{errormessage}");
                    return new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent($"{errormessage}")
                    };
                }

                #region ImageToThumbnail 
                //Converting Image file to Thumbnail image
                if (headerModel.FileExtension == Constants.PNG || headerModel.FileExtension == Constants.JPEG)
                {
                    try
                    {
                        //Validating Image specific headers
                        if (HeaderValidation.ValidateImageHeader(headers, ref errormessage))
                        {
                            headerModel.ThumnailImageHeight = headers.GetValues(Constants.THUMBNAIL_HEIGHT).First();
                            headerModel.ThumnailImageWidth = headers.GetValues(Constants.THUMBNAIL_WIDTH).First();
                            headerModel.InlineImageHeight = headers.GetValues(Constants.INLINE_HEIGHT).First();
                            headerModel.InlineImageWidth = headers.GetValues(Constants.INLINE_WIDTH).First();
                            log.LogError($"HeaderValidation Success for Image to thumbnail conversion");
                        }
                        else
                        {
                            //Header values has empty or null return badrequest response
                            log.LogInformation($"HeaderValidation Failure in Image to thumbnail conversion: {errormessage}");
                            return new HttpResponseMessage(HttpStatusCode.BadRequest)
                            {
                                Content = new StringContent($"{ errormessage }", Encoding.UTF8, Constants.JSON)
                            };

                        }
                        //Convert file content to memorystream
                        var memoryStream = new MemoryStream(Convert.FromBase64String(jsonContent));
                        var imageObj = new { thumbnail = ConvertToThumbnail(memoryStream, Convert.ToInt32(headerModel.ThumnailImageHeight), Convert.ToInt32(headerModel.ThumnailImageWidth), log), inline = ConvertToThumbnail(memoryStream, Convert.ToInt32(headerModel.InlineImageHeight), Convert.ToInt32(headerModel.InlineImageWidth), log) };
                        jsonToReturn = JsonConvert.SerializeObject(imageObj);
                    }
                    catch (Exception ex)
                    {
                        log.LogError($"Exception occurred in Image file conversion to thumbnail, Error : {ex.Message},Details:{ex.InnerException}");
                        return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                        {
                            Content = new StringContent(ex.Message, Encoding.UTF8, Constants.JSON)
                        };
                    }


                }
                #endregion

                #region PDFToThumbnail
                //Converting Pdf first page to Thumbnail image
                if (headerModel.FileExtension == Constants.PDF)
                {
                    try
                    {
                        //To include license
                        Aspose.Pdf.License license = new Aspose.Pdf.License();
                        license.SetLicense("Aspose.Pdf.lic");
                        log.LogInformation("Aspose SetLicense Success");
                        ImageFormat format = GetImageFormat(headerModel.FileType, log);
                        PdfConverter pdfConverter = new PdfConverter();
                        //Validating pdf file specific headers
                        if (HeaderValidation.ValidatePdfFileHeader(headers, ref errormessage))
                        {
                            headerModel.Height = headers.GetValues(Constants.HEIGHT).First();
                            headerModel.Width = headers.GetValues(Constants.WIDTH).First();
                            log.LogInformation($"HeaderValidation Success for PDF to thumbnail conversion");
                        }
                        else
                        {
                            //Header values has empty or null return badrequest response
                            log.LogError($"HeaderValidation Failure for PDF to thumbnail conversion : {errormessage}");
                            return new HttpResponseMessage(HttpStatusCode.BadRequest)
                            {
                                Content = new StringContent($"{errormessage}", Encoding.UTF8, Constants.JSON)
                            };
                        }

                        var streamContent = httpRequestMessage.Content.ReadAsStreamAsync();
                        pdfConverter.BindPdf(streamContent.Result);
                        //To convert first page of PDF
                        pdfConverter.StartPage = 1;
                        pdfConverter.EndPage = 1;
                        pdfConverter.Resolution = new Aspose.Pdf.Devices.Resolution(100);
                        pdfConverter.DoConvert();
                        MemoryStream imageStream = new MemoryStream();
                        while (pdfConverter.HasNextImage())
                        {
                            // Save the image in the given image Format
                            pdfConverter.GetNextImage(imageStream, format);
                            // Set the stream position to the beginning of the stream
                            imageStream.Position = 0;
                        }
                        var imageObj = new { content = ConvertToThumbnail(imageStream, Convert.ToInt32(headerModel.Width), Convert.ToInt32(headerModel.Height), log), contentType = "image/" + headerModel.FileType };
                        jsonToReturn = JsonConvert.SerializeObject(imageObj);
                        pdfConverter.Close();
                        imageStream.Close();
                    }
                    catch (Exception ex)
                    {
                        log.LogError($"Exception occurred in pdf file conversion to thumbnail,Error : {ex.Message},Details:{ex.InnerException}");
                        return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                        {
                            Content = new StringContent(ex.Message, Encoding.UTF8, Constants.JSON)
                        };
                    }

                }

                #endregion
                log.LogInformation("ImageConverterFunction successfully processed.");
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonToReturn, Encoding.UTF8, Constants.JSON)
                };

            }
            catch (Exception ex)
            {
                log.LogError($"Exception occurred in ImageConverterFunction,Error: { ex.Message},Details: { ex.InnerException}");
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(ex.Message, Encoding.UTF8, Constants.JSON)
                };

            }

        }


        /// <summary>
        /// ConvertToThumbnail method to Convert memorystream as a imagebytes
        /// </summary>
        /// <param name="memoryStream"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static Byte[] ConvertToThumbnail(MemoryStream memoryStream, int width, int height, ILogger log)
        {
            log.LogInformation("In ConvertToThumbnail method");
            Image image = null;
            byte[] imgbytes = null;
            Bitmap sourceImage = null;
            try
            {
                sourceImage = new Bitmap(memoryStream);

                using (Bitmap objBitmap = new Bitmap(width, height))
                {
                    objBitmap.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
                    Graphics objGraphics = Graphics.FromImage(objBitmap);
                    objGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    objGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    objGraphics.DrawImage(sourceImage, 0, 0, width, height);
                    image = objBitmap;
                    ImageConverter converter = new ImageConverter();
                    //return image bytes 
                    imgbytes = (byte[])converter.ConvertTo(image, typeof(byte[]));

                }

            }
            catch (Exception ex)
            {
                log.LogError($"Exception occurred while converting from memorystream as a imagebytes, Error : {ex.Message},Details:{ex.InnerException}");
                image = null;
                throw ex;
            }
            log.LogInformation("Out ConvertToThumbnail method");
            return imgbytes;
        }

        /// <summary>
        /// Get ImageFormat passing a string value
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static ImageFormat GetImageFormat(string extension, ILogger log)
        {
            ImageFormat result = null;
            try
            {
                PropertyInfo prop = typeof(ImageFormat).GetProperties().Where(p => p.Name.Equals(extension, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (prop != null)
                {
                    result = prop.GetValue(prop) as ImageFormat;
                }
            }
            catch (Exception ex)
            {
                log.LogError("Exception occurred in GetImageFormat method : " + ex.Message);
                throw ex;
            }

            return result;
        }
    }
}








