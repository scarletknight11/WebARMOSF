using UnityEditor;
using UnityEngine;

namespace WebARFoundation {
    public class FaceTrackingDebugWindow : EditorWindow {

        int targetIndex = 0;

        [MenuItem("Window/WebAR/Face Tracking Debugger")]
        static void OpenCustomEditorWindow() {
            FaceTrackingDebugWindow window = GetWindow<FaceTrackingDebugWindow>();
            window.Show();
        }

        void OnGUI() {
            if (GUILayout.Button("trigger face detected")) {
                MindARFacePlugin.OnARUpdate(1);
            }

            if (GUILayout.Button("trigger face lost")) {
                MindARFacePlugin.OnARUpdate(0);
            }
        }
}
}