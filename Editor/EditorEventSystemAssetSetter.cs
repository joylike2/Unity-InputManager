#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace LifeLogs.InputSystem.Editor {
    [InitializeOnLoad]
    internal static class EditorEventSystemAssetSetter {
        private const string INPUT_ACTIONS_FILE_PATH = "Assets/InputManager/NewInputSystemActions.inputactions";
        private static bool _isPendingChange;

        static EditorEventSystemAssetSetter() {
            // ReSharper disable once AccessToStaticMemberViaDerivedType
            EditorSceneManager.activeSceneChanged += OnSceneChanged;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        private static void OnSceneChanged(Scene oldScene, Scene newScene) => ChangeInputActions();

        private static void OnHierarchyChanged() {
            if (_isPendingChange) return;

            _isPendingChange = true;
            EditorApplication.delayCall += () => {
                ChangeInputActions();
                _isPendingChange = false;
            };
        }

        public static void ChangeInputActions() {
            InputActionAsset inputActionsAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(INPUT_ACTIONS_FILE_PATH);
            if (inputActionsAsset == null) return;
            if (!InputSystemSettings.IsAutoEventSystemSettings) return;

#if UNITY_2023_2_OR_NEWER
            EventSystem[] eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
#else
        EventSystem[] eventSystems = Object.FindObjectsOfType<EventSystem>();
#endif
            foreach (var eventSystem in eventSystems) {
                InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
                if (inputModule is not null && inputModule.actionsAsset != inputActionsAsset) {
                    inputModule.actionsAsset = inputActionsAsset;
                    EditorUtility.SetDirty(inputModule);
                    Debug.Log($"EventSystem: '{eventSystem.name}'에 액션 에셋이 자동 연결되었습니다.");
                }
            }
        }
    }
}
#endif