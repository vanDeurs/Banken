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
        public override string ToString() => $"{amount} {currency}";
    }
}
