using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR 
    using UnityEditor;

[CustomEditor(typeof(Portal))]
public class PortalEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Portal myScript = target as Portal;
        if (myScript.oneWay)
        {
            serializedObject.Update();
            var flipSideContent = new GUIContent("FlipSide", "Flip from which side you can enter from");
            myScript.flipSide = EditorGUILayout.Toggle(flipSideContent, myScript.flipSide);
            serializedObject.ApplyModifiedProperties();
        }

    }
}
#endif