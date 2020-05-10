using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    interface ITransaction
    {
        void TakeOutFunds(decimal funds);
        void AddFunds(decimal funds);
    }
}
