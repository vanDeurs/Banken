using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Server
{
    [DataContract]
    class User
    {
        [DataMember]
        List<Account> accounts = new List<Account>();
        [DataMember]
        string name;
        [DataMember]
        string socialSecurityNumber;

        public User (string name, string socialSecurityNumber)
        {
            this.name = name;
            this.socialSecurityNumber = socialSecurityNumber;
            this.accounts.Add(new SavingsAccount("Savings Account"));
        }

        public List<Account> Accounts
        {
            get { return accounts; }
        }


        public Account this[string number] => FindAccountByNumber(number);

        private Account FindAccountByNumber(string number)
        {
            for (int i = 0; i < accounts.Count; i++)
            {
                if (accounts[i].Number == int.Parse(number))
                {
                    return accounts[i];
                }
            }
            throw new ArgumentOutOfRangeException(nameof(number), $"Number {number} is not found.");
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }
        public string SocialSecurityNumber
        {
            get
            {
                return this.socialSecurityNumber;
            }
        }

        public void DeleteAccount (string accountNumber)
        {
            for (int i = 0; i < accounts.Count; i++)
            {
                if (accounts[i].Number == int.Parse(accountNumber))
                {
                    accounts.RemoveAt(i);
                }
            }
        }
        public void CreateAccount(string accountType, string name)
        {
           if (accountType == "SavingsAccount")
            {
                SavingsAccount newAccount = new SavingsAccount(name);
                accounts.Add(newAccount);
            } else if (accountType == "CardAccount")
            {
                CardAccount newAccount = new CardAccount(name);
                accounts.Add(newAccount);
            } else
            {
                throw new Exception("Account type is faulty.");
            }

        }
    }
}
