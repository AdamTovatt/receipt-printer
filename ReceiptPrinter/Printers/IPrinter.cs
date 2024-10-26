namespace ReceiptPrinter.Printers
{
    public interface IPrinter
    {
        public abstract void Print(Receipt receipt);
    }
}
