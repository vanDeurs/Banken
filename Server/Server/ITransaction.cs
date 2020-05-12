namespace Server
{
    interface ITransaction
    {
        void TakeOutFunds(decimal funds);
        void AddFunds(decimal funds);
    }
}
