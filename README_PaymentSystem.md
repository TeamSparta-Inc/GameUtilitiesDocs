# 📄 **README: Payment System in Unity**

---

### 📌 **개요**

이 문서는 Unity 프로젝트에서 사용할 수 있는 결제 시스템 모듈에 대해 설명합니다. 이 결제 시스템은 **Unity IAP**를 기반으로 하며, 다음과 같은 클래스들로 구성되어 있습니다:

1. `IPaymentService` - 결제 서비스 인터페이스
2. `PaymentManager` - 결제 요청을 처리하는 매니저
3. `PaymentServiceFactory` - 결제 서비스의 인스턴스를 생성하는 팩토리 클래스
4. `UnityIAPService` - Unity IAP를 구현하는 서비스 클래스

이 구조를 사용하면 결제 로직을 모듈화하고 유지 보수를 용이하게 할 수 있습니다.

---

### 🚀 **구성 요소 설명**

#### 1. **IPaymentService**

`IPaymentService`는 결제 서비스의 기본 인터페이스입니다.

```csharp
namespace JungWoo.IAP
{
    public interface IPaymentService
    {
        void Initialize();
        void PurchaseProduct(string productId);
        void RestorePurchases();
    }
}
```

- **`Initialize`**: 결제 시스템 초기화  
- **`PurchaseProduct`**: 지정된 상품 ID로 결제를 수행  
- **`RestorePurchases`**: 구매 복원 (주로 iOS에서 사용)  

---

#### 2. **PaymentManager**

`PaymentManager`는 결제 요청을 처리하는 매니저 클래스입니다.

```csharp
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
```

- **`BuyProduct`**: 상품 ID를 기반으로 결제를 요청합니다.  
- **`RestorePurchases`**: 이전에 구매한 항목을 복원합니다.  

---

#### 3. **PaymentServiceFactory**

`PaymentServiceFactory`는 결제 서비스의 인스턴스를 생성하고 반환하는 팩토리 클래스입니다.

```csharp
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
```

- **싱글톤 패턴**으로 `IPaymentService`의 인스턴스를 생성하고 관리합니다.  
- **`DontDestroyOnLoad`**를 사용하여 결제 서비스 객체가 씬 전환 시에도 유지되도록 합니다.  

---

#### 4. **UnityIAPService**

`UnityIAPService`는 Unity IAP 기능을 구현한 클래스입니다.

```csharp
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
    }
}
```

- **`Initialize`**: Unity IAP를 초기화하고 구매 가능한 상품을 등록합니다.  
- **`PurchaseProduct`**: 지정된 상품 ID로 구매를 시작합니다.  
- **`RestorePurchases`**: 이전 구매를 복원합니다 (iOS 전용).  
- **`OnInitialized`**: IAP 초기화가 성공했을 때 호출됩니다.  
- **`OnInitializeFailed`**: IAP 초기화가 실패했을 때 호출됩니다.  

---

### 🛠️ **사용법**

1. **PaymentManager 설정**  
   `PaymentManager`를 Unity 씬에 추가합니다.

2. **상품 구매하기**  
   다음 코드를 사용해 상품을 구매할 수 있습니다.

   ```csharp
   PaymentManager paymentManager = FindObjectOfType<PaymentManager>();
   paymentManager.BuyProduct("com.example.consumable");
   ```

3. **구매 복원하기**  
   iOS에서 구매 복원을 호출합니다.

   ```csharp
   paymentManager.RestorePurchases();
   ```

---

### ⚠️ **주의사항**

1. **Unity IAP 설정**  
   Unity Editor에서 **Services** > **In-App Purchasing**를 활성화하고, 상품 ID를 설정해야 합니다.

2. **플랫폼별 테스트**  
   Android 및 iOS에서 각각의 결제 로직이 올바르게 작동하는지 테스트하세요.

3. **보안**  
   결제 검증 로직을 서버에서 처리하는 것이 안전합니다.

---

이 결제 시스템을 사용하여 Unity 프로젝트에서 안전하고 효율적인 결제 기능을 구현하세요! 🛒😊