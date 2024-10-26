using ReceiptPrinter;
using System.Text;

namespace ReceiptPrinterTests
{
    [TestClass]
    public class TextFormatTests
    {
        [TestMethod]
        public void SplitLongText1()
        {
            int width = 26;

            StringBuilder original = new StringBuilder();

            original.AppendLine("ReceiptPrinter service successfully started and initialized.");
            original.AppendLine("2024-10-26 20:48:18");
            original.AppendLine($"RefreshDelay: 5000 ms");
            original.AppendLine("Category filter is specified, will only print receipts for the following categories:");
            original.AppendLine($"- mat");
            original.AppendLine("Have a nice day! :)");

            StringBuilder expected = new StringBuilder();

            expected.AppendLine("ReceiptPrinter service");
            expected.AppendLine("successfully started and");
            expected.AppendLine("initialized.");
            expected.AppendLine("2024-10-26 20:48:18");
            expected.AppendLine("RefreshDelay: 5000 ms");
            expected.AppendLine("Category filter is");
            expected.AppendLine("specified, will only print");
            expected.AppendLine("receipts for the following");
            expected.AppendLine("categories:");
            expected.AppendLine("- mat");
            expected.AppendLine("Have a nice day! :)");

            string actual = new FormattedText(original.ToString()).ApplyMaxWidth(width);
            string expectedString = expected.ToString();

            Assert.AreEqual(expectedString, actual);
        }
    }
}