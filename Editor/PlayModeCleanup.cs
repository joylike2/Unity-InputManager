#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace LifeLogs.InputSystem.Editor {
    [InitializeOnLoad]
    public static class PlayModeCleanup {
        static PlayModeCleanup() {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state) {
            if (state == PlayModeStateChange.ExitingPlayMode) {
                var leftover = GameObject.Find("[InputManager]");
                if (leftover != null) {
                    Object.DestroyImmediate(leftover);
                }
            }
        }
    }
}
#endif