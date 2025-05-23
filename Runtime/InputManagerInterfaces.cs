using UnityEngine.InputSystem;

namespace LifeLogs.InputSystem {
    /// <summary> 입력 이벤트를 수신 하는 인터페이스 </summary>
    public interface IInputReceiver {
        void OnInputPerformed(string actionMap, string actionName, InputAction.CallbackContext context);
    }
    
    /// <summary> Input Device 변경 이벤트를 수신 하는 인터페이스 </summary>
    public interface IDeviceChangedReceiver {
        /// <summary> 디바이스가 추가 되면 알림 </summary>
        public void OnDeviceConnected(string deviceType);
        /// <summary> 디바이스가 삭제 되면 알림 </summary>
        public void OnDeviceDisconnected(string deviceType);
    }
    
    /// <summary> [내부용] Input Device 변경 이벤트를 수신 하는 인터페이스 </summary>
    internal interface IInputDeviceConnectorDelegate {
        internal void OnDeviceChanged(bool isAdd, string deviceType);
    }
}