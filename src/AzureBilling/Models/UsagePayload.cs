using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureBilling.Models
{
    public class UsagePayload
    {
        public List<UsageAggregate> Value { get; set; }
    }
}
