using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

namespace Custom_Inspector
{
    [CustomEditor(typeof(BreakableBlockManager))]
    public class BreakableBlockManager_Inspector : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Generate Blocks"))
            {
                ((BreakableBlockManager)target).GenerateBlocks();
            }
        }
    }
}

#endif