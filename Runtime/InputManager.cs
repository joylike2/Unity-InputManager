using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

namespace LifeLogs.InputSystem {
    public class InputManager : MonoBehaviour, IInputDeviceConnectorDelegate {
        private static InputManager _instance;

        public static InputManager Instance {
            get {
                if (_instance == null) {
#if UNITY_2023_1_OR_NEWER
                    _instance = FindFirstObjectByType<InputManager>();
#else
                    _instance = FindObjectOfType<InputManager>();
#endif
                    if (_instance == null) {
                        GameObject go = new GameObject("[InputManager]");
                        _instance = go.AddComponent<InputManager>();
                        DontDestroyOnLoad(go);
                    }
                }

                return _instance;
            }
        }

        private InputActionAsset _inputActionsAsset;
        private InputActionMap _currentActionMap;
        private readonly List<IInputReceiver> _receivers = new List<IInputReceiver>();
        private const string INPUT_ACTIONS_FILE_PATH = "Assets/InputManager/NewInputSystemActions.inputactions";
        private const string REBINDS_FILE_NAME = "/Rebinds.json";
        
        private IDeviceChangedReceiver _deviceChangedReceiver;
        /// <summary>
        /// <para> IInputDeviceConnectorDelegate 인터페이스 등록 필수 </para>
        /// <para> 디바이스 변경 이벤트 수신 델리게이트 등록 </para>
        /// </summary>
        public void SetDeviceChangedReceiver(IDeviceChangedReceiver delegateInstance) => _deviceChangedReceiver = delegateInstance;
        /// <summary> 디바이스 변경 이벤트 수신 델리게이트 해제 </summary>
        public void RemoveDeviceChangedReceiver() => _deviceChangedReceiver = null;
        /// <summary> 디바이스 변경 이벤트 수신 델리게이트 등록 유무 </summary>
        public bool IsDeviceChangedReceiver => _deviceChangedReceiver != null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateInstance() => _ = Instance;

        private void Awake() {
            if (_instance != null && _instance != this) {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            if (!VerifyAndLoadInputActionsAsset()) return;

            _inputActionsAsset.Enable();
            LoadRebinds(); //저장된 키설정 로드

            InputActionBindingResolver.Init(_inputActionsAsset);
            InputDeviceConnector.Init(_inputActionsAsset, this);
        }

        #region Initialization

        /// <summary> InputActionAsset 검증 및 로드 </summary>
        private bool VerifyAndLoadInputActionsAsset() {
            _inputActionsAsset = UnityEngine.InputSystem.InputSystem.actions; // 프로젝트 설정에서 직접 로드

            if (_inputActionsAsset == null) {
                Debug.LogError("[InputManager] InputActionAsset 로드 실패: 프로젝트 설정 확인이 필요합니다.");
                return false;
            }

#if UNITY_EDITOR
            InputActionAsset expectedAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<InputActionAsset>(INPUT_ACTIONS_FILE_PATH);

            if (expectedAsset == null) {
                Debug.LogError("[InputManager] 지정된 경로에 InputActions 파일이 없습니다.");
                return false;
            }

            if (_inputActionsAsset.ToJson() != expectedAsset.ToJson()) {
                Debug.LogError("[InputManager] 프로젝트 설정과 InputActions 파일이 일치하지 않습니다.");
                return false;
            }
#endif
            return true;
        }

        #endregion Initialization

        #region Binding Saves & Load

        /// <summary> 리바인딩 정보 저장 </summary>
        private void SaveRebinds() {
            string rebinds = _inputActionsAsset.SaveBindingOverridesAsJson();
            File.WriteAllText(Application.persistentDataPath + REBINDS_FILE_NAME, rebinds);
        }

        /// <summary> 리바인딩 정보 불러오기 </summary>
        private void LoadRebinds() {
            string path = Application.persistentDataPath + REBINDS_FILE_NAME;
            if (!File.Exists(path)) return;

            string rebinds = File.ReadAllText(path);
            _inputActionsAsset.LoadBindingOverridesFromJson(rebinds);
            Debug.Log("[InputManager] 저장된 키 설정 로드 완료");
        }

        #endregion Binding Saves & Load

        #region ActionMap Event Handling

        /// <summary> 액션맵 이벤트 바인딩 </summary>
        private void BindCurrentActionMapEvents() {
            foreach (var action in _currentActionMap.actions) {
                action.performed += BroadcastInputEvent;
                action.canceled += BroadcastInputEvent;
            }
        }

        /// <summary> 액션맵 이벤트 바인딩 해제 </summary>
        private void UnbindCurrentActionMapEvents() {
            foreach (var action in _currentActionMap.actions) {
                action.performed -= BroadcastInputEvent;
                action.canceled -= BroadcastInputEvent;
            }
        }

        #endregion ActionMap Event Handling

        #region Input Event Management

        /// <summary> 입력 이벤트를 모든 리시버에 전달 </summary>
        private void BroadcastInputEvent(InputAction.CallbackContext context) {
            if (_receivers.Count <= 0) return;
            IInputReceiver activeReceiver = _receivers[_receivers.Count - 1];
            activeReceiver.OnInputPerformed(_currentActionMap.name, context.action.name, context);
        }

        /// <summary> InputDeviceConnector 변경 정보 전달 </summary>
        void IInputDeviceConnectorDelegate.OnDeviceChanged(bool isAdd, string deviceType) {
            if (isAdd) {
                _deviceChangedReceiver?.OnDeviceConnected(deviceType);
            }
            else {
                _deviceChangedReceiver?.OnDeviceDisconnected(deviceType);
            }
        }
        #endregion Input Event Management

        #region public interface

        /// <summary> 액션맵 전환 </summary>
        public void SwitchActionMap(string mapName) {
            if (_currentActionMap != null) {
                _currentActionMap.Disable();
                UnbindCurrentActionMapEvents();
            }

            _currentActionMap = _inputActionsAsset.FindActionMap(mapName);
            if (_currentActionMap == null) {
                Debug.LogError($"[InputManager] '{mapName}' 액션맵을 찾을 수 없습니다.");
                return;
            }

            BindCurrentActionMapEvents();
            _currentActionMap.Enable();
        }

        /// <summary> 입력 리시버 등록 </summary>
        public void RegisterReceiver(IInputReceiver receiver) {
            if (!_receivers.Contains(receiver)) {
                _receivers.Add(receiver);
            }
        }

        /// <summary> 입력 리시버 해제 </summary>
        public void UnregisterReceiver(IInputReceiver receiver) {
            if (_receivers.Contains(receiver)) {
                _receivers.Remove(receiver);
            }
        }

        #region All InputActions

        /// <summary> InputActions의 액션맵 리스트 반환 </summary>
        public List<string> GetActionMapNames() => InputActionBindingResolver.GetActionMapNames();

        /// <summary> 해당 액션맵 정보 반환 </summary>
        public InputActionInfo GetActionInfo(string actionMapName) => InputActionBindingResolver.GetActionInfo(actionMapName);

        /// <summary> 해당 액션맵이 가진 디바이스 리스트 반환 </summary>
        public List<string> GetDeviceNameList(string actionMapName) => InputActionBindingResolver.GetDeviceNameList(actionMapName);

        /// <summary> 해당 액션맵의 디바이스의 바인딩 정보 반환 </summary>
        public DeviceBindingInfo GetDeviceBindingInfo(string actionMapName, string deviceType) => InputActionBindingResolver.GetDeviceBindingInfo(actionMapName, deviceType);

        /// <summary> 해당 디바이스 정보를 가진 액션맵 리스트 반환 </summary>
        public List<string> GetInputDeviceActionMapNames(string deviceType) => InputActionBindingResolver.GetInputDeviceActionMapNames(deviceType);

        #endregion All InputActions

        #region Connected InputActions

        /// <summary> 현재 연결된 디바이스 리스트 </summary>
        public List<string> GetConnectedInputDevices() => InputDeviceConnector.GetConnectedInputDevices();

        /// <summary> 현재 연결된 디바이스 인지 확인 </summary>
        public bool IsConnectedInputDevice(string deviceType) => InputDeviceConnector.IsConnectedInputDevice(deviceType);

        /// <summary> 현재 연결된 액션맵 리스트 반환 </summary>
        public List<string> GetConnectedActionMapNames() => InputDeviceConnector.GetConnectedActionMapNames();

        /// <summary> 현재 연결된 액션맵 인지 확인 </summary>
        public bool IsConnectedInputActionMap(string actionMapName) => InputDeviceConnector.IsConnectedInputActionMap(actionMapName);

        /// <summary> 현재 연결된 액션맵 정보 반환 </summary>
        public InputActionInfo GetConnectedActionInfo(string actionMapName) => InputDeviceConnector.GetConnectedActionInfo(actionMapName);

        /// <summary> 현재 연결된 액션맵이 가진 디바이스 리스트 반환 </summary>
        public List<string> GetConnectedActionMapDevices(string actionMapName) => InputDeviceConnector.GetConnectedActionMapDevices(actionMapName);

        /// <summary> 현재 연결된 액션맵의 디바이스의 바인딩 정보 반환 </summary>
        public DeviceBindingInfo GetConnectedActionMapDeviceBindingInfo(string actionMapName, string deviceName) => InputDeviceConnector.GetConnectedActionMapDeviceBindingInfo(actionMapName, deviceName);

        #endregion Connected InputActions


        /// <summary> 입력키 변경 </summary>
        /// <param name="actionMapName"> 변경할 액션맵 이름 </param>
        /// <param name="deviceType"> 변경할 액션의 디바이스 </param>
        /// <param name="actionName"> 변경할 액션 이름 </param>
        /// <param name="targetKey"> 변경할 키 </param>
        /// <param name="compositePartName"> 변경할 파트 이름. 복합 & 단일 유무 판단용 </param>
        /// <param name="onComplete"> 결과 반환(중복에 따른 성공(true) 실패(false) 정보, 변경 전 키, 변경 된 키) </param>
        public void StartRebinding(string actionMapName, string deviceType, string actionName, string targetKey, string compositePartName = "", Action<bool, string, string> onComplete = null) {
            InputActionMap actionMap = _inputActionsAsset.FindActionMap(actionMapName);
            InputAction action = actionMap.FindAction(actionName);

            action.Disable();

            //정확히 바꿀 키(targetKey)를 직접 지정하여 bindingIndex 찾기
            int bindingIndex = Array.FindIndex(action.bindings.ToArray(), b =>
                InputControlPath.TryGetDeviceLayout(b.effectivePath) == deviceType &&
                InputControlPath.ToHumanReadableString(b.effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice)
                    .Equals(targetKey, StringComparison.OrdinalIgnoreCase));

            action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("Mouse")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(operation => {
                    action.Enable();
                    operation.Dispose();

                    string newBinding = action.bindings[bindingIndex].effectivePath;
                    string newReadableKey = InputControlPath.ToHumanReadableString(newBinding, InputControlPath.HumanReadableStringOptions.OmitDevice);

                    bool isComplete = false;
                    if (InputDeviceConnector.IsDuplicateBinding(actionMapName, deviceType, actionName, newReadableKey)) {
                        Debug.LogError($"[InputManager] '{newReadableKey}' 키는 '{deviceType}'에서 이미 사용 중입니다.");
                        action.RemoveBindingOverride(bindingIndex); //중복 시 원상복구
                    }
                    else {
                        isComplete = true;
                        SaveRebinds();
                        InputActionBindingResolver.ChangeBindingKey(actionMapName, deviceType, actionName, targetKey, newReadableKey);
                        InputDeviceConnector.ChangeBindingKey(actionMapName, deviceType, actionName, targetKey, newReadableKey);

                        Debug.Log($"[InputManager] '{actionName}' [{compositePartName}] 리바인딩 완료: {newBinding}, 눌린 키: {newReadableKey}");
                    }

                    onComplete?.Invoke(isComplete, targetKey, newReadableKey);
                })
                .Start();
        }
        
        /// <summary> 변경 된 inputActionsAsset 을 리셋 합니다. </summary>
        public void ResetRebinds() {
            string path = Application.persistentDataPath + REBINDS_FILE_NAME;

            if (File.Exists(path)) {
                File.Delete(path);
                
                _inputActionsAsset.RemoveAllBindingOverrides();
                InputActionBindingResolver.ResetRebinds(_inputActionsAsset);
                InputDeviceConnector.ResetRebinds(_inputActionsAsset);
                
                Debug.Log("[InputManager] 키 바인딩이 초기화되었습니다.");
            }
        }

        #endregion public interface
    }

    /// <summary> InputActionsMap의 전체 정보 </summary>
    [Serializable]
    public class InputActionMapInfo {
        /// <summary> InputActionsMap 정보(key: 액션맵 이름, value: 정보) </summary>
        public Dictionary<string, InputActionInfo> actionMaps = new Dictionary<string, InputActionInfo>();

        /// <summary> InputActionsMap 전체 리스트 </summary>
        public List<string> GetActionMapNameList() => actionMaps.Keys.ToList();
    }

    /// <summary> InputAction의 전체 정보 </summary>
    [Serializable]
    public class InputActionInfo {
        /// <summary> 액션맵 이름 </summary>
        public string actionMapName;

        /// <summary> 디바이스 정보(key: 장치유형, value: 정보) </summary>
        public Dictionary<string, DeviceBindingInfo> devices;

        /// <summary> InputActions 디바이스 전체 리스트 </summary>
        public List<string> GetDeviceNameList() => devices.Keys.ToList();
    }

    [Serializable]
    public class DeviceBindingInfo {
        /// <summary> 장치 유형(ex: "Keyboard", "Gamepad") </summary>
        public string deviceType;

        /// <summary> 해당 장치에 연결된 액션 리스트 </summary>
        public List<ActionBindingInfo> actions;
    }

    [Serializable]
    public class ActionBindingInfo {
        /// <summary> 액션 이름(ex: "Move", "Attack") </summary>
        public string actionName;

        /// <summary> 복합 or 단일 </summary>
        public bool isComposite;

        /// <summary> 액션의 입력 키 정보 리스트 </summary>
        public List<BindingPartInfo> parts;
    }

    [Serializable]
    public class BindingPartInfo {
        /// <summary> 복합키의 파트 이름(ex: "Up", "Down") *단일일 경우 비어 있음 </summary>
        public string compositePartName;

        /// <summary> 실제 입력 키(ex: "W", "Space") </summary>
        public string key;
    }
}