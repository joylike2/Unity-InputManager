#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.IO;

namespace LifeLogs.InputSystem.Editor {
    internal class InputSystemSettings : EditorWindow {
        private const string INPUT_MANAGER_FOLDER_PATH = "Assets/InputManager";
        private const string INPUT_ACTIONS_FILE_PATH = "Assets/InputManager/NewInputSystemActions.inputactions";

        private InputActionAsset _inputActionsAsset;
        private string _newActionMapName = "";

        private Vector2 _scrollPos;

        public static bool IsBuildValidationEnabled = true;
        public static bool IsAutoEventSystemSettings = true;

        //상태 로그 메시지
        private string _statusLogMessage = "";
        private double _statusMessageTimestamp;
        private const double STATUS_MESSAGE_DURATION = 8f; //상태 로그 유지 시간

        // 액션맵의 펼침 상태 저장용 Dictionary 추가
        private Dictionary<string, bool> _actionMapFoldouts = new Dictionary<string, bool>();

        [MenuItem("Tools/InputManager/Settings")]
        public static void ShowWindow() => GetWindow<InputSystemSettings>("Input System Settings");

        private void OnEnable() {
            IsBuildValidationEnabled = EditorPrefs.GetBool("IsBuildValidationEnabled", true);
            IsAutoEventSystemSettings = EditorPrefs.GetBool("IsAutoEventSystemSettings", true);
            LoadInputActionAsset();
        }

        private void LoadInputActionAsset() => _inputActionsAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(INPUT_ACTIONS_FILE_PATH);

        private void OnGUI() {
            GUILayout.Label("🛠️ Input System 설정", EditorStyles.boldLabel);

            if (!_inputActionsAsset) {
                GUILayout.Space(10);
                GUILayout.Label("🔴 설정 파일이 없습니다.", EditorStyles.helpBox);
                GUILayout.Space(4);
                if (GUILayout.Button("⚙️ 셋업 (설정 파일 생성)", GUILayout.Height(40))) {
                    SetupInputActions();
                }

                DrawStatusLog();
                return;
            }

            //파일이 있으면 여기서부터 보여줌
            GUILayout.Space(10);
            GUILayout.Label("🟢 설정 파일 경로:", EditorStyles.label);
            GUILayout.Label(INPUT_ACTIONS_FILE_PATH, EditorStyles.helpBox);
            GUILayout.Space(2);
            EditorGUILayout.ObjectField("Input Actions Asset", _inputActionsAsset, typeof(InputActionAsset), false);
            GUILayout.Space(10);

            EditorGUI.BeginChangeCheck();

            DrawBuildValidationSystemToggle();
            
            DrawAutoEventSystemToggle();

            GUILayout.Space(28);

            GUILayout.Label("🔹 액션맵(ActionMap) 추가", EditorStyles.boldLabel);
            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            _newActionMapName = EditorGUILayout.TextField(_newActionMapName);

            if (GUILayout.Button("액션맵 추가", GUILayout.Width(100))) {
                AddActionMap();
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.Label("- 현재 액션맵 목록:", EditorStyles.boldLabel);
            GUILayout.Space(4);
            DrawActionMapList();

            GUILayout.Space(40);

            DrawStatusLog(); //상태 로그 출력
        }

        #region Set Input Actions

        /// <summary> 초기 설정 생성 </summary>
        private void SetupInputActions() {
            if (!AssetDatabase.IsValidFolder(INPUT_MANAGER_FOLDER_PATH)) {
                Directory.CreateDirectory(INPUT_MANAGER_FOLDER_PATH);
                AssetDatabase.Refresh();
            }
            
            IsBuildValidationEnabled = true;
            EditorPrefs.SetBool("IsBuildValidationEnabled", true);
            
            IsAutoEventSystemSettings = true;
            EditorPrefs.SetBool("IsAutoEventSystemSettings", true);
            
            InputActionAsset asset = Resources.Load<InputActionAsset>("Dufault_NewInputSystemActions");
            
            //InputActionAsset 생성
            string json = asset.ToJson();

            //파일로 JSON 직접 작성
            File.WriteAllText(INPUT_ACTIONS_FILE_PATH, json);
            AssetDatabase.ImportAsset(INPUT_ACTIONS_FILE_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _inputActionsAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(INPUT_ACTIONS_FILE_PATH);
            UnityEngine.InputSystem.InputSystem.actions = _inputActionsAsset;
            SetStatusLog("✅ 설정 파일이 생성되었습니다.");
            // EditorUtility.DisplayDialog("완료", "파일이 생성되었습니다.", "확인");
        }

        /// <summary> 액션맵 추가 </summary>
        private void AddActionMap() {
            if (string.IsNullOrEmpty(_newActionMapName)) {
                SetStatusLog("⚠️ 액션맵 이름을 입력하세요.");
                // EditorUtility.DisplayDialog("오류", "액션맵 이름을 입력하세요.", "확인");
                return;
            }

            if (_inputActionsAsset.FindActionMap(_newActionMapName) is not null) {
                SetStatusLog("⚠️ 이미 존재하는 액션맵 이름입니다.");
                // EditorUtility.DisplayDialog("오류", "이미 존재하는 액션맵 이름입니다.", "확인");
                return;
            }

            _inputActionsAsset.AddActionMap(_newActionMapName);

            string json = _inputActionsAsset.ToJson();
            File.WriteAllText(INPUT_ACTIONS_FILE_PATH, json);
            AssetDatabase.ImportAsset(INPUT_ACTIONS_FILE_PATH);
            AssetDatabase.Refresh();

            SetStatusLog($"✅ 액션맵 '{_newActionMapName}'이 추가되었습니다.");
            // Debug.Log($"새로운 액션맵 '{_newActionMapName}' 이 추가되었습니다.");
            _newActionMapName = "";
            GUI.FocusControl(null); //에디터 GUI 포커스 해제
        }

        /// <summary> 액션맵 & 액션 목록 표시 및 삭제 </summary>
        private void DrawActionMapList() {
            if (_inputActionsAsset.actionMaps.Count == 0) {
                GUILayout.Label("현재 액션맵이 없습니다.", EditorStyles.helpBox);
                return;
            }

            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(360));

            foreach (var actionMap in _inputActionsAsset.actionMaps) {
                GUILayout.BeginVertical("box");

                GUILayout.BeginHorizontal();
                GUILayout.Label($"• {actionMap.name}", EditorStyles.boldLabel);

                // 펼침 토글 버튼 추가
                if (!_actionMapFoldouts.ContainsKey(actionMap.name)) {
                    _actionMapFoldouts[actionMap.name] = false;
                }

                if (GUILayout.Button(_actionMapFoldouts[actionMap.name] ? "🔼 닫기" : "🔽 액션 보기", GUILayout.Width(92))) {
                    _actionMapFoldouts[actionMap.name] = !_actionMapFoldouts[actionMap.name];
                }

                if (GUILayout.Button("삭제", GUILayout.Width(50))) {
                    RemoveActionMap(actionMap.name);
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    break;
                }
                GUILayout.EndHorizontal();

                // 액션 리스트를 펼쳐서 보여주는 부분
                if (_actionMapFoldouts[actionMap.name]) {
                    GUILayout.Space(4);
                    // 액션이 없을 때 명확히 표시
                    if (actionMap.actions.Count == 0) {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(20); // 들여쓰기
                        GUILayout.Label("❗액션이 없습니다.", EditorStyles.miniLabel);
                        GUILayout.EndHorizontal();
                    } else {
                        foreach (var action in actionMap.actions) {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(20); // 들여쓰기
                            GUILayout.Label($"- {action.name} ({action.type})");
                            GUILayout.EndHorizontal();
                        }
                    }
                    GUILayout.Space(4);
                }

                GUILayout.EndVertical();
                GUILayout.Space(4);
            }

            GUILayout.EndScrollView();
        }
        
        /// <summary> 액션맵 삭제 </summary>
        private void RemoveActionMap(string actionMapName) {
            InputActionMap map = _inputActionsAsset.FindActionMap(actionMapName);
            if (map is null) return;

            _inputActionsAsset.RemoveActionMap(map);

            string json = _inputActionsAsset.actionMaps.Count > 0
                ? _inputActionsAsset.ToJson()
                : @"{
                        ""name"": ""PlayerControls"",
                        ""maps"": [],
                        ""controlSchemes"": []
                        }";

            File.WriteAllText(INPUT_ACTIONS_FILE_PATH, json);
            AssetDatabase.ImportAsset(INPUT_ACTIONS_FILE_PATH);
            AssetDatabase.Refresh();

            SetStatusLog($"✅ 액션맵 '{actionMapName}'이 삭제되었습니다.");
            // Debug.Log($"액션맵 '{actionMapName}'이 삭제되었습니다.");
        }

        #endregion Set Input Actions

        #region Build Validation System
        
        /// <summary> 빌드시 시스템 체크 토글 상태 </summary>
        private void DrawBuildValidationSystemToggle() {
            if (_inputActionsAsset is null) return;

            GUILayout.Space(10);
            GUILayout.Label("🔹 빌드 오류 체크를 활성화할지 여부", EditorStyles.boldLabel);
            GUILayout.Space(4);
            EditorGUI.BeginChangeCheck();
            
            IsBuildValidationEnabled = EditorGUILayout.Toggle("빌드 오류 체크 사용", IsBuildValidationEnabled);
            
            if (EditorGUI.EndChangeCheck()) {
                EditorPrefs.SetBool("IsBuildValidationEnabled", IsBuildValidationEnabled);
                SetStatusLog($"🔄 빌드 오류 검사 기능이 {(IsBuildValidationEnabled ? "활성화" : "비활성화")} 되었습니다.");
            }
        }

        #endregion Build Validation System
        
        #region Auto Change Input Actions Asset

        /// <summary> 자동 변경 토글 상태 </summary>
        private void DrawAutoEventSystemToggle() {
            if (_inputActionsAsset is null) return;

            GUILayout.Space(10);
            GUILayout.Label("🔹 EventSystem 자동설정", EditorStyles.boldLabel);
            GUILayout.Space(4);
            EditorGUI.BeginChangeCheck();
            
            IsAutoEventSystemSettings = EditorGUILayout.Toggle("자동설정 사용", IsAutoEventSystemSettings);
            
            if (EditorGUI.EndChangeCheck()) {
                EditorPrefs.SetBool("IsAutoEventSystemSettings", IsAutoEventSystemSettings);
                SetStatusLog($"🔄 EventSystem 자동설정이 {(IsAutoEventSystemSettings ? "활성화" : "비활성화")} 되었습니다.");
                // Debug.Log($"EventSystem 자동설정이 {(isAutoEventSystemSettings ? "활성화" : "비활성화")}되었습니다.");
                if (IsAutoEventSystemSettings) {
                    EditorEventSystemAssetSetter.ChangeInputActions();
                }
            }
        }

        #endregion Auto Change Input Actions Asset

        #region Input System Logs

        /// <summary> 상태 로그 입력 </summary>
        private void SetStatusLog(string message) {
            _statusLogMessage = message;
            _statusMessageTimestamp = EditorApplication.timeSinceStartup;
        }

        /// <summary> 상태 로그 출력 </summary>
        private void DrawStatusLog() {
            if (string.IsNullOrEmpty(_statusLogMessage)
                || !((EditorApplication.timeSinceStartup - _statusMessageTimestamp) <= STATUS_MESSAGE_DURATION)) return;

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(_statusLogMessage, MessageType.Info);
            Repaint(); //즉시 GUI 갱신
        }

        #endregion Input System Logs
    }
}

#endif