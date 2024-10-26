﻿using ReceiptPrinter.Printers;
using System.Runtime.InteropServices;

namespace ReceiptPrinter
{
    public class PrintingManager
    {
        private readonly IPrinter printer;
        private readonly ILogger logger;

        private List<string> printedDocuments = new List<string>();

        public PrintingManager(ILogger logger)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                printer = new LinuxPrinter(logger);
            else
                printer = new WindowsPrinter(logger);

            this.logger = logger;
        }

        public async Task PrintAsync(Receipt receipt)
        {
            if (printedDocuments.Contains(receipt.FileName))
                return;

            await printer.PrintAsync(receipt);

            printedDocuments.Add(receipt.FileName);

            CleanDocumentList();
        }

        private void CleanDocumentList()
        {
            while (printedDocuments.Count > 200)
                printedDocuments.RemoveAt(0);
        }
    }
}
