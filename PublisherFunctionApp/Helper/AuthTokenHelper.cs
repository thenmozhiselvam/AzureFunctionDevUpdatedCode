using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace PublisherFunctionApp.Helper
{
    [ExcludeFromCodeCoverage]
    public class AuthTokenHelper
    {
        /// <summary>
        /// Get the Access token of the azure sql db
        /// </summary>
        public static Task<String> GetSqlTokenAsync(string TenantId, string SqlEndPointURI)
        {
            var provider = new AzureServiceTokenProvider();
            return provider.GetAccessTokenAsync(SqlEndPointURI, TenantId);
        }

    }
}
