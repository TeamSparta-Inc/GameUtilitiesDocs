# 📄 **README: Addressable 기반 리소스 매니저**

---

### 📌 **개요**

`ResourceManager`는 Unity의 **Addressable 시스템**을 기반으로 리소스(에셋)를 효율적으로 로드하고 관리하는 모듈입니다. 이 모듈을 통해 리소스를 비동기적으로 로드하고, 캐싱 및 인스턴스화된 리소스를 관리할 수 있습니다.

---

### 🚀 **기능**

1. **비동기 리소스 로드**  
   Addressable 키를 기반으로 리소스를 비동기적으로 로드합니다.

2. **캐싱 기능**  
   이미 로드된 리소스를 다시 로드하지 않고, 캐시에서 가져옵니다.

3. **인스턴스화된 오브젝트 핸들 관리**  
   인스턴스화된 게임 오브젝트와 관련된 핸들을 관리합니다.

4. **에러 핸들링 지원**  
   로드 실패 시 예외 처리를 지원합니다.

---

### 📦 **코드 예제**

#### **1. 리소스 로드하기**

리소스를 비동기적으로 로드하고, 성공 시 콜백을 실행합니다.

```csharp
public enum AddressableEnum
{
    PlayerPrefab,
    EnemyPrefab,
    // 다른 리소스 키를 여기에 추가하세요.
}

void Start()
{
    ResourceManager.Instance.LoadAsset<GameObject>(
        AddressableEnum.PlayerPrefab,
        onLoadComplete: (playerPrefab) => 
        {
            Instantiate(playerPrefab);
        },
        onLoadError: (exception) => 
        {
            Debug.LogError(exception.Message);
        });
}
```

#### **2. 리소스 언로드하기**

로드된 리소스를 언로드하여 메모리를 해제합니다.

```csharp
ResourceManager.Instance.ReleaseAsset(AddressableEnum.PlayerPrefab);
```

---

### 🛠️ **함수 설명**

#### **`LoadAsset<T>`**

```csharp
public void LoadAsset<T>(AddressableEnum key, Action<T> onLoadComplete = null, Action<Exception> onLoadError = null)
```

- **설명:**  
  Addressable 키를 기반으로 비동기적으로 리소스를 로드합니다.

- **매개변수:**
  - `key`: `AddressableEnum` 형식의 리소스 키입니다.
  - `onLoadComplete`: 로드 성공 시 호출되는 콜백 함수입니다.
  - `onLoadError`: 로드 실패 시 호출되는 콜백 함수입니다.

#### **`ReleaseAsset`**

```csharp
public void ReleaseAsset(AddressableEnum key)
```

- **설명:**  
  특정 리소스를 언로드하여 메모리를 해제합니다.

- **매개변수:**
  - `key`: `AddressableEnum` 형식의 리소스 키입니다.

---

### ⚠️ **주의사항**

1. **Addressable 키**는 `AddressableEnum`으로 관리되므로, Addressable 에셋의 키 값과 열거형 값이 일치해야 합니다.
2. 리소스를 언로드할 때는 **캐시에 저장된 핸들**을 사용하여 언로드를 수행하므로, `ReleaseAsset`을 적절히 호출해야 메모리 누수를 방지할 수 있습니다.

---

### 📚 **확장하기**

- **Addressable 키 관리**:  
  `AddressableEnum`에 새로운 리소스 키를 추가하세요.

- **로드 상태 모니터링**:  
  로드 중 상태를 UI에 표시하려면 `AsyncOperationHandle`의 진행 상태를 활용하세요.

---

이 모듈을 활용하여 프로젝트의 리소스 관리를 효율화하고 성능을 개선하세요! 😊
