/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Caster))]
public class CasterEditorTest : Editor
{
    private Caster _caster;
    private SerializedObject serializedObject;
    private SerializedProperty mainProperty;
    private void OnEnable()
    {
        _caster = (Caster)target;
        serializedObject = new SerializedObject(_caster);
        mainProperty = serializedObject.FindProperty("SpelsList");
    }
    public override void OnInspectorGUI()
    {
        DrawProperties(mainProperty, true);
    }

    private void DrawProperties(SerializedProperty prop, bool drawChildren)
    {
        string lastPropertyPath = string.Empty;
        foreach (SerializedProperty p in prop)
        {
            if (p.isArray && p.propertyType == SerializedPropertyType.Generic)
            {
                EditorGUILayout.BeginHorizontal();
                p.isExpanded = EditorGUILayout.Foldout(p.isExpanded, p.displayName);
                EditorGUILayout.EndHorizontal();

                if (p.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    DrawProperties(p, drawChildren);
                    EditorGUI.indentLevel--;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(lastPropertyPath) && p.propertyPath.Contains(lastPropertyPath)) { continue; }
                lastPropertyPath = p.propertyPath;
                EditorGUILayout.PropertyField(p, drawChildren);
            }
        }
    }
}
*/