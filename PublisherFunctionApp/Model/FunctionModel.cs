using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace PublisherFunctionApp.Model
{
    [ExcludeFromCodeCoverage]
    public class MessageModel
    {

        [Required(AllowEmptyStrings = false, ErrorMessage = "app_id cannot be null")]
        public int app_id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "r_object_id cannot be null")]
        public Guid r_object_id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "EventType cannot be null")]
        public string event_Type { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Lifecycle stage cannot be null")]
        public string lifecyclestage { get; set; }
    }
    [ExcludeFromCodeCoverage]
    public class ConfigurationModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Azure SQL Connection string cannot be null")]
        public string connStrAzure { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Retrycount cannot be null")]
        public int RetryCount { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Increment Time cannot be null")]
        public TimeSpan IncrementTime { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Increment Time cannot be null")]
        public TimeSpan IntervalTime { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Stored Procedure cannot be null")]
        public string StoredProcedureName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "TenantId cannot be null")]
        public string TenantId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Sql EndPoint URI cannot be null")]
        public string SqlEndPointURI { get; set; }

    }
}
