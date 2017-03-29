using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureBilling.Models
{
    public class ResourceDateItem
    {
        public DateTime UsageStartTime { get; set; }

        public DateTime UsageEndTime { get; set; }

        public double Quantity { get; set; }

        private double cost;
        public double Cost {
            get
            {
                return Math.Round(cost, 2);
            }

            set
            {
                cost = value;
            }
        }
    }
}
