using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

namespace LifeLogs.InputSystem {
    internal static class InputDeviceConnector {
        private static InputActionAsset _inputActionsAsset;
        private static bool _isInit = false;
        private static List<string> _cachedConnectedDevices;
        private static InputActionMapInfo _cachedConnectedInputActionMapInfo;
        
        private static IInputDeviceConnectorDelegate DeviceChangedDelegate { get; set; }
        
        public static void Init(InputActionAsset inputActionAsset, IInputDeviceConnectorDelegate delegateInstance) {
            _inputActionsAsset = inputActionAsset;
            DeviceChangedDelegate = delegateInstance;
            
            _cachedConnectedDevices = new List<string>();
            UpdateDeviceList();
            UpdateInputActions();
            UnityEngine.InputSystem.InputSystem.onDeviceChange += OnDeviceChange;
            _isInit = true;
        }

        private static void OnDeviceChange(InputDevice device, InputDeviceChange change) {
            if (change == InputDeviceChange.Added || change == InputDeviceChange.Removed) {
                UpdateDeviceList();
                UpdateInputActions();
                
                // string temp = $"====    •총개수: [{_cachedConnectedDevices.Count}]    •현재연결된 디바이스: [";
                // foreach (var ss in _cachedConnectedDevices) {
                //     temp += ss + ", ";
                // }
                // temp = temp.Remove(temp.Length - 2, 2);
                // temp += "]    •현재연결된 디바이스의 액션맵: [";
                // foreach (var ss in _cachedConnectedInputActionMapInfo.GetActionMapNameList()) {
                //     temp += ss + ", ";
                // }
                // temp = temp.Remove(temp.Length - 2, 2);
                // temp += "]    ====";
                // Debug.Log(temp);
                
                if (_isInit) {
                    DeviceChangedDelegate.OnDeviceChanged(change == InputDeviceChange.Added ? true : false, GetDeviceType(device));
                }
            }
        }

        private static void UpdateDeviceList() {
            _cachedConnectedDevices.Clear();
            foreach (var device in UnityEngine.InputSystem.InputSystem.devices) {
                string typeName = GetDeviceType(device);
                if (!_cachedConnectedDevices.Contains(typeName))
                {
                    _cachedConnectedDevices.Add(typeName);
                }
            }
        }

        private static void UpdateInputActions() {
            _cachedConnectedInputActionMapInfo = new InputActionMapInfo();
            foreach (var deviceType in _cachedConnectedDevices) {
                List<string> actionMapNames = InputActionBindingResolver.GetInputDeviceActionMapNames(deviceType);
                foreach (var actionMapName in actionMapNames) {
                    if (_cachedConnectedInputActionMapInfo.actionMaps.ContainsKey(actionMapName)) continue;
                    InputActionInfo resultInfo = InputActionBindingResolver.GetActionInfo(actionMapName);
                    _cachedConnectedInputActionMapInfo.actionMaps.Add(actionMapName, resultInfo);
                }
            }
        }
        
        private static string GetDeviceType(InputDevice device) {
            return device switch {
                       Keyboard => "Keyboard", 
                       Mouse => "Mouse",
                       Gamepad => "Gamepad",
                       XRController => "XRController",
                       Joystick => "Joystick", 
                       Touchscreen => "Touchscreen",
                       Pen => "Pen",
                       Pointer => "Pointer",
                       Sensor => "Sensor",
                       XRHMD => "XRHMD",
                       TrackedDevice => "TrackedDevice",
                       _ => "Other"
                   };
        }

        public static void ResetRebinds(InputActionAsset inputActionAsset) {
            _inputActionsAsset = inputActionAsset;
            UpdateInputActions();
        }

        public static void ChangeBindingKey(string actionMapName, string deviceType, string actionName, string targetKey, string newReadableKey) {
            List<ActionBindingInfo> actions = _cachedConnectedInputActionMapInfo.actionMaps[actionMapName].devices[deviceType].actions;
            foreach (var abInfo in actions) {
                if (abInfo.actionName.Equals(actionName)) {
                    foreach (var part in abInfo.parts) {
                        if (part.key.Equals(targetKey)) {
                            part.key = newReadableKey;
                            break;
                        }
                    }
                    break;
                }
            }
        }

        #region Get
        
        /// <summary> 중복 키 체크 </summary>
        public static bool IsDuplicateBinding(string actionMapName, string deviceType, string actionName, string readableKey) {
            InputActionInfo iaInfo = _cachedConnectedInputActionMapInfo.actionMaps[actionMapName];
            DeviceBindingInfo dbInfo = iaInfo.devices[deviceType];
            foreach (var abInfo in dbInfo.actions) {
                if (abInfo.actionName.Equals(actionName)) continue;

                foreach (var bpInfo in abInfo.parts) {
                    if (!bpInfo.key.Equals(readableKey)) continue;
                    return true;
                }
            }

            return false;
        }
        
        /// <summary> 현재 연결된 디바이스 리스트 </summary>
        public static List<string> GetConnectedInputDevices() => _cachedConnectedDevices;
        /// <summary> 현재 연결된 디바이스 인지 확인 </summary>
        public static bool IsConnectedInputDevice(string deviceType) => _cachedConnectedDevices.Contains(deviceType);
        
        /// <summary> 현재 연결된 액션맵 리스트 반환 </summary>
        public static List<string> GetConnectedActionMapNames() => _cachedConnectedInputActionMapInfo.GetActionMapNameList();
        /// <summary> 현재 연결된 액션맵 인지 확인 </summary>
        public static bool IsConnectedInputActionMap(string actionMapName) => _cachedConnectedInputActionMapInfo.actionMaps.ContainsKey(actionMapName);
        /// <summary> 현재 연결된 액션맵 정보 반환 </summary>
        public static InputActionInfo GetConnectedActionInfo(string actionMapName) => _cachedConnectedInputActionMapInfo.actionMaps[actionMapName];
        /// <summary> 현재 연결된 액션맵이 가진 디바이스 리스트 반환 </summary>
        public static List<string> GetConnectedActionMapDevices(string actionMapName) => _cachedConnectedInputActionMapInfo.actionMaps[actionMapName].GetDeviceNameList();
        /// <summary> 현재 연결된 액션맵의 디바이스의 바인딩 정보 반환 </summary>
        public static DeviceBindingInfo GetConnectedActionMapDeviceBindingInfo(string actionMapName, string deviceName) => _cachedConnectedInputActionMapInfo.actionMaps[actionMapName].devices[deviceName];

        
        #endregion Get
    }
}
