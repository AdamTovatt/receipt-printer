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
            logger.LogInformation("Will print");

            string printerName = await GetPrinterNameAsync();

            receipt.GenerateTxtFile();

            Command printCommand = new Command($"lp -d {printerName} {receipt.FileName}.txt", workingDirectory);

            await printCommand.RunAsync();

            receipt.RemoveTextFile();
        }

        private async Task<string> GetPrinterNameAsync()
        {
            if (_printerName == null)
            {
                logger.LogInformation("Starting command for printer name in: " + workingDirectory);
                Command getNameCommand = new Command("lpstat -d", workingDirectory);
                _printerName = (await getNameCommand.RunAsync()).Split(':').Last().Trim();
                logger.LogInformation(_printerName);
            }

            return _printerName;
        }
    }
}
