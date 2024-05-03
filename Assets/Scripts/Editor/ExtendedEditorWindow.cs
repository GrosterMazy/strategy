using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ExtendedEditorWindow : EditorWindow
{
    protected SerializedObject serializedObject;
    protected Caster caster;
    protected CasterEditorWindow window;
    protected SerializedProperty currentProperty;

    private Dictionary<int, bool> destroySpell = new Dictionary<int, bool>();
    private int helperDestroySpellDictInit = 0;

    private string selectedPropertyPath;
    protected SerializedProperty selectedProperty;

    protected void DrawProperties(SerializedProperty prop, bool drawChildren)
    {
        string oldSelectedPropertyPath = prop.propertyPath;
        List<string> alreadyDrawPropertyPaths = new List<string>();
        foreach (SerializedProperty p in prop)
        {
            if (p.name.Contains("_Header_") || p.name.Contains("_EndHeader_")) continue;
            if (p.isArray && p.propertyType == SerializedPropertyType.Generic)
            {
                EditorGUILayout.BeginHorizontal();
                p.isExpanded = EditorGUILayout.Foldout(p.isExpanded, p.name);
                EditorGUILayout.EndHorizontal();

                if (p.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    DrawProperties(p, drawChildren);
                    EditorGUI.indentLevel--;
                }
                foreach (SerializedProperty child in p)
                {
                    alreadyDrawPropertyPaths.Add(child.propertyPath);
                }
            }
            else
            {
                if (alreadyDrawPropertyPaths.Contains(p.propertyPath)) { continue; }
                EditorGUILayout.PropertyField(p, false);
            }
            alreadyDrawPropertyPaths.Add(p.propertyPath);
        }
        selectedProperty = serializedObject.FindProperty(oldSelectedPropertyPath);
    }

    protected void DrawMainProperties_name(SerializedProperty prop)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Name", GUILayout.MaxWidth(50));
        prop.FindPropertyRelative("Name").stringValue = EditorGUILayout.TextField(prop.FindPropertyRelative("Name").stringValue);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("Description");
        prop.FindPropertyRelative("Description").stringValue = EditorGUILayout.TextArea(prop.FindPropertyRelative("Description").stringValue, GUILayout.MaxWidth(600));
    }

    protected void DrawMainProperties(SerializedProperty prop, bool drawChildren)
    {
        string oldSelectedPropertyPath = prop.propertyPath;
        List<string> alreadyDrawPropertyPaths = new List<string>();
        foreach (SerializedProperty p in prop)
        {
            if (p.name.Contains("_Header_"))
            {
                break;
            }
            if (p.isArray && p.propertyType == SerializedPropertyType.Generic)
            {
                EditorGUILayout.BeginHorizontal();
                p.isExpanded = EditorGUILayout.Foldout(p.isExpanded, p.name);
                EditorGUILayout.EndHorizontal();

                if (p.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    DrawProperties(p, drawChildren);
                    EditorGUI.indentLevel--;
                }
                else
                {
                    foreach (SerializedProperty child in p)
                    {
                        alreadyDrawPropertyPaths.Add(child.propertyPath);
                    }
                }
            }
            else
            {
                if (alreadyDrawPropertyPaths.Contains(p.propertyPath)) { continue; }
                EditorGUILayout.PropertyField(p, false);
            }
            alreadyDrawPropertyPaths.Add(p.propertyPath);
        }
        selectedProperty = serializedObject.FindProperty(oldSelectedPropertyPath);
    }

    protected void DrawSidebar(SerializedProperty prop)
    {
        int spellIndex = 0;
        if (helperDestroySpellDictInit == 0) // Инициализация словаря
        {
            for (int i = 0; i < 50; i++)
            {
                destroySpell[i] = false;
            }
        }
        helperDestroySpellDictInit += 1;

        EditorGUILayout.LabelField("Spells:");
        foreach (SerializedProperty p in prop)
        {

            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("X", GUILayout.MaxWidth(20)))
            {
                destroySpell[spellIndex] = !destroySpell[spellIndex];
            }
            if (destroySpell[spellIndex])
            {
                EditorGUILayout.LabelField("Are u sure?", GUILayout.MaxWidth(65));
                if (GUILayout.Button("V", GUILayout.MaxWidth(20)))
                {
                    caster.SpellsList.RemoveAt(spellIndex);
                    ReCreateWindow(window, caster);
                }
            }
            if (GUILayout.Button(p.displayName))/////////////////
            {
                selectedPropertyPath = p.propertyPath;
            }
            EditorGUILayout.EndHorizontal();
            spellIndex++;
        }

        if (!string.IsNullOrEmpty(selectedPropertyPath))
        {
            selectedProperty = serializedObject.FindProperty(selectedPropertyPath);
        }

        GUILayout.Space(15);

        if (GUILayout.Button("+", GUILayout.MinHeight(45)))
        {
            caster.SpellsList.Add(null);
            ReCreateWindow(window, caster);
        }
    }
    private void SetSpellsCount(int spellsCount)
    {
        while (caster.SpellsList.Count < spellsCount)
        {
            caster.SpellsList.Add(null);
        }
        while (caster.SpellsList.Count > spellsCount)
        {
            caster.SpellsList.RemoveAt(caster.SpellsList.Count - 1);
        }
    }

    protected void DrawField(string propName, bool relative)
    {
        if (relative && currentProperty != null)
        {
            EditorGUILayout.PropertyField(currentProperty.FindPropertyRelative(propName), true);
        }
        else if (serializedObject != null)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(propName), true);
        }
    }

    protected void DrawHeaders(SerializedProperty prop)
    {
        string oldSelectedPropertyPath = prop.propertyPath;
        List<string> alreadyDrawPropertyPaths = new List<string>();
        bool lastExpanded = false;
        foreach (SerializedProperty p in prop)
        {
            if (p.name.Contains("_Header_"))
            {
                EditorGUILayout.BeginVertical("box");
                p.isExpanded = EditorGUILayout.Foldout(p.isExpanded, p.name.Substring(0, p.name.Length - 8));
                lastExpanded = p.isExpanded;
            }
            if (p.name.Contains("_EndHeader_"))
            {
                EditorGUILayout.EndVertical();
                lastExpanded = false;
            }
            if ((lastExpanded && !p.name.Contains("_Header_") && p.isArray && p.propertyType == SerializedPropertyType.Generic) || p.name.Contains("_DrawAnyway_"))
            {
                DrawDefaultArrays(p, alreadyDrawPropertyPaths);
            }
            else if (lastExpanded && !p.name.Contains("_Header_") || p.name.Contains("_DrawAnyway_"))
            {
                if (alreadyDrawPropertyPaths.Contains(p.propertyPath)) { continue; }
                if (p.name.Contains("_DrawAnyway_")) { EditorGUILayout.PropertyField(p, new GUIContent(p.name.Substring(0, p.name.Length - 12)), false); }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(p.name, GUILayout.MinWidth(300));
                    EditorGUILayout.PropertyField(p, new GUIContent(""), false);
                    EditorGUILayout.EndHorizontal();
                }
            }
            alreadyDrawPropertyPaths.Add(p.propertyPath);
            
        }
        selectedProperty = serializedObject.FindProperty(oldSelectedPropertyPath);
    }

    protected void Apply()
    {
        serializedObject.ApplyModifiedProperties();
    }
    
    protected void ReCreateWindow(CasterEditorWindow window, Caster caster)
    {
        DestroyImmediate(window);
        CasterEditorWindow.Open(caster);
        for (int i = 0; i < 50; i++)
        {
            destroySpell[i] = false;
        }
    }

    protected void DrawDefaultArrays(SerializedProperty p, List<string> alreadyDrawPropertyPaths)
    {
        EditorGUILayout.BeginHorizontal();
        if (p.name.Contains("_DrawAnyway_")) { p.isExpanded = EditorGUILayout.Foldout(p.isExpanded, p.name.Substring(0, p.name.Length - 12)); }
        else { p.isExpanded = EditorGUILayout.Foldout(p.isExpanded, p.name); }
        EditorGUILayout.EndHorizontal();

        if (p.isExpanded)
        {
            EditorGUI.indentLevel++;
            DrawProperties(p, true);
            EditorGUI.indentLevel--;
        }
        foreach (SerializedProperty child in p)
        {
            alreadyDrawPropertyPaths.Add(child.propertyPath);
        }
    }
}
