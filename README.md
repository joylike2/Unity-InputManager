# Unity InputManager (v1.0.0)  
　
　
## ✅ 소개  

Unity의 New Input System을 기반으로 한 고급 입력 관리 시스템을 제공합니다.

씬(Scene) 및 캔버스(Canvas) 단위의 입력 설정을 지원하며, 플레이 중 실시간 키 재설정(리바인딩) 및 모든 플랫폼 디바이스를 지원합니다.  
　
　
## ⭐ 주요 특징

* **씬 및 캔버스 단위 입력 관리**: 씬과 캔버스 별로 개별 입력 설정 지원
* **런타임 키 리바인딩**: 게임 실행 중 실시간으로 키 변경 가능
* **자동 장치 관리**: 게임패드, 키보드, 마우스 등 모든 플랫폼 디바이스 지원
* **입력 중복 방지**: 디바이스 및 액션 별로 중복 키 입력을 자동으로 방지
* **입력 이벤트 기반 구조**: 입력 발생 시 이벤트를 통해 손쉽게 처리 가능
* **DontDestroyOnLoad** 자동 적용: 씬 전환 시 입력 시스템 유지 보장

　
　
　
## 📌 설치 방법

Unity Package Manager를 통해 쉽게 설치할 수 있습니다.

1. **Package Manager** 열기
2. **Install package from git URL…** 선택 
3. **`https://github.com/joylike2/Unity-InputManager.git`** 입력 후 설치
<img src="https://github.com/joylike2/Unity-SaveLoad-FileManager/blob/main/Documentation~/Img_PackageManager1.png?raw=true" width="480px">
```none
https://github.com/joylike2/Unity-InputManager.git
```

　



## 📌 사용 방법

### 입력 시스템 사용 할 ActionMap 선택

```csharp
using LifeLogs.InputSystem;

//입력 시스템 ActionMap 선택
void Start() {
    InputManager.Instance.SwitchActionMap("Player");
}
```

### 입력 이벤트 사용 예시

```csharp
public class PlayerController : MonoBehaviour, IInputReceiver {

    private void OnEnable() {
        InputManager.Instance.RegisterReceiver(this);
    }

    private void OnDisable() {
        InputManager.Instance.UnregisterReceiver(this);
    }

    public void OnInputPerformed(string actionMap, string actionName, InputAction.CallbackContext context) {
        Debug.Log($"입력: {actionMap} - {actionName} ({context.ReadValueAsObject()})");
    }
}
```

### 키 리바인딩 예시(키 변경)

```csharp
InputManager.Instance.StartRebinding("Player", "Keyboard", "Move", "Up", "W", (isResult, newBinding, readableKey) => {
    Debug.Log($"리바인딩 완료: {readableKey}");
});
```
　



## 지원 환경

* Unity 2021.3 이상 버전에서 정상 동작
* NewInputSystem 1.14.0 으로 작성.　

　



## 🎉 라이선스
This package is licensed under The MIT License (MIT)

Copyright © 2025 joylike2 (https://github.com/joylike2)

[https://github.com/joylike2/Unity-InputManager](https://github.com/joylike2/Unity-InputManager)    
　

See full copyrights in LICENSE.md inside repository
　

