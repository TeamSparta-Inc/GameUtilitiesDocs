using UnityEngine;

namespace JungWoo.IAP
{
    public static class PaymentServiceFactory
    {
        private static IPaymentService _paymentService;

        public static IPaymentService GetPaymentService()
        {
            if (_paymentService == null)
            {
                var paymentGameObject = new GameObject("UnityIAPService");
                _paymentService = paymentGameObject.AddComponent<UnityIAPService>();
                _paymentService.Initialize();
                Object.DontDestroyOnLoad(paymentGameObject);
            }

            return _paymentService;
        }
    }
}