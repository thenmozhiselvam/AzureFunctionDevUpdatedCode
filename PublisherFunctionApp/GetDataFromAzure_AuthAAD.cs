using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.SqlAzure;
using Microsoft.Practices.TransientFaultHandling;
using Newtonsoft.Json;
using PublisherFunctionApp.Helper;
using PublisherFunctionApp.Model;
using PublisherFunctionApp.Validation;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PublisherFunctionApp
{
    [ExcludeFromCodeCoverage]
    public static class GetDataFromAzure_AuthAAD
    {

        [FunctionName(Constants.GET_DATAFROM_AZURE_SQL_FUNCTION)]
        /// <summary>
        /// GetDataFromAzure_AuthAAD function to get data from azure sql using AzureAD Authentication 
        /// </summary>
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage httpRequestMessage, ILogger log)
        {
            log.LogInformation($"GetDataFromAzure_AuthAAD Function:HTTP trigger function processed request at : { DateTime.Now}");
            DataSet publishMetadta = new DataSet();
            string errormessage = string.Empty;
            try
            {

                //get the content from httpRequestMessage
                var jsonContent = await httpRequestMessage.Content.ReadAsStringAsync();
                //Get the service bus message values from httpRequestMessage
                MessageModel messageModel = JsonConvert.DeserializeObject<MessageModel>(jsonContent);
                //Get the Configuration values from Function app Configuration
                ConfigurationModel configurationModel = new ConfigurationModel();
                //Validating message values from httprequestMessage
                if (ModelValidation.ValidateMessageModel(messageModel, ref errormessage))
                {
                    log.LogInformation($"Service bus Message values returned from the HttpRequestMessage body: {messageModel}");
                    log.LogInformation($"r_object_id:{messageModel.r_object_id}");
                    log.LogInformation($"lifecyclestage:{messageModel.lifecyclestage}");
                    var configurationJSON = ConfigurationManager.AppSettings[Constants.CONFIGURATION_SETTINGS];
                    configurationModel = JsonConvert.DeserializeObject<ConfigurationModel>(configurationJSON);
                    //Get the Azure SQL connection string value based on the lifecyclestage
                    if (messageModel.lifecyclestage == Constants.WIP)
                    {
                        configurationModel.connStrAzure = ConfigurationManager.ConnectionStrings[Constants.WIP_SQLCONNECTIONSTRING].ConnectionString;
                    }
                    else if (messageModel.lifecyclestage == Constants.STAGING)
                    {
                        configurationModel.connStrAzure = ConfigurationManager.ConnectionStrings[Constants.STAGING_SQLCONNECTIONSTRING].ConnectionString;
                    }
                    else if (messageModel.lifecyclestage == Constants.ACTIVE)
                    {
                        configurationModel.connStrAzure = ConfigurationManager.ConnectionStrings[Constants.ACTIVE_SQLCONNECTIONSTRING].ConnectionString;
                    }
                    else
                    {
                        log.LogError($"Please provide valid lifecyclestage name: {messageModel.lifecyclestage}");
                        return new HttpResponseMessage(HttpStatusCode.BadRequest)
                        {
                            Content = new StringContent($"Please provide valid lifecyclestage name : {messageModel.lifecyclestage}", Encoding.UTF8, Constants.JSON)
                        };

                    }

                    //Validating configuration values
                    if (ModelValidation.ValidateConfigurationModel(configurationModel, ref errormessage))
                    {
                        log.LogInformation($"Configuration values returned from,function app configuration app settings: {configurationModel}");
                        //Get access token for Azure SQL
                        var accessToken = await AuthTokenHelper.GetSqlTokenAsync(configurationModel.TenantId, configurationModel.SqlEndPointURI);
                        publishMetadta = GetDataFromAzureSql(configurationModel, messageModel, accessToken, log);
                        if (publishMetadta != null)
                        {
                            log.LogInformation($"Table count: { publishMetadta.Tables.Count}");
                            string publishMetadtaJSON = JsonConvert.SerializeObject(publishMetadta, Formatting.Indented);
                            //return dataset in the response
                            log.LogInformation($"GetDataFromAzure_AuthAAD Function successfully processed.");
                            return new HttpResponseMessage(HttpStatusCode.OK)
                            {

                                Content = new StringContent(publishMetadtaJSON, Encoding.UTF8, Constants.JSON)
                            };
                        }
                        else
                        {
                            log.LogError($"Content not available in azure sql for the given r_object_id : {messageModel.r_object_id}");
                            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                            {
                                Content = new StringContent($"Content not available in azure sql for the given r_object_id : {messageModel.r_object_id}", Encoding.UTF8, Constants.JSON)
                            };
                        }

                    }
                    else
                    {
                        //configuration values has empty or null return badrequest response
                        log.LogError($"{errormessage}");
                        return new HttpResponseMessage(HttpStatusCode.BadRequest)
                        {
                            Content = new StringContent($"{errormessage}", Encoding.UTF8, Constants.JSON)
                        };

                    }
                }
                else
                {
                    //Message model values has empty or null return badrequest response 
                    log.LogError($"{errormessage}");
                    return new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent($"{errormessage}", Encoding.UTF8, Constants.JSON)
                    };

                }
            }
            catch (Exception ex)
            {
                log.LogError($"Exception occurred in GetDataFromAzure_Function,Error : {ex.Message}, Details:{ex.InnerException}");
                publishMetadta = null;
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(ex.Message, Encoding.UTF8, Constants.JSON)
                };
            }
        }
        /// <summary>
        /// Returns target table,S table, R table and Filepath table data from azure sql
        /// </summary>
        /// <param name="configurationModel"></param>
        /// <param name="functionModel"></param>
        /// <param name="accessToken"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        private static DataSet GetDataFromAzureSql(ConfigurationModel configurationModel, MessageModel messageModel, string accessToken, ILogger log)
        {
            DataSet dtpublishMetadta = new DataSet();
            log.LogInformation("In GetDataFromsql method");
            //Define retryStrategy to retry for transientError while connecting to Azure SQL
            var retryStrategy = new Incremental(configurationModel.RetryCount, configurationModel.IntervalTime, configurationModel.IncrementTime);
            var retryPolicy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(retryStrategy);
            retryPolicy.Retrying += (sender, args) =>
            {
                // Log details of the retry.
                log.LogInformation("Retry - Count:{0}" + args.CurrentRetryCount + "," + "Delay:{1}" + args.Delay + "Exception:{2}" + args.LastException.Message);
            };
            try
            {
                dtpublishMetadta = retryPolicy.ExecuteAction(() =>
                {
                    using (SqlConnection sqlConnection = new SqlConnection(configurationModel.connStrAzure))
                    {
                        sqlConnection.AccessToken = accessToken;
                        sqlConnection.Open();
                        log.LogInformation("open sql connection success");
                        SqlCommand sqlCommand = new SqlCommand(configurationModel.StoredProcedureName, sqlConnection);
                        sqlCommand.Parameters.AddWithValue(Constants.SP_APP_ID, messageModel.app_id);
                        sqlCommand.Parameters.AddWithValue(Constants.SP_R_OBJECT_ID, messageModel.r_object_id);
                        sqlCommand.Parameters.AddWithValue(Constants.SP_EVEN_TYPE, messageModel.event_Type);
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
                        sqlDataAdapter.SelectCommand = sqlCommand;
                        //Rename the tables
                        sqlDataAdapter.TableMappings.Add("Table", Constants.TARGET_TABLE);
                        sqlDataAdapter.TableMappings.Add("Table1", Constants.S_TABLE);
                        sqlDataAdapter.TableMappings.Add("Table2", Constants.R_TABLE);
                        sqlDataAdapter.TableMappings.Add("Table3", Constants.FILEPATH_TABLE);

                        sqlDataAdapter.Fill(dtpublishMetadta);
                        log.LogInformation($"Target table count : {dtpublishMetadta.Tables[0].Rows.Count}");
                        log.LogInformation($"S table count : {dtpublishMetadta.Tables[1].Rows.Count}");
                        log.LogInformation($"R table count : {dtpublishMetadta.Tables[2].Rows.Count}");
                        log.LogInformation($"Filepath table count : {dtpublishMetadta.Tables[3].Rows.Count}");
                        sqlConnection.Close();
                    }
                    log.LogInformation($"GetDataFromAzuresql method returns tables,table count : {dtpublishMetadta.Tables.Count}");
                    return dtpublishMetadta;
                });

                return dtpublishMetadta;

            }
            catch (Exception ex)
            {
                log.LogError($"Exception occurred in GetDataFromAzuresql method, Error : {ex.Message},Details:{ex.InnerException}");
                return dtpublishMetadta = null;
                throw ex;
            }
            finally
            {
                SqlConnection sqlConnection = new SqlConnection(configurationModel.connStrAzure);
                sqlConnection.Close();
                log.LogInformation("Out GetDataFromsql method");
            }
        }

    }
}


