using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Server
{
    [DataContract]
    sealed class SavingsAccount : Account
    {
        [DataMember]
        readonly static double interest = 1.05;
        public SavingsAccount(string accountName) : base(accountName)
        {
        }
    }
}
