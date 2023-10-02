using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HybridJobs.UEditor
{
    [CustomEditor(typeof(GlobalHybridJob))]
    public class GlobalHybridJobEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Push Changes"))
            {
                GlobalHybridJob instance = (GlobalHybridJob)target;
                instance.Editor_PushChanges();
            }            
            
            if (GUILayout.Button("Log System"))
            {
                GlobalHybridJob instance = (GlobalHybridJob)target;
                instance.Editor_LogSystem();
            }
        }
    }
}