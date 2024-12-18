using UnityEngine;

namespace JungWoo.IAP
{
    public class PaymentManager : MonoBehaviour
    {
        private IPaymentService paymentService;

        void Start()
        {
            paymentService = PaymentServiceFactory.GetPaymentService();
        }

        public void BuyProduct(string productId)
        {
            paymentService.PurchaseProduct(productId);
        }

        public void RestorePurchases()
        {
            paymentService.RestorePurchases();
        }
    }
}