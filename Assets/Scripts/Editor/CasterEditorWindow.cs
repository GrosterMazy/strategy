using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class CasterEditorWindow : ExtendedEditorWindow
{
    public static void Open(Caster _caster)
    {
        CasterEditorWindow window = GetWindow<CasterEditorWindow>(_caster.gameObject.name + " Spells Editor");
        window.serializedObject = new SerializedObject(_caster);
        window.caster = _caster;
    }

    private void OnGUI()
    {
        DrawWindow();
    }

    private void DrawSelectedPropertiesPanel()
    {
        
        DrawMainProperties_name(selectedProperty);
        //EditorGUILayout.BeginHorizontal();
        DrawHeaders(selectedProperty);
        //EditorGUILayout.EndHorizontal();
        

        //DrawProperties(selectedProperty, true);

    }


    public void DrawWindow()
    {
        currentProperty = serializedObject.FindProperty("SpellsList");

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true));
        DrawSidebar(currentProperty);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));
        if (selectedProperty != null)
        {
            DrawSelectedPropertiesPanel();
        }
        else
        {
            EditorGUILayout.LabelField("Select a spell from the list");
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        Apply();
    }
}