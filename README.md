# 📦 **Unity Project Modular Features Documentation**

---

이 문서는 Unity 프로젝트에서 사용되는 모듈화된 기능들의 설명과 각 모듈에 대한 링크를 제공합니다. 각 모듈은 독립적으로 설계되어 있으며, 기능별로 필요한 시스템을 쉽게 통합하고 유지보수할 수 있습니다.

---

## 📚 **모듈 목록**

1. [🔹 **Addressable 기반 리소스 매니저**](./README_ResourceManager.md)  
   Unity의 Addressable 시스템을 기반으로 리소스를 효율적으로 로드하고 관리하는 모듈입니다.

2. [🔹 **푸시 알림 관리자**](./README_PushNotificationManager.md)  
   Android와 iOS에서 로컬 푸시 알림을 처리하는 모듈입니다.

3. [🔹 **사운드 관리자**](./README_SoundManager.md)  
   오디오 클립을 비동기적으로 로드하고, 여러 사운드를 동시에 재생할 수 있는 모듈입니다.

4. [🔹 **광고 관리자**](./README_AdsManager.md)  
   보상형 광고와 전면 광고를 관리하고, 일일 광고 노출 제한을 설정하는 모듈입니다.

5. [🔹 **결제 시스템**](./README_PaymentSystem.md)  
   Unity IAP를 사용하여 결제를 처리하는 모듈입니다. 결제 서비스 인터페이스와 팩토리를 통해 확장성을 고려한 설계입니다.

---

## 📖 **모듈 설명**

### 1. **Addressable 기반 리소스 매니저**

- **기능:**  
  - Addressable 시스템을 사용한 리소스 로드  
  - 리소스 캐싱 및 재사용  
  - 비동기 로드 및 에러 핸들링  

- **자세한 내용:** [📄 **README_ResourceManager.md**](./README_ResourceManager.md)

---

### 2. **푸시 알림 관리자**

- **기능:**  
  - Android 및 iOS 로컬 푸시 알림 지원  
  - 권한 요청 및 초기화  
  - 알림 데이터 Addressable 로드  

- **자세한 내용:** [📄 **README_PushNotificationManager.md**](./README_PushNotificationManager.md)

---

### 3. **사운드 관리자**

- **기능:**  
  - Addressable을 사용한 오디오 클립 비동기 로드  
  - 오디오 소스 풀링을 통한 다중 사운드 재생  
  - 자동 풀 확장 및 캐싱  

- **자세한 내용:** [📄 **README_SoundManager.md**](./README_SoundManager.md)

---

### 4. **광고 관리자**

- **기능:**  
  - 보상형 광고 및 전면 광고 표시  
  - 일일 광고 노출 제한  
  - Firebase Analytics를 통한 광고 추적  

- **자세한 내용:** [📄 **README_AdsManager.md**](./README_AdsManager.md)

---

### 5. **결제 시스템**

- **기능:**  
  - Unity IAP 기반 결제 시스템  
  - 결제 서비스 인터페이스 및 팩토리 패턴  
  - 구매 및 구매 복원 기능  

- **자세한 내용:** [📄 **README_PaymentSystem.md**](./README_PaymentSystem.md)

---

## 🛠️ **사용법**

1. 각 모듈에 대한 상세한 설명은 위의 링크를 참조하세요.
2. 필요에 따라 각 모듈을 프로젝트에 통합하고 초기화 코드를 추가하세요.
3. 모듈들은 독립적으로 작동하므로 필요한 기능만 선택적으로 사용할 수 있습니다.

---

이 문서를 참고하여 Unity 프로젝트에서 모듈화된 기능을 효율적으로 관리하세요! 😊🚀