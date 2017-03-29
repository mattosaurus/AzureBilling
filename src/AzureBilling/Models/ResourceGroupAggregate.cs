using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureBilling.Models
{
    public class ResourceGroupAggregate
    {
        public ResourceGroupAggregate()
        {
            ResourceItems = new List<ResourceItem>();
        }

        public string ResourceGroupName { get; set; }

        public double Cost
        {
            get
            {
                return Math.Round(ResourceItems.Sum(x => x.Cost), 2);
            }
        }

        public List<ResourceItem> ResourceItems { get; set; }
    }
}
