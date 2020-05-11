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
        protected int number;
        [DataMember]
        protected string name;

        public Account (string accountName)
        {
            this.balance = new Balance(100, NumberFormatInfo.CurrentInfo.CurrencySymbol);
            this.number = CreateAccountNumber();
            this.name = accountName;
        }
        public string Name
        {
            get { return this.name; }
        }
        public Balance Balance
        {
            get { return this.balance; }
        }
        public int Number
        {
            get { return this.number; }
        }

        public void AddFunds(decimal funds)
        {
            this.balance.amount += funds;
        }

        public void TakeOutFunds(decimal funds)
        {
            if (funds > this.balance.amount)
            {
                throw new Exception("This take out would overdraw the account. STOP.");
            }
            decimal newBalance = this.balance.amount - funds;
            this.balance.amount = newBalance;
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
