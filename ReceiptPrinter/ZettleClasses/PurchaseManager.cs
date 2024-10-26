namespace ReceiptPrinter.ZettleClasses
{
    public class PurchaseManager
    {
        private ZettleConfig _zettleConfig;
        private ReceiptConfig _receiptConfig;

        private Zettle zettle;

        private bool initialized = false;
        private int lastPurchaseNumber = 0;

        public PurchaseManager(ZettleConfig zettleConfig, ReceiptConfig receiptConfig)
        {
            zettle = new Zettle(zettleConfig);
            _receiptConfig = receiptConfig;
            _zettleConfig = zettleConfig;
        }

        public async Task InitializeAsync()
        {
            if (initialized) return;

            Purchase? lastPurchase = (await zettle.GetPurchasesAsync(1000, allowedCategoryNames: _receiptConfig.AllowedCategories)).FirstOrDefault();

            if (lastPurchase == null)
                lastPurchase = (await zettle.GetPurchasesAsync(1000)).FirstOrDefault(); // try again without the category filter

            if (lastPurchase == null)
                throw new Exception("Failed to initalize, no purchases found");

            lastPurchaseNumber = lastPurchase.GlobalPurchaseNumber;

            initialized = true;
        }

        public async Task<List<Purchase>> GetNewPurchasesAsync()
        {
            if (!initialized)
                throw new Exception("InitializeAsync() needs to be called before GetNewPurchasesAsync() can be called!");

            List<Purchase> purchases = await zettle.GetPurchasesAsync(10, allowedCategoryNames: _receiptConfig.AllowedCategories);
            List<Purchase> newPurchases = purchases.Where(p => p.GlobalPurchaseNumber > lastPurchaseNumber).ToList();

            if (newPurchases.Count > 0)
                lastPurchaseNumber = newPurchases.Max(p => p.GlobalPurchaseNumber);

            return newPurchases;
        }
    }
}
