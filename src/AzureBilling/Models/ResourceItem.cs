using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureBilling.Models
{
    public class ResourceItem
    {
        public ResourceItem()
        {
            ResourceDateItems = new List<ResourceDateItem>();
        }

        public string MeterName { get; set; }

        public string MeterCategory { get; set; }

        public string Unit { get; set; }

        public double Quantity
        {
            get
            {
                return ResourceDateItems.Sum(x => x.Quantity);
            }
        }

        public double Cost
        {
            get
            {
                return Math.Round(ResourceDateItems.Sum(x => x.Cost), 2);
            }
        }

        public List<ResourceDateItem> ResourceDateItems { get; set; }
    }
}
