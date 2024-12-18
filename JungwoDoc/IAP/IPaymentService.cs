namespace JungWoo.IAP
{
    public interface IPaymentService
    {
        void Initialize();
        void PurchaseProduct(string productId);
        void RestorePurchases();
    }
}