namespace ReceiptPrinter.Printers
{
    public class LinuxPrinter : IPrinter
    {
        private ILogger logger;

        private string workingDirectory;
        private string? _printerName = null;

        public LinuxPrinter(ILogger logger)
        {
            workingDirectory = Environment.CurrentDirectory;
            this.logger = logger;
        }

        public async Task PrintAsync(Receipt receipt)
        {
            string printerName = await GetPrinterNameAsync();

            receipt.GenerateTxtFile(26);

            Command printCommand = new Command($"lp -d {printerName} -o lpi=8 -o cpi=14 {receipt.FileName}.txt", workingDirectory);

            await printCommand.RunAsync();

            receipt.RemoveTextFile();
        }

        private async Task<string> GetPrinterNameAsync()
        {
            if (_printerName == null)
            {
                Command getNameCommand = new Command("lpstat -d", workingDirectory);
                _printerName = (await getNameCommand.RunAsync()).Split(':').Last().Trim();
            }

            return _printerName;
        }
    }
}
