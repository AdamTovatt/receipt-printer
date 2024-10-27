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

            int cpi = CalculateCpiFromFontSize(receipt.Config.FontSize);
            int lpi = CalculateLpiFromCpi(cpi);
            int maxWidth = CalculateMaxWidth(receipt.Config.PageWidthInMillimeters, cpi);

            string writeContent = new FormattedText(receipt.TextContent).ApplyMaxWidth(maxWidth);
            File.WriteAllText($"{receipt.FileName}.txt", ApplyBottomMargin(receipt, writeContent));

            try
            {
                Command printCommand = new Command($"lp -d {printerName} -o lpi={lpi} -o cpi={cpi} {receipt.FileName}.txt", workingDirectory);
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

            return $"{text}{new string('\n', receipt.Config.BottomMargin)}{receipt.Config.BottomDecoration}";
        }

        private int CalculateMaxWidth(float pageWidthInMillimeters, int cpi)
        {
            double pageWidthInInches = pageWidthInMillimeters / 25.4;
            return (int)Math.Round(pageWidthInInches * cpi);
        }

        private int CalculateCpiFromFontSize(int fontSize)
        {
            return Math.Clamp(24 - fontSize, 4, 23); // a font size of 10 shuold yield cpi 14. A larger font size will yield lower cpi since that means more charaters per inch
        }

        private int CalculateLpiFromCpi(int cpi)
        {
            return (int)Math.Round(cpi / 1.75); // the characters should be about 1.75 as tall as wide
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
