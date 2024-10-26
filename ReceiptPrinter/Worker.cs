using ReceiptPrinter.ZettleClasses;
using System.Text;

namespace ReceiptPrinter
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        private readonly PurchaseManager purchaseManager;
        private readonly PrintingManager printingManager;
        private readonly ReceiptConfig receiptConfig;

        private int refreshDelay = 5000;

        public Worker(ILogger<Worker> logger, ZettleConfig zettleConfig, ReceiptConfig receiptConfig)
        {
            this.logger = logger;
            purchaseManager = new PurchaseManager(zettleConfig, receiptConfig);
            printingManager = new PrintingManager(logger);
            this.receiptConfig = receiptConfig;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            bool hasHadAnError = false;

            try
            {
                bool initialized = await InitializeAsync();

                while (!initialized)
                {
                    await Task.Delay(1000);
                    initialized = await InitializeAsync();
                }

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        foreach (Purchase purchase in await purchaseManager.GetNewPurchasesAsync())
                        {
                            Receipt receipt = new Receipt(purchase);
                            await printingManager.PrintAsync(receipt);
                        }

                        if (hasHadAnError)
                            await printingManager.PrintAsync(Receipt.CreateReceiptFromText("success-message", "The system is back online"));

                        printingManager.RemovePrints("start-error-info"); // remove error message prints
                        printingManager.RemovePrints("error-message"); // so they will be printed again if a new error occurrs
                        printingManager.RemovePrints("success-message");
                        hasHadAnError = false;

                        await Task.Delay(refreshDelay, stoppingToken);
                    }
                    catch (Exception exception)
                    {
                        hasHadAnError = true;
                        await printingManager.PrintAsync(Receipt.CreateReceiptFromText("error-message", $"An error has occurred, the system might be offline: {exception.Message}"));
                    }
                }
            }
            catch (Exception exception)
            {
                await printingManager.PrintAsync(Receipt.CreateReceiptFromText("fatal-error-message", $"A fatal error has occurred and the system needs to be restarted: {exception.Message}"));
            }
        }

        protected async Task<bool> InitializeAsync()
        {
            string? errorMessage = null;

            try
            {
                await purchaseManager.InitializeAsync();
            }
            catch (Exception exception)
            {
                errorMessage = exception.Message;
            }

            await printingManager.PrintAsync(CreateStartupReceipt(errorMessage));

            return errorMessage == null;
        }

        private Receipt CreateStartupReceipt(string? errorMessage)
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (errorMessage == null)
            {
                DateTime time = DateTime.Now;

                stringBuilder.AppendLine("ReceiptPrinter service successfully started and initialized.");
                stringBuilder.AppendLine($"{time.ToString("yyyy-MM-dd")} {time.ToShortTimeString()}");
                stringBuilder.AppendLine($"RefreshDelay: {refreshDelay} ms");

                if (receiptConfig.AllowedCategories == null)
                {
                    stringBuilder.AppendLine("No category filter is specified, will print receipts for all purchase categories.");
                }
                else
                {
                    stringBuilder.AppendLine("Category filter is specified, will only print receipts for the following categories:");
                    foreach (string category in receiptConfig.AllowedCategories)
                    {
                        stringBuilder.AppendLine($"- {category}");
                    }
                }

                stringBuilder.AppendLine("Have a nice day! :)");
            }
            else
            {
                stringBuilder.AppendLine("ReceiptPrinter service failed to initialize");
                stringBuilder.AppendLine(DateTime.Now.ToString());
                stringBuilder.AppendLine($"Error:\n{errorMessage}");
            }

            return Receipt.CreateReceiptFromText(errorMessage == null ? "startup-info" : "start-error-info", stringBuilder.ToString());
        }
    }
}
