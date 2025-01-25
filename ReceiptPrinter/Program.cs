using QuestPDF.Infrastructure;
using ReceiptPrinter.ZettleClasses;

namespace ReceiptPrinter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            RuntimeArguments runtimeArguments = new RuntimeArguments(args);
            runtimeArguments.Require("client-id", "client-secret");

            ZettleConfig zettleConfig = new ZettleConfig(
                runtimeArguments.Get<string>("client-id")!,
                runtimeArguments.Get<string>("client-secret")!);

            ReceiptConfig receiptConfig = new ReceiptConfig(
                runtimeArguments.Get<string>("allowed-categories"),
                runtimeArguments.Get<int>("font-size"),
                runtimeArguments.Get<int>("page-width-mm"),
                runtimeArguments.Get<bool>("is-for-customer"),
                runtimeArguments.Get<int>("bottom-margin"),
                runtimeArguments.Get<string>("bottom-decoration"),
                runtimeArguments.Get<string>("top-decoration"));

            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddSingleton(zettleConfig);
            builder.Services.AddSingleton(receiptConfig);
            builder.Services.AddHostedService<Worker>();

            IHost host = builder.Build();
            host.Run();
        }
    }
}