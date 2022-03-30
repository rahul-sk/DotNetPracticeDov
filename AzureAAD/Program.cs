using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Azure.Management.Monitor;
using System.Net;
using System.IO;

namespace AzureAAD
{
    class Program
    {
        public static string clientId = "de9a7e8f-e357-4adb-8b46-01afc3c5f947";
        public static string clientSecret = "CmEfgm4LJ6omGQ~r1_8Yj7bwGgVPZ0dJpu";
        public static string tenantId = "3d2d2b6f-061a-48b6-b4b3-9312d687e3a1";
        /* Azure free pass */
        //public static string SubscriptionId = "dabff70e-a8ac-40a3-a275-83675fc1702a";
        public static string SubscriptionId = "dabff70e-a8ac-40a3-a275-83675fc1702a";

        string newur = "https://management.azure.com/subscriptions/b324c52b-4073-4807-93af-e07d289c093e/"+
            "resourceGroups/test/providers/Microsoft.Storage/storageAccounts/rahulazure/blobServices/default/providers/" +
            "Microsoft.Insights/metrics?timespan=2021-04-14T02:20:00Z/2021-04-14T04:20:00Z&interval=PT1M&" +
 "aggregation=Average,count&top=3&orderby=Average asc&$filter=BlobType eq '*'&api-version=2018-01-01&metricnamespace=Microsoft.Azure.Cosmos";

        
        public static string Location="eastus";
        public static string ResourceGroup="RahulRG";
        public static string DocumentDBAccountName = "rahulazure";
        public static string AccessToken { get; set; }
        public static string Response { get; set; }



        static void Main(string[] args)
        {
            AccessToken = GetAuthCode();
            getAllResourceGroupDetails().GetAwaiter().GetResult();
            Console.WriteLine(Response);
          }

        private static string GetAuthCode()
        {
            ClientCredential cc = new ClientCredential(clientId, clientSecret);
            var context = new AuthenticationContext("https://login.microsoftonline.com/" + tenantId);
            var result = context.AcquireTokenAsync("https://management.azure.com/", cc);
            if (result == null)
            {
                throw new InvalidOperationException("Failed to get access token");
            }
            return result.Result.AccessToken;
        }

        public static async Task getAllResourceGroupDetails()
        {
            HttpClient client = new HttpClient();
            //client.BaseAddress = new Uri("https://management.azure.com/subscriptions/" + SubscriptionId +
            //    "/resourcegroups?api-version=2019-10-01");

            //"https://management.azure.com/subscriptions/"+{SubscriptionId}+"/resourceGroups/"+{ResourceGroup}+"/providers/Microsoft.DocumentDb/databaseAccounts/"+{DocumentDBAccountName}+"/providers/microsoft.insights/metricDefinitions?api-version=2018-01-01"

            // this is the important one
            client.BaseAddress = new Uri("https://management.azure.com/subscriptions/" + SubscriptionId + "/resourceGroups/" + ResourceGroup
            + "/providers/Microsoft.DocumentDb/databaseAccounts/" + DocumentDBAccountName
            + "/providers/microsoft.insights/metricDefinitions?api-version=2018-01-01");

            //           client.BaseAddress = new Uri("https://management.azure.com/subscriptions/dabff70e-a8ac-40a3-a275-83675fc1702a/" +
            //           "resourceGroups/RahulRG/providers/Microsoft.DocumentDb/databaseAccounts/rahulazure/" +
            //           "Microsoft.Insights/metrics?timespan=2021-04-14T02:20:00Z/2021-04-14T04:20:00Z&interval=PT1M&" +
            //"aggregation=Average,count&top=3&api-version=2018-01-01&metricnamespace=microsoft.documentdb/databaseaccounts");


            //client.BaseAddress = new Uri("https://management.azure.com/subscriptions/dabff70e-a8ac-40a3-a275-83675fc1702a/resourceGroups/RahulRG/providers/Microsoft.DocumentDb/databaseAccounts/rahulazure/Microsoft.Insights/metrics?metricname=TotalRequests&api-version=2018-01-01");







            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + AccessToken);
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, client.BaseAddress);
            var res = await MakeRequestAsync(req, client);
            Response = res;

        }

        public string Get(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
        private static async Task<string> MakeRequestAsync(HttpRequestMessage req, HttpClient client)
        {
            var res = await client.SendAsync(req).ConfigureAwait(false);
            var responseString = string.Empty;
            try
            {
                res.EnsureSuccessStatusCode();
                responseString = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

            }catch(Exception e)
            {

            }
            return responseString;
        }
    }
}
