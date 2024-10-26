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
            printingManager = new PrintingManager();
            this.receiptConfig = receiptConfig;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!await InitializeAsync())
                return;

            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (Purchase purchase in await purchaseManager.GetNewPurchasesAsync())
                {
                    Receipt receipt = new Receipt(purchase);
                    printingManager.Print(receipt);
                }

                await Task.Delay(refreshDelay, stoppingToken);
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

            printingManager.Print(CreateStartupReceipt(errorMessage));

            return true;
        }

        private Receipt CreateStartupReceipt(string? errorMessage)
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (errorMessage == null)
            {
                stringBuilder.AppendLine("ReceiptPrinter service successfully started and initialized.");
                stringBuilder.AppendLine(DateTime.Now.ToString());
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

                for (int i = 0; i < 10; i++)
                {
                    stringBuilder.AppendLine("TestLine");
                }

                stringBuilder.AppendLine("Have a nice day! :)");
            }
            else
            {
                stringBuilder.AppendLine("ReceiptPrinter service failed to initialize");
                stringBuilder.AppendLine(DateTime.Now.ToString());
                stringBuilder.AppendLine($"Error:\n{errorMessage}");
            }

            return Receipt.CreateReceiptFromText("startup-info", stringBuilder.ToString());
        }
    }
}
