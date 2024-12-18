# 📄 **README: EmailSender**

---

### 📌 **개요**

`EmailSender`는 Unity 애플리케이션 내에서 **이메일 전송** 기능을 구현하는 유틸리티 클래스입니다. 사용자가 버튼을 클릭하면 미리 작성된 버그 리포트나 문의사항 이메일을 전송할 수 있는 **기본 이메일 클라이언트**를 엽니다.

---

### 🚀 **기능**

1. **버그 리포트/문의 이메일 작성**  
   버튼 클릭 시, 이메일 클라이언트가 열리며 버그 리포트나 문의사항을 작성할 수 있습니다.

2. **기기 정보 자동 포함**  
   사용자의 **기기 모델**과 **운영체제 정보**가 자동으로 이메일 본문에 포함됩니다.

3. **URL 인코딩 지원**  
   `EscapeURL` 메서드를 사용하여 이메일 제목과 본문에 특수문자가 올바르게 처리되도록 합니다.

---

### 🛠️ **사용법**

#### **1. 버튼 클릭 이벤트에 연결하기**

Unity UI 버튼의 **OnClick 이벤트**에 `EmailSender`의 `OnClickEvent` 메서드를 연결합니다.

```csharp
public void OnClickEvent()
{
    string mailto = "myapp.support@gmail.com";
    string subject = EscapeURL("버그 리포트 / 기타 문의사항");
    string body = EscapeURL(
        "이 곳에 내용을 작성해주세요.\n\n\n\n" + 
        "________" + 
        "Device Model : " + SystemInfo.deviceModel + "\n\n" + 
        "Device OS : " + SystemInfo.operatingSystem + "\n\n" + 
        "________"
    );
    
    Application.OpenURL("mailto:" + mailto + "?subject=" + subject + "&body=" + body);
}
```

#### **2. URL 인코딩**

`EscapeURL` 메서드는 URL 인코딩을 수행하며, 공백을 `%20`으로 변환합니다.

```csharp
private string EscapeURL(string url)
{
    return WWW.EscapeURL(url).Replace("+", "%20");
}
```

---

### 📦 **스크립트 예제**

전체 `EmailSender` 스크립트 예제:

```csharp
using UnityEngine;

namespace JungWoo.Utilities
{
    public class EmailSender : MonoBehaviour
    {
        public void OnClickEvent()
        {
            string mailto = "myapp.support@gmail.com";
            string subject = EscapeURL("버그 리포트 / 기타 문의사항");
            string body = EscapeURL(
                "이 곳에 내용을 작성해주세요.\n\n\n\n" + 
                "________" + 
                "Device Model : " + SystemInfo.deviceModel + "\n\n" + 
                "Device OS : " + SystemInfo.operatingSystem + "\n\n" + 
                "________"
            );
            Application.OpenURL("mailto:" + mailto + "?subject=" + subject + "&body=" + body);
        }

        private string EscapeURL(string url)
        {
            return WWW.EscapeURL(url).Replace("+", "%20");
        }
    }
}
```

---

### ⚠️ **주의사항**

1. **이메일 클라이언트 필요**  
   사용자의 기기에 이메일 클라이언트가 설치되어 있어야 합니다.

2. **URL 인코딩**  
   특수문자와 공백이 URL에 올바르게 포함되도록 `EscapeURL`을 사용합니다.

3. **Android 및 iOS 테스트**  
   플랫폼에 따라 이메일 클라이언트 동작이 다를 수 있으므로 Android와 iOS에서 테스트하는 것이 좋습니다.

---

### 📚 **확장하기**

- **사용자 입력 필드 추가**:  
  이메일 본문에 사용자 입력 내용을 포함할 수 있습니다.

- **다국어 지원**:  
  이메일 제목과 본문에 다국어 시스템을 적용할 수 있습니다.

---

이 스크립트를 활용하여 사용자들이 쉽게 버그 리포트나 문의사항을 보낼 수 있도록 기능을 제공하세요! 😊