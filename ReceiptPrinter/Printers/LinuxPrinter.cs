using QuestPDF.Helpers;

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

            string writeContent = new FormattedText(receipt.TextContent).ApplyMaxWidth(receipt.Config.MaxWidth);
            File.WriteAllText($"{receipt.FileName}.txt", ApplyBottomMargin(receipt, writeContent));

            try
            {
                Command printCommand = new Command($"lp -d {printerName} -o lpi={receipt.Config.Lpi} -o cpi={receipt.Config.Cpi} {receipt.FileName}.txt", workingDirectory);
                await printCommand.RunAsync();
            }
            catch { throw; }
            finally
            {
                File.Delete($"{receipt.FileName}.txt"); // always clean up the file
            }
        }

        private string ApplyBottomMargin(Receipt receipt, string text)
        {
            if (receipt.Config.BottomMargin == 0)
                return text;

            string bottomDecoration = receipt.Config.BottomDecoration.LoopToLength(receipt.Config.MaxWidth);

            return $"{text}{new string('\n', receipt.Config.BottomMargin)}{bottomDecoration}";
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
