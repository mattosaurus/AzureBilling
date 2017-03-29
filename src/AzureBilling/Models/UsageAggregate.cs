using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureBilling.Models
{
    public class UsageAggregate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public Properties Properties { get; set; }
    }
}
