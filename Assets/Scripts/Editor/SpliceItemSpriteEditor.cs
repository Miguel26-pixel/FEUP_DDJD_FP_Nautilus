using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpliceItemSprite), true)]
public class SpliceItemSpriteEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SpliceItemSprite spliceItemSprite = (SpliceItemSprite)target;
        if (GUILayout.Button("Splice"))
        {
            spliceItemSprite.Splice();
        }
    }
}