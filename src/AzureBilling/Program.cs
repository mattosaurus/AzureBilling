using AzureBilling.Helpers;
using AzureBilling.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AzureBilling
{
    public class Program
    {
        // This is the ID of the client being used to access the AAD, it can be found as Application ID under Azure Active Directory > App registrations > YOUR_APP_NAME
        // This app is created as part of the process in https://github.com/Azure-Samples/billing-dotnet-ratecard-api
        private static string clientId = "";
        // This is the key for the client ID above, it can be found as Application ID under Azure Active Directory > App registrations > YOUR_APP_NAME > Keys
        // This key is only visible when first created so make sure you note it down then!
        private static string clientSecret = "";
        // This is the ID of the AAD to be accessed, it can be found as Directory ID under Properties in the Azure Active Directory Blade in the portal
        private static string tennantId = "";
        private static string resource = "https://management.core.windows.net/";
        private static string authority = "https://login.windows.net";
        private static string billingService = "https://management.azure.com";
        // This is the ID of the subscription that you want to view billing information for, it can be found in the Subscriptions blade of the portal
        private static string subscriptionId = "";

        public static void Main(string[] args)
        {
            // Get the last months billing data
            DateTime startDateTime = DateTime.Now.Date.AddMonths(-1);
            DateTime endDateTime = DateTime.Now.Date;

            // Get AAD token
            string token = GetOAuthTokenFromAAD(clientId, clientSecret, tennantId, resource, authority).Result;

            // Get rate card
            RateCardPayload rateCardPayload = GetRateCard(token, billingService, subscriptionId).Result;

            // Get usage
            UsagePayload usagePayload = GetUsage(token, billingService, subscriptionId, startDateTime, endDateTime).Result;

            // Create aggregates by resource group
            List<ResourceGroupAggregate> resourceGroupAggregates = CreateResourceGroupAggregates(usagePayload, rateCardPayload);

            // Write headers to console
            Console.WriteLine("Resource Group\tDate\tCost");

            // Write daily output to console
            foreach (ResourceGroupAggregate resourceGroupAggregate in resourceGroupAggregates)
            {
                foreach (DateTime date in resourceGroupAggregate.ResourceItems.SelectMany(x => x.ResourceDateItems.Select(y => y.UsageStartTime)).Distinct().ToList())
                {
                    Console.WriteLine(resourceGroupAggregate.ResourceGroupName + "\t" + date.ToString("dd/MM/yyyy") + "\t" + resourceGroupAggregate.ResourceItems.SelectMany(x => x.ResourceDateItems.Where(y => y.UsageStartTime == date)).Sum(z => z.Cost).ToString("F"));
                }
            }

            Console.ReadLine();
        }

        public static async Task<string> GetOAuthTokenFromAAD(string clientId, string clientSecret, string tenantId, string resource, string authority)
        {
            ClientCredential clientCredential = new ClientCredential(clientId, clientSecret);

            AuthenticationContext authenticationContext = new AuthenticationContext(String.Format("{0}/{1}", authority, tenantId));

            AuthenticationResult result = await authenticationContext.AcquireTokenAsync(resource, clientCredential);

            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }

            return result.AccessToken;
        }

        public static async Task<RateCardPayload> GetRateCard(string token, string billingService, string subscriptionId)
        {
            /*Setup API call to RateCard API
             Callouts:
             * See the App.config file for all AppSettings key/value pairs
             * You can get a list of offer numbers from this URL: http://azure.microsoft.com/en-us/support/legal/offer-details/
             * You can configure an OfferID for this API by updating 'MS-AZR-{Offer Number}'
             * The RateCard Service/API is currently in preview; please use "2015-06-01-preview" or "2016-08-31-preview" for api-version (see https://msdn.microsoft.com/en-us/library/azure/mt219005 for details)
             * Please see the readme if you are having problems configuring or authenticating: https://github.com/Azure-Samples/billing-dotnet-ratecard-api
             */
            // Build up the HttpWebRequest
            string requestURL = String.Format("{0}/{1}/{2}/{3}",
                       billingService,
                       "subscriptions",
                       subscriptionId,
                       "providers/Microsoft.Commerce/RateCard?api-version=2016-08-31-preview&$filter=OfferDurableId eq 'MS-AZR-0063P' and Currency eq 'GBP' and Locale eq 'en-GB' and RegionInfo eq 'GB'");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestURL);

            // Add the OAuth Authorization header, and Content Type header
            request.Headers["Authorization"] = "Bearer " + token;
            request.ContentType = "application/json";

            RateCardPayload payload = new RateCardPayload();

            // Call the RateCard API
            try
            {
                // Call the REST endpoint
                WebResponse response = await request.GetResponseAsync();
                Stream receiveStream = response.GetResponseStream();

                // Pipes the stream to a higher level stream reader with the required encoding format. 
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                var rateCardResponse = readStream.ReadToEnd();

                // Convert the Stream to a strongly typed RateCardPayload object.  
                // You can also walk through this object to manipulate the individuals member objects. 
                payload = JsonConvert.DeserializeObject<RateCardPayload>(rateCardResponse);
                response.Dispose();
                readStream.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("{0} \n\n{1}", e.Message, e.InnerException != null ? e.InnerException.Message : ""));
                Console.ReadLine();
            }

            return payload;
        }

        public static async Task<UsagePayload> GetUsage(string token, string billingService, string subscriptionId, DateTime startDateTime, DateTime endDateTime)
        {

            /*Setup API call to Usage API
             Callouts:
             * See the App.config file for all AppSettings key/value pairs
             * You can get a list of offer numbers from this URL: http://azure.microsoft.com/en-us/support/legal/offer-details/
             * See the Azure Usage API specification for more details on the query parameters for this API.
             * The Usage Service/API is currently in preview; please use 2016-06-01-preview for api-version
            */

            // Build up the HttpWebRequest
            string requestURL = String.Format("{0}/{1}/{2}/{3}",
                       billingService,
                       "subscriptions",
                       subscriptionId,
                       "providers/Microsoft.Commerce/UsageAggregates?api-version=2015-06-01-preview&reportedStartTime=" + WebUtility.HtmlEncode(startDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ")) + "&reportedEndTime=" + endDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ"));

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestURL);

            // Add the OAuth Authorization header, and Content Type header
            request.Headers["Authorization"] = "Bearer " + token;
            request.ContentType = "application/json";

            UsagePayload payload = new UsagePayload();

            // Call the Usage API, dump the output to the console window
            try
            {
                // Call the REST endpoint
                WebResponse response = await request.GetResponseAsync();
                Stream receiveStream = response.GetResponseStream();

                // Pipes the stream to a higher level stream reader with the required encoding format. 
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                var usageResponse = readStream.ReadToEnd();

                // Resolve camelcase from response to model
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.ContractResolver = new CamelcaseContractResolver();
                settings.NullValueHandling = NullValueHandling.Ignore;

                // Convert the Stream to a strongly typed UsagePayload object.  
                // You can also walk through this object to manipulate the individuals member objects. 
                payload = JsonConvert.DeserializeObject<UsagePayload>(usageResponse, settings);
                response.Dispose();
                readStream.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("{0} \n\n{1}", e.Message, e.InnerException != null ? e.InnerException.Message : ""));
                Console.ReadLine();
            }

            return payload;
        }

        public static List<ResourceGroupAggregate> CreateResourceGroupAggregates(UsagePayload usagePayload, RateCardPayload rateCardPayload)
        {
            List<ResourceGroupAggregate> resourceGroupAggregates = new List<ResourceGroupAggregate>();

            // Get distinct resource groups
            foreach (UsageAggregate usage in usagePayload.Value)
            {
                if (usage.Properties.InstanceData.MicrosoftResources.ResourceList.ContainsKey("resourceGroups"))
                {
                    // Get resource group
                    ResourceGroupAggregate resourceGroupAggregate = resourceGroupAggregates.SingleOrDefault(x => x.ResourceGroupName.ToLower() == usage.Properties.InstanceData.MicrosoftResources.ResourceList["resourceGroups"].ToLower());

                    // Add resource group if it doesn't already exist
                    if (resourceGroupAggregate == null)
                    {
                        resourceGroupAggregate = new ResourceGroupAggregate();
                        resourceGroupAggregate.ResourceGroupName = usage.Properties.InstanceData.MicrosoftResources.ResourceList["resourceGroups"];
                        resourceGroupAggregates.Add(resourceGroupAggregate);
                        resourceGroupAggregate = resourceGroupAggregates.SingleOrDefault(x => x.ResourceGroupName == usage.Properties.InstanceData.MicrosoftResources.ResourceList["resourceGroups"]);
                    }

                    // Get resource item
                    ResourceItem resourceItem = resourceGroupAggregate.ResourceItems.SingleOrDefault(x => x.MeterName == usage.Properties.MeterName);

                    // Add resource item if it doesn't already exist
                    if (resourceItem == null)
                    {
                        resourceItem = new ResourceItem();
                        resourceItem.MeterCategory = usage.Properties.MeterCategory;
                        resourceItem.MeterName = usage.Properties.MeterName;
                        resourceGroupAggregate.ResourceItems.Add(resourceItem);
                        resourceItem = resourceGroupAggregate.ResourceItems.SingleOrDefault(x => x.MeterName == usage.Properties.MeterName);
                    }

                    ResourceDateItem resourceDateItem = new ResourceDateItem();
                    resourceDateItem.Quantity = usage.Properties.Quantity;
                    resourceDateItem.Cost = rateCardPayload.Meters.Where(x => x.MeterId == usage.Properties.MeterId).FirstOrDefault().MeterRates[0] * usage.Properties.Quantity;
                    resourceDateItem.UsageStartTime = DateTime.Parse(usage.Properties.UsageStartTime);
                    resourceDateItem.UsageEndTime = DateTime.Parse(usage.Properties.UsageEndTime);

                    resourceItem.ResourceDateItems.Add(resourceDateItem);
                }
            }

            return resourceGroupAggregates;
        }
    }
}
