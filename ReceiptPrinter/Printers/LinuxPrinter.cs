namespace ReceiptPrinter.Printers
{
    public class LinuxPrinter : IPrinter
    {
        private string workingDirectory;
        private string? _printerName = null;

        public LinuxPrinter()
        {
            workingDirectory = Environment.CurrentDirectory;
        }

        public void Print(Receipt receipt)
        {
            string printerName = GetPrinterName();

            receipt.GenerateTxtFile();

            Command printCommand = new Command($"lp -d {printerName} {receipt.FileName}.txt", workingDirectory);

            printCommand.RunAsync().Wait();

            receipt.RemoveTextFile();
        }

        private string GetPrinterName()
        {
            if (_printerName == null)
            {
                Command getNameCommand = new Command("lpstat -d", workingDirectory);
                _printerName = getNameCommand.RunAsync().Result;
            }

            return _printerName;
        }
    }
}
