using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureBilling.Models
{
    public class Properties
    {
        public string SubscriptionId { get; set; }
        public string UsageStartTime { get; set; }
        public string UsageEndTime { get; set; }
        public string MeterId { get; set; }
        public InfoFields InfoFields { get; set; }

        [JsonProperty("InstanceData")]
        public string InstanceDataRaw { get; set; }

        [JsonProperty("InstanceDataDeserialized")]
        public InstanceDataType InstanceData
        {
            get
            {
                return JsonConvert.DeserializeObject<InstanceDataType>(InstanceDataRaw.Replace("\\\"", ""));
            }
        }
        public double Quantity { get; set; }
        public string Unit { get; set; }
        public string MeterName { get; set; }
        public string MeterCategory { get; set; }
        public string MeterSubCategory { get; set; }
        public string MeterRegion { get; set; }
    }

    public class InstanceDataType
    {
        [JsonProperty("Microsoft.Resources")]
        public MicrosoftResourcesDataType MicrosoftResources { get; set; }
    }

    public class MicrosoftResourcesDataType
    {
        public string ResourceUri { get; set; }

        public Dictionary<string, string> ResourceList {
            get
            {
                return SplitResourceUri(ResourceUri);
            }
        }

        public IDictionary<string, string> Tags { get; set; }

        public IDictionary<string, string> AdditionalInfo { get; set; }

        public string Location { get; set; }

        public string PartNumber { get; set; }

        public string OrderNumber { get; set; }

        private static Dictionary<string, string> SplitResourceUri(string resourceUri)
        {
            Dictionary<string, string> resourceList = new Dictionary<string, string>();

            // Remove forward slash if at start
            if (resourceUri.StartsWith("/"))
            {
                resourceUri = resourceUri.Remove(0, 1);
            }

            string[] items = resourceUri.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries).Where((i, index) => (index & 1) == 0).ToArray();
            string[] values = resourceUri.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries).Where((i, index) => (index & 1) == 1).ToArray();

            int resourceIndex = 0;

            foreach (string item in items)
            {
                resourceList.Add(item, values[resourceIndex]);

                resourceIndex++;
            }

            return resourceList;
        }
    }
}
