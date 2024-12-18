using System;
using UnityEngine;
using UnityEngine.Purchasing;

namespace JungWoo.IAP
{
    public class UnityIAPService : MonoBehaviour, IPaymentService, IStoreListener
    {
        private IStoreController storeController;
        private IExtensionProvider storeExtensionProvider;

        public event Action<string> OnPurchaseSuccess;
        public event Action<string, PurchaseFailureReason> OnPurchaseFailed;

        public void Initialize()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            // 상품 ID 등록 예제
            builder.AddProduct("com.example.consumable", ProductType.Consumable);
            builder.AddProduct("com.example.nonconsumable", ProductType.NonConsumable);

            UnityPurchasing.Initialize(this, builder);
        }

        public void PurchaseProduct(string productId)
        {
            if (storeController != null)
            {
                storeController.InitiatePurchase(productId);
            }
            else
            {
                Debug.LogError("Purchase failed: IAP not initialized.");
            }
        }

        public void RestorePurchases()
        {
            if (storeExtensionProvider != null)
            {
                var apple = storeExtensionProvider.GetExtension<IAppleExtensions>();
                apple.RestoreTransactions(result =>
                {
                    Debug.Log($"RestorePurchases result: {result}");
                });
            }
            else
            {
                Debug.LogError("Restore failed: IAP not initialized.");
            }
        }

        // IStoreListener 콜백 메서드
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            storeController = controller;
            storeExtensionProvider = extensions;
            Debug.Log("IAP Initialization successful.");
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError($"IAP Initialization failed: {error}");
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            Debug.Log($"Purchase successful: {args.purchasedProduct.definition.id}");
            OnPurchaseSuccess?.Invoke(args.purchasedProduct.definition.id);
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            Debug.LogError($"Purchase failed: {product.definition.id}, Reason: {reason}");
            OnPurchaseFailed?.Invoke(product.definition.id, reason);
        }
    }
}