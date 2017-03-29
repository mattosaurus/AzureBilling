using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureBilling.Models
{
    public class InfoFields
    {
        public string MeteredRegion { get; set; }
        public string MeteredService { get; set; }
        public string Project { get; set; }
        public string MeteredServiceType { get; set; }
        public string ServiceInfo1 { get; set; }
    }
}
