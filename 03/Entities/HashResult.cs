using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _03.Entities
{
    public class HashResult
    {
        public string Hash { get; set; }
        public byte[] Salt { get; set; }
    }
}
