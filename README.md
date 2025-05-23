# Unity InputManager (v1.1.0)
　
　
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

- Unity Package Manager를 통해 쉽게 설치할 수 있습니다.

1. **Package Manager** 열기
2. **Install package from git URL…** 선택
3. **`https://github.com/joylike2/Unity-InputManager.git`** 입력 후 설치
   
   <img src="https://github.com/joylike2/Unity-SaveLoad-FileManager/blob/main/Documentation~/Img_PackageManager1.png?raw=true" width="480px">
```none
https://github.com/joylike2/Unity-InputManager.git
```

　



## 📌 사용 방법

### - 초기화
상단 메뉴 **Tools / InputManager / Settings** 를 선택 합니다.

<img src="https://github.com/joylike2/Unity-InputManager/blob/main/Documentation~/Img_PackageManager1.png?raw=true" width="380px"> 　<img src="https://github.com/joylike2/Unity-InputManager/blob/main/Documentation~/Img_PackageManager2.png?raw=true" width="380px">

액션맵 추가 및 삭제가 가능 하며, 해당 액션들을 확인 할 수 있습니다.


　




<img src="https://github.com/joylike2/Unity-InputManager/blob/main/Documentation~/Img_PackageManager3.png?raw=true" width="420px">

해당 경로에 파일이 생성됩니다. 사용할 키를 추가, 변경을 해서 사용하세요.


---
 
### 입력 시스템 사용 할 ActionMap 선택

```csharp
using LifeLogs.InputSystem;

//입력 시스템으로 사용할 ActionMap 선택
void Start() {
    InputManager.Instance.SwitchActionMap("Player");
}
```

### 입력 이벤트 사용 예시

```csharp
//1. IInputReceiver 추가
public class PlayerController : MonoBehaviour, IInputReceiver {

    private void OnEnable() {
        //2. Event 등록
        InputManager.Instance.RegisterReceiver(this);
    }

    private void OnDisable() {
        //3. Event 해제
        InputManager.Instance.UnregisterReceiver(this);
    }

    //interface 함수로 Event 콜백을 받습니다.
    public void OnInputPerformed(string actionMap, string actionName, InputAction.CallbackContext context) {
        Debug.Log($"입력: {actionMap} - {actionName} ({context.ReadValueAsObject()})");
    }
}
```

### 키 리바인딩 예시(키 변경)

```csharp
InputManager.Instance.StartRebinding(ActionMapName, DeviceType, ActionName, PartKey, compositePartName, (isResult, partKey, changePartKey) => {
    Debug.Log($"키변경 유무: {isResult}, 리바인딩 할 키: {partKey} 리바인딩 된 키: {changePartKey}");
});
```

### 리바인딩 키 초기화

```csharp
InputManager.Instance.ResetRebinds();
```

---

### 입력 디바이스의 추가 및 해제 알림(변경v1.1.0)

```csharp
//1. IDeviceChangedReceiver 추가
public class PlayerController : MonoBehaviour, IDeviceChangedReceiver {

    private void OnEnable() {
        //2. Delegate 등록
        InputManager.Instance.SetDeviceChangedReceiver(this);
    }

    private void OnDisable() {
        //3. Delegate 해제
        InputManager.Instance.RemoveDeviceChangedReceiver();
    }

    //* interface 함수로 Event 콜백을 받습니다.
    //디바이스가 추가 되면 알림
    public void OnDeviceConnected(string deviceType) {
        Debug.Log($"새로운 디바이스 연결, 디바이스: {deviceType}");
    }
    ///디바이스가 삭제 되면 알림
    public void OnDeviceDisconnected(string deviceType) {
        Debug.Log($"연결 디바이스 해제, 디바이스: {deviceType}");
    }
}

//디바이스 변경 이벤트 수신 델리게이트 등록 유무
bool isRegisteredToDeviceChange = InputManager.Instance.IsDeviceChangedReceiver();
```


　
　
　
## 📌 데모 설치 방법
- 데모 설치 순서
　  
	Unity Package Manager 를 통해 가져올 수 있습니다.
	1. **Package Manager** 열기
	2. **설치된 FileManager 패키지 메뉴에서 Samples** 선택
	3. **우측 Import** 선택 설치

<img src="https://github.com/joylike2/Unity-InputManager/blob/main/Documentation~/Img_PackageManager0.png?raw=true" width="480px">  
　  


- 아래 경로에서 샘플을 확인 할 수 있습니다.  
Canvas 가 가진 게임오브젝트들의 Script 를 확인 해주세요.
<img src="https://github.com/joylike2/Unity-InputManager/blob/main/Documentation~/Img_PackageManager4.png?raw=true" width="480px">
　  



## 지원 환경

* Unity 2021.3 이상 버전에서 정상 동작
* NewInputSystem 1.14.0 으로 작성.　

　



## 🎉 라이선스
This package is licensed under The MIT License (MIT)

Copyright © 2025 joylike2 (https://github.com/joylike2)

[https://github.com/joylike2/Unity-InputManager](https://github.com/joylike2/Unity-InputManager)    
　

See full copyrights in LICENSE.md inside repository
　

