using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Server
{
    [DataContract]
    sealed class CardAccount : Account
    {
        [DataMember]
        readonly static double interest = 1.03;
        public CardAccount(string accountName) : base(accountName)
        {
        }
    }
}
