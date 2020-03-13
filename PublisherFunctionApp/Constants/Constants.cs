using System.Diagnostics.CodeAnalysis;

namespace PublisherFunctionApp
{
    [ExcludeFromCodeCoverage]
    public static class Constants
    {
        public const string IMAGE_CONVERTER_FUNCTION = "ImageConverterFunction";
        public const string GET_DATAFROM_AZURE_SQL_FUNCTION = "GetDataFromAzure_AuthAAD";
        public const string FILE_EXTENSION = "FileExtension";
        public const string PNG = "image/png";
        public const string JPEG = "image/jpeg";
        public const string THUMBNAIL_HEIGHT = "ThumnailImageHeight";
        public const string THUMBNAIL_WIDTH = "ThumbnailWidth";
        public const string INLINE_HEIGHT = "InlineImageHeight";
        public const string INLINE_WIDTH = "InlineWidth";
        public const string PDF = "application/pdf";
        public const string HEIGHT = "Height";
        public const string WIDTH = "Width";
        public const string JSON = "application/json";
        public const string FILE_TYPE = "FileType";
        public const string HEIGHT_VALUE = "72";
        public const string WIDTH_VALUE = "132";


        //GetDataFromAzure_AuthAAD CONSTANTS

        public const string CONFIGURATION_SETTINGS = "ConfigurationSettings";
        public const string WIP = "WIP";
        public const string STAGING = "Staging";
        public const string ACTIVE = "Active";
        public const string WIP_SQLCONNECTIONSTRING = "WIP_SqlConnection";
        public const string STAGING_SQLCONNECTIONSTRING = "Staging_SqlConnection";
        public const string ACTIVE_SQLCONNECTIONSTRING = "Active_SqlConnection";
        public const string SP_APP_ID = "@app_id";
        public const string SP_R_OBJECT_ID = "@r_object_id";
        public const string SP_EVEN_TYPE = "@Eventtype";

        //TABLE NAMES
        public const string TARGET_TABLE = "Target_Table";
        public const string S_TABLE = "S_Table";
        public const string R_TABLE = "R_Table";
        public const string FILEPATH_TABLE = "Filepath_Table";

    }
}


