#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OutlineGenerator))]
public class OutlineGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default Inspector
        DrawDefaultInspector();

        // Add the Generate Outline button
        OutlineGenerator generator = (OutlineGenerator)target;
        if (GUILayout.Button("Generate Outline"))
        {
            //generator.GenerateOutline();
        }
    }
}
#endif
