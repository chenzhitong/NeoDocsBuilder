using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchEngine
{
    public class MessageSenderOptions
    {
        public string FromAddress { get; set; }
        public string FromDisplayName { get; set; }
        public string EmailPassword { get; set; }
        public string ToAddress { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }
}
