using QuestPDF.Infrastructure;
using ReceiptPrinter.ZettleClasses;

namespace ReceiptPrinter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            if (args.Length < 2)
            {
                Console.WriteLine("Missing command line arguments for client id and client secret.\nUsage: ReceiptPrinter <client_id> <client_secret>");
                return;
            }

            ZettleConfig zettleConfig = new ZettleConfig(args[0], args[1]);
            ReceiptConfig receiptConfig = new ReceiptConfig(args.SafeGetElementAtIndex(2));

            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddSingleton(zettleConfig);
            builder.Services.AddSingleton(receiptConfig);
            builder.Services.AddHostedService<Worker>();

            IHost host = builder.Build();
            host.Run();
        }
    }
}