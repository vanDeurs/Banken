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
            for (int j = 0; j < accounts.Count; j++)
            {
                if (accounts[j].Number == int.Parse(number))
                {
                    return accounts[j];
                }
            }
            throw new ArgumentOutOfRangeException(nameof(name), $"Name {name} is not found.");
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
    }
}
