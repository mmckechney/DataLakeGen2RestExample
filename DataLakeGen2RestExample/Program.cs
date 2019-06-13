using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace DataLakeGen2RestExample
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        static async Task Main(string[] args)
        {
            HttpResponseMessage response;
            // *** Set up your Azure Active Directory Application as per the instructions here: 
            //https://docs.microsoft.com/en-us/previous-versions/azure/storage/data-lake-storage-rest-api-guide
            
            //Get your AAD Tenant ID, App ID and App Secret to use to aquire token from the app you set up above
            var applicationId = ConfigurationManager.AppSettings["applicationId"];
            var secretKey = ConfigurationManager.AppSettings["secretKey"];
            var tenantId = ConfigurationManager.AppSettings["tenantId"];

            //Variables for the storage account, file system and path that will be created in this sample
            string storageAccountName = "adlsg2mwm";
            string filesystem = "datafilesystem";
            string path = "DataPath";

            //Create the POST body for the Auth token generation
            var values = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", applicationId },
                { "client_secret", secretKey },
                { "resource", "https://storage.azure.com" },
                { "scope", "https://storage.azure.com/.default" }

            };

            //POST to get the access token to the Data Lake
            var content = new FormUrlEncodedContent(values);
            var authResponse = await client.PostAsync($"https://login.microsoftonline.com/{tenantId}/oauth2/token", content);
            var authString = await authResponse.Content.ReadAsStringAsync();
            var json = JObject.Parse(authString);

            //Add the Bearer token and API version to the HttpClient -- these will be used in all calls
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", json["access_token"].ToString());
            client.DefaultRequestHeaders.Add("x-ms-version", "2018-11-09");

            //This call will create an Azure Datalake Gen2 file system
            //See: https://docs.microsoft.com/en-us/rest/api/storageservices/datalakestoragegen2/filesystem
            var resourceUrl = $"https://{storageAccountName}.dfs.core.windows.net/{filesystem}?resource=filesystem";
            response = await client.PutAsync(resourceUrl, null);
            Console.WriteLine($"------\r\nCreate Filesystem '{filesystem}' response: {response.StatusCode} -- {response.ReasonPhrase}");

            //This call will create a path within a filesystem  
            //See: https://docs.microsoft.com/en-us/rest/api/storageservices/datalakestoragegen2/path
            resourceUrl = $"https://{storageAccountName}.dfs.core.windows.net/{filesystem}/{path}?resource=directory";
            response = await client.PutAsync(resourceUrl, null);
            Console.WriteLine($"------\r\nCreate Path '{path}' response: {response.StatusCode} -- {response.ReasonPhrase}");

            //This call will create an empty file within a filesystem  
            //See: https://docs.microsoft.com/en-us/rest/api/storageservices/datalakestoragegen2/path
            string tmpFile = Path.GetTempFileName();
            string fileName = HttpUtility.UrlEncode(Path.GetFileName(tmpFile));
            resourceUrl = $"https://{storageAccountName}.dfs.core.windows.net/{filesystem}/{path}/{fileName}?resource=file";
            response = await client.PutAsync(resourceUrl, null);
            Console.WriteLine($"------\r\nCreate File '{fileName}' response: {response.StatusCode} -- {response.ReasonPhrase}");

            //This call will upload a file within a filesystem  
            //See: https://docs.microsoft.com/en-us/rest/api/storageservices/datalakestoragegen2/path
            File.WriteAllText(tmpFile, $"this is sample file content for {tmpFile}");
            using (var formContent = new StreamContent(new FileStream(tmpFile, FileMode.Open, FileAccess.Read)))
            {
                //upload to the file buffer
                resourceUrl = $"https://{storageAccountName}.dfs.core.windows.net/{filesystem}/{path}/{fileName}?action=append&timeout=1000&position=0";
                HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Patch, resourceUrl);
                msg.Content = formContent;
                msg.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                response = await client.SendAsync(msg);
                Console.WriteLine($"------\r\nUpload File '{fileName}' response: {response.StatusCode} -- {response.ReasonPhrase}");

                //flush the buffer to commit the file
                var flushUrl = $"https://{storageAccountName}.dfs.core.windows.net/{filesystem}/{path}/{fileName}?action=flush&position={msg.Content.Headers.ContentLength}";
                HttpRequestMessage flushMsg = new HttpRequestMessage(HttpMethod.Patch, flushUrl);
                response = await client.SendAsync(flushMsg);
                Console.WriteLine($"------\r\nBuffer flush '{fileName}' response: {response.StatusCode} -- {response.ReasonPhrase}");
            }
                       
            try
            {
                File.Delete(tmpFile);
            }
            catch { }
            Console.WriteLine("Press any key to close...");
            Console.ReadLine();

        }



    }
}
