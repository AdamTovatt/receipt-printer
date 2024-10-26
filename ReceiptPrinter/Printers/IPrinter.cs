namespace ReceiptPrinter.Printers
{
    public interface IPrinter
    {
        public abstract Task PrintAsync(Receipt receipt);
    }
}
