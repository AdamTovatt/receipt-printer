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
        private int failedFetchAttempts = 0;
        private DateTime lastPrintTime = DateTime.MinValue;

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
            lastPrintTime = DateTime.UtcNow; // the system is being used by some one right now since it just started

            try
            {
                await printingManager.PrintAsync("first-startup-message", "System is starting...", receiptConfig);
                await Task.Delay(5000);

                bool initialized = await InitializeAsync();

                while (!initialized)
                {
                    await Task.Delay(refreshDelay);
                    initialized = await InitializeAsync();
                }

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        foreach (Purchase purchase in await purchaseManager.GetNewPurchasesAsync())
                        {
                            Receipt receipt = new Receipt(purchase, receiptConfig);
                            await printingManager.PrintAsync(receipt);
                            lastPrintTime = DateTime.UtcNow;
                        }

                        if (hasHadAnError)
                        {
                            await printingManager.PrintAsync("success-message", "The system is back online", receiptConfig);
                            failedFetchAttempts = 0;
                        }

                        printingManager.RemovePrints("start-error-info"); // remove error message prints
                        printingManager.RemovePrints("error-message"); // so they will be printed again if a new error occurrs
                        printingManager.RemovePrints("success-message");
                        hasHadAnError = false;

                        await Task.Delay(refreshDelay, stoppingToken);
                    }
                    catch (Exception exception)
                    {
                        failedFetchAttempts++;

                        if (GetShouldPrintErrorMessage())
                        {
                            hasHadAnError = true;
                            string formattedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                            await printingManager.PrintAsync("error-message", $"{formattedDate} An error has occurred, the system might be offline: {exception.Message}", receiptConfig);
                        }

                        double refreshDelayMultiplier = GetRefreshDelayMultiplier(DateTime.Now, GetSecondsSinceLastPrint());
                        int _refreshDelay = Math.Clamp((int)Math.Round(refreshDelay * refreshDelayMultiplier), refreshDelay, refreshDelay * 24);
                        await Task.Delay(_refreshDelay);
                    }
                }
            }
            catch (Exception exception)
            {
                await printingManager.PrintAsync("fatal-error-message", $"A fatal error has occurred and the system needs to be restarted: {exception.Message}", receiptConfig);
            }
        }

        // If the last print was less than 30 minutes ago, print the error message immediately since the system is probably being used right now
        // If the print was more than 30 minutes ago but still less than an hour, print the error message if there has been more than 1 failed fetch attempt
        // If the print was more than an hour ago, print the error message if there has been more than 2 failed fetch attempts
        private bool GetShouldPrintErrorMessage()
        {
            int secondsSinceLastPrint = GetSecondsSinceLastPrint();

            if (secondsSinceLastPrint < 1800) // Printed in the last half hour
                return true;

            if (secondsSinceLastPrint < 3600 && failedFetchAttempts > 1)
                return true;

            if (secondsSinceLastPrint >= 3600 && failedFetchAttempts > 2)
                return true;

            return false;
        }

        private int GetSecondsSinceLastPrint()
        {
            return (int)Math.Clamp((DateTime.UtcNow - lastPrintTime).TotalSeconds, 0, 21600); // 21 600 seconds is the same as 6 hours
        }

        // This is to prevent the system from spamming the Zettle API with requests if it's not even being used
        public static double GetRefreshDelayMultiplier(DateTime now, int timeSinceLastPrint)
        {
            if (timeSinceLastPrint <= 3600) // Printed something in the last hour
                return 1;

            DayOfWeek day = now.DayOfWeek;
            TimeSpan time = now.TimeOfDay;

            // Check for Wednesday between 17:00 and 20:00
            if (day == DayOfWeek.Wednesday && time >= new TimeSpan(17, 0, 0) && time <= new TimeSpan(20, 0, 0))
                return 2;

            // Check for Friday between 21:00 and 23:00
            if (day == DayOfWeek.Friday && time >= new TimeSpan(21, 0, 0) && time <= new TimeSpan(23, 0, 0))
                return 2;

            // Determine the max clamp value and time of day multiplier
            bool isEarlyMorning = time >= new TimeSpan(1, 0, 0) && time <= new TimeSpan(8, 0, 0);
            double maxClamp = isEarlyMorning ? 24 : 12;
            double timeOfDayMultiplier = isEarlyMorning ? 2 : 1;

            // Linear multiplier based on time since last print
            double timeSinceLastPrintHours = timeSinceLastPrint / 3600.0;
            double linearMultiplier = 1 + ((timeSinceLastPrintHours - 1) / 15) * (maxClamp - 1);
            linearMultiplier = Math.Clamp(linearMultiplier * 2, 1, maxClamp);

            return Math.Min(timeOfDayMultiplier * linearMultiplier, maxClamp);
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

            return Receipt.CreateReceiptFromText(errorMessage == null ? "startup-info" : "start-error-info", stringBuilder.ToString(), receiptConfig);
        }
    }
}
