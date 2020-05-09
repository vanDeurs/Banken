using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Runtime.Serialization;

namespace Server
{
    [DataContract]
    abstract class Account : ITransaction
    {
        [DataMember]
        protected Balance balance;
        [DataMember]
        protected int accountNumber;
        [DataMember]
        protected string accountName;

        public Account (string accountName)
        {
            this.balance = new Balance(0, NumberFormatInfo.CurrentInfo.CurrencySymbol);
            this.accountNumber = CreateAccountNumber();
            this.accountName = accountName;
        }
        public string AccountName
        {
            get { return this.accountName; }
        }

        public bool TransferFunds(int accountNumber, decimal funds)
        {
            return true;
        }

        int CreateAccountNumber ()
        {
            var chars = "1234567890";
            var stringChars = new char[8];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars);
            return int.Parse(finalString);
        }
    }
}
