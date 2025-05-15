#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;

namespace LifeLogs.InputSystem.Editor {
    public class InputSystemBuildValidator : MonoBehaviour {
        public int callbackOrder => 0;

        private const string INPUT_ACTIONS_FILE_PATH = "Assets/InputManager/NewInputSystemActions.inputactions";

        public void OnPreprocessBuild(BuildReport report) {
            if (!InputSystemSettings.IsBuildValidationEnabled) return;
            
            InputActionAsset expectedAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(INPUT_ACTIONS_FILE_PATH);
            InputActionAsset projectAsset = UnityEngine.InputSystem.InputSystem.actions;

            if (expectedAsset == null) {
                Debug.LogError("[빌드 오류] 지정된 InputActions 파일을 찾을 수 없음\n" +
                               $"경로 확인: {INPUT_ACTIONS_FILE_PATH}");
                throw new BuildFailedException("InputActions 파일이 누락됨");
            }

            if (projectAsset == null) {
                Debug.LogError("[빌드 오류] 프로젝트 설정에서 InputActionAsset을 찾을 수 없음");
                throw new BuildFailedException("프로젝트 InputActionAsset 설정이 누락됨");
            }

            if (!IsAssetMatch(expectedAsset, projectAsset)) {
                Debug.LogError("[빌드 오류] 프로젝트 설정과 실제 InputActions 파일이 불일치함\n" +
                               "설정 파일과 프로젝트 설정 확인이 필요함");
                throw new BuildFailedException("InputActions 파일과 프로젝트 설정이 불일치함");
            }

            Debug.Log("[빌드 검증] InputActionAsset 검증 성공. 빌드를 계속합니다.");
        }

        private bool IsAssetMatch(InputActionAsset expectedAsset, InputActionAsset projectAsset) {
            return expectedAsset.ToJson() == projectAsset.ToJson();
        }
    }
}

#endif