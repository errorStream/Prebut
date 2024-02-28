using UnityEngine;
using UnityEditor;

namespace Prebut
{
    [CustomEditor(typeof(SetupPreRenderedBackground))]
    public class SetupPreRenderedBackgroundEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            // if (GUILayout.Button("Setup in Editor"))
            // {
            //     ((SetupPreRenderedBackground)target).Setup();
            // }
        }
    }
}
