using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace Server
{
    public struct Balance
    {
        public decimal amount;
        public string currency;

        public Balance (decimal amount, string currency)
        {
            this.amount = amount;
            this.currency = currency;
        }
        public override string ToString() => $"({amount} {currency})";
    }
}
