using UnityEditor;
using UnityEngine;

namespace Custom_Inspector
{
    [CustomEditor(typeof(EmptySpriteCreator))]
    public class EmptySpriteCreator_Inspector : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Create Empty Sprite"))
            {
                ((EmptySpriteCreator)target).CreateEmptySprite();
            }
        }
    }
}
