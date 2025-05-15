using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LifeLogs.InputSystem;

public class MenuViewInputSample : MonoBehaviour {
    private void Start() {
        SetInputKeyText();
        InputManager.Instance.onDeviceChanged += OnDeviceChanged;
    }

    #region Action List View

    public Text checkActionsText;

    /// <summary> 액션맵의 액션을 조회 합니다. 현재 디바이스를 기준으로 반환합니다. </summary>
    public void CheckActionMapButtonAction() {
        checkActionsText.text = "";

        List<string> actionMapNames = InputManager.Instance.GetActionMapNames();
        string actionMapName = "";
        foreach (var mapName in actionMapNames) {
            actionMapName += "   · " + mapName + "\n";
        }

        // ···•●☉
        checkActionsText.text += $"============================\n";
        checkActionsText.text += $" ● 액션맵 리스트:\n";
        checkActionsText.text += $"{actionMapName}";
        checkActionsText.text += $"============================\n";

        foreach (var mapName in actionMapNames) {
            InputActionInfo actionInfo = InputManager.Instance.GetActionInfo(mapName);
            checkActionsText.text += $"\n============================\n";
            checkActionsText.text += $" ☉ 액션맵: {actionInfo.actionMapName}\n";
            checkActionsText.text += $"============================\n";
            int i = 0;
            foreach (var deviceName in actionInfo.GetDeviceNameList()) {
                i++;
                checkActionsText.text += $"{i}. {deviceName}\n";

                DeviceBindingInfo data = actionInfo.devices[deviceName];
                foreach (var info in data.actions) {
                    checkActionsText.text += $"    • {info.actionName}\n";
                    if (info.isComposite) {
                        foreach (var parts in info.parts) {
                            if (!string.IsNullOrEmpty(parts.compositePartName)) {
                                checkActionsText.text += $"       BindingName: {parts.compositePartName}\n";
                            }

                            checkActionsText.text += $"       Key: {parts.key}\n";
                        }
                    }
                    else {
                        BindingPartInfo parts = info.parts[0];
                        if (!string.IsNullOrEmpty(parts.compositePartName)) {
                            checkActionsText.text += $"       BindingName: {parts.compositePartName}\n";
                        }

                        checkActionsText.text += $"       Key: {parts.key}\n";
                    }
                }

                checkActionsText.text += $"\n";
            }
        }

        List<string> connectedInputDevices = InputManager.Instance.GetConnectedInputDevices();
        checkActionsText.text += $"\n\n\n============================\n";
        checkActionsText.text += $" ● 현재 연결된 디바이스: 총 {connectedInputDevices.Count}개\n";
        foreach (var connected in connectedInputDevices) {
            checkActionsText.text += "   · " + connected + "\n";
        }

        checkActionsText.text += $"============================\n";
    }

    #endregion Action List View

    #region Rebinding View

    [Space] public GameObject infoTextObjA;
    public GameObject infoTextObjB;

    public Text jumpBtnText;

    [Space] public BindingPartInfoScript jumpScript;

    private void SetInputKeyText() {
        List<string> actionMaps = InputManager.Instance.GetConnectedActionMapNames();
        foreach (var actionMap in actionMaps) {
            if (actionMap == "Player") {
                InputActionInfo actionInfo = InputManager.Instance.GetConnectedActionInfo(actionMap);
                foreach (var action in actionInfo.devices["Keyboard"].actions) {
                    
                    if (action.actionName == "Jump") {
                        jumpBtnText.text = action.parts[0].key;
                        jumpScript.actionMapName = actionMap;
                        jumpScript.deviceType = "Keyboard";
                        jumpScript.actionName = action.actionName;
                        jumpScript.compositePartName = "";
                        jumpScript.key = action.parts[0].key;
                        break;
                    }

                    //복합형일 경우 참고용.
                    // if (action.actionName == "Move") {
                    //     foreach (var bpInfo in action.parts) {
                    //         if (bpInfo.compositePartName == "up") {
                    //             jumpBtnText.text = bpInfo.key;
                    //             jumpScript.actionMapName = actionMap;
                    //             jumpScript.deviceType = "Keyboard";
                    //             jumpScript.actionName = action.actionName;
                    //             jumpScript.compositePartName = bpInfo.compositePartName;
                    //             jumpScript.key = bpInfo.key;
                    //             break;
                    //         }
                    //     }
                    //
                    //     break;
                    // }
                }

                break;
            }
        }
    }

    public void JumpButtonAction() {
        infoTextObjA.SetActive(false);
        infoTextObjB.SetActive(true);
        
        InputManager.Instance.StartRebinding(jumpScript.actionMapName, jumpScript.deviceType, jumpScript.actionName, jumpScript.key, jumpScript.compositePartName, (isResult, partKey, changePartKey) => {
            Debug.Log($"Jump- isResult: {isResult}, partKey: {partKey}, changePartKey: {changePartKey}");
            SetInputKeyText();
            infoTextObjB.SetActive(false);
            infoTextObjA.SetActive(true);
        });
        
        //복합형일 경우 참고용.
        // InputManager.Instance.StartRebinding(jumpScript.actionMapName, jumpScript.deviceType, jumpScript.actionName, jumpScript.key, jumpScript.compositePartName, (isResult, partKey, changePartKey) => {
        //     Debug.Log($"Up- isResult: {isResult}, partKey: {partKey}, changePartKey: {changePartKey}");
        //     SetInputKeyText();
        //     infoTextObjB.SetActive(false);
        //     infoTextObjA.SetActive(true);
        // });
    }

    #endregion Rebinding View

    #region Device Connect Info View

    [Space] public Text deviceInfoText;

    private void OnDeviceChanged(bool isConnect, string deviceType) {
        deviceInfoText.text = $"연결유무: {isConnect}, 디바이스: {deviceType}";
    }

    #endregion Device Connect Info View
}