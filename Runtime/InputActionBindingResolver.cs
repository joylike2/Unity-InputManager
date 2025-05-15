using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Linq;

namespace LifeLogs.InputSystem {
    /// <summary> InputActions 정보 </summary>
    internal static class InputActionBindingResolver {
        private static InputActionAsset _inputActionsAsset;
        private static InputActionMapInfo _inputActionMapInfo;

        public static void Init(InputActionAsset inputActionAsset) {
            _inputActionsAsset = inputActionAsset;
            SetStructuredBinding();
        }

        /// <summary> 액션맵의 액션과 입력키 정보 셋팅 </summary>
        private static void SetStructuredBinding() {
            _inputActionMapInfo = new InputActionMapInfo();

            foreach (var actionMap in _inputActionsAsset.actionMaps) {
                InputActionInfo resultInfo = new InputActionInfo {
                    actionMapName = actionMap.name,
                    devices = new Dictionary<string, DeviceBindingInfo>()
                };

                foreach (var action in actionMap.actions) {
                    Dictionary<string, ActionBindingInfo> actionBindingsPerDevice = new Dictionary<string, ActionBindingInfo>();

                    BindingPartInfo partInfo;
                    foreach (var binding in action.bindings) {
                        if (binding.isComposite) continue;

                        string deviceType = GetDeviceTypeFromBindingPath(binding.effectivePath);
                        if (!actionBindingsPerDevice.ContainsKey(deviceType)) {
                            actionBindingsPerDevice[deviceType] = new ActionBindingInfo {
                                actionName = action.name,
                                isComposite = action.bindings.Any(b => b.isComposite),
                                parts = new List<BindingPartInfo>()
                            };
                        }

                        partInfo = new BindingPartInfo {
                            compositePartName = binding.isPartOfComposite ? binding.name : "",
                            key = InputControlPath.ToHumanReadableString(binding.effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice)
                        };

                        actionBindingsPerDevice[deviceType].parts.Add(partInfo);
                    }

                    //각 디바이스별로 액션 정보를 최종 추가
                    foreach (var kvp in actionBindingsPerDevice) {
                        if (!resultInfo.devices.ContainsKey(kvp.Key)) {
                            resultInfo.devices[kvp.Key] = new DeviceBindingInfo {
                                deviceType = kvp.Key,
                                actions = new List<ActionBindingInfo>()
                            };
                        }

                        resultInfo.devices[kvp.Key].actions.Add(kvp.Value);
                    }
                }

                _inputActionMapInfo.actionMaps[actionMap.name] = resultInfo;
            }
        }

        private static string GetDeviceTypeFromBindingPath(string bindingPath) {
            return bindingPath switch {
                       _ when bindingPath.StartsWith("<Keyboard>") => "Keyboard",
                       _ when bindingPath.StartsWith("<Mouse>") => "Mouse",
                       _ when bindingPath.StartsWith("<Gamepad>") => "Gamepad",
                       _ when bindingPath.StartsWith("<Joystick>") => "Joystick",
                       _ when bindingPath.StartsWith("<XRController>") => "XRController",
                       _ when bindingPath.StartsWith("<Touchscreen>") => "Touchscreen",
                       _ when bindingPath.StartsWith("<Pen>") => "Pen",
                       _ when bindingPath.StartsWith("<Pointer>") => "Pointer",
                       _ when bindingPath.StartsWith("<Sensor>") => "Sensor",
                       _ when bindingPath.StartsWith("<XRHMD>") => "XRHMD",
                       _ when bindingPath.StartsWith("<TrackedDevice>") => "TrackedDevice",
                       _ => "Other"
                   };
        }

        public static void ResetRebinds(InputActionAsset inputActionAsset) => Init(inputActionAsset);
        
        public static void ChangeBindingKey(string actionMapName, string deviceType, string actionName, string targetKey, string newReadableKey) {
            List<ActionBindingInfo> actions = _inputActionMapInfo.actionMaps[actionMapName].devices[deviceType].actions;
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
        
        /// <summary> 액션맵 리스트 반환 </summary>
        public static List<string> GetActionMapNames() => _inputActionMapInfo.GetActionMapNameList();

        /// <summary> 해당 액션맵 정보 반환 </summary>
        public static InputActionInfo GetActionInfo(string actionMapName) => _inputActionMapInfo.actionMaps[actionMapName];

        /// <summary> 해당 액션맵이 가진 디바이스 리스트 반환 </summary>
        public static List<string> GetDeviceNameList(string actionMapName) => _inputActionMapInfo.actionMaps[actionMapName].GetDeviceNameList();

        /// <summary> 해당 액션맵의 디바이스의 바인딩 정보 반환 </summary>
        public static DeviceBindingInfo GetDeviceBindingInfo(string actionMapName, string deviceType) => _inputActionMapInfo.actionMaps[actionMapName].devices[deviceType];

        /// <summary> 해당 디바이스 정보를 가진 액션맵 리스트 반환 </summary>
        public static List<string> GetInputDeviceActionMapNames(string deviceType) {
            List<string> result = new List<string>();
            foreach (var actionMap in _inputActionMapInfo.actionMaps) {
                if (actionMap.Value.devices.ContainsKey(deviceType)) {
                    result.Add(actionMap.Key);
                }
            }

            return result;
        }

        #endregion Get
    }
}