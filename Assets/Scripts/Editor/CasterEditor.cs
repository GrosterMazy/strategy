using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class AssetHandler
{
    [OnOpenAsset()]
    public static bool OpenEditor(int instanceId, int line)
    {
        Caster _caster = EditorUtility.InstanceIDToObject(instanceId) as Caster;
        if (_caster != null)
        {
            CasterEditorWindow.Open(_caster);
            return true;
        }

        return false;
    }
}

[CustomEditor(typeof(Caster))]
public class CasterEditor : Editor
{
    new private SerializedObject serializedObject;
    private Caster caster;
    private CasterEditorWindow window;
    private SerializedProperty currentProperty;
    private bool isSpellEditorOpen = false;
    private bool isWindowMode = false;

    private Dictionary<int, bool> destroySpell = new Dictionary<int, bool>();
    private int helperDestroySpellDictInit = 0;

    private string selectedPropertyPath;
    private SerializedProperty selectedProperty;

    public override void OnInspectorGUI()
    {
        caster = (Caster)target;
        serializedObject = new SerializedObject(caster);
        string Open_Close = "Open";

        EditorGUILayout.BeginHorizontal();
        isWindowMode = EditorGUILayout.Toggle(isWindowMode, GUILayout.MaxWidth(30));
        if (isWindowMode)
        {
            if (GUILayout.Button("Open Spells Editor Window"))
            {
                CasterEditorWindow.Open((Caster)target);
            }
            EditorGUILayout.EndHorizontal();
        }
        if (isSpellEditorOpen && !isWindowMode)
        {
            Open_Close = "Close";
            if (GUILayout.Button(Open_Close + " Spells Editor"))
            {
                isSpellEditorOpen = !isSpellEditorOpen;
                CloseAllFolders(serializedObject.FindProperty("SpellsList"));
            }
            EditorGUILayout.EndHorizontal();
            DrawWindow();
        }
        else if (!isSpellEditorOpen && !isWindowMode)
        {
            Open_Close = "Open";
            if (GUILayout.Button(Open_Close + " Spells Editor"))
            {
                isSpellEditorOpen = !isSpellEditorOpen;
            }
            EditorGUILayout.EndHorizontal();
//            DrawSpellsNames(serializedObject.FindProperty("SpellsList"));
        }
    }

    private void DrawProperties(SerializedProperty prop, bool drawChildren)
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

    private void DrawMainProperties_name(SerializedProperty prop)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Name", GUILayout.MaxWidth(50));
        prop.FindPropertyRelative("Name").stringValue = EditorGUILayout.TextField(prop.FindPropertyRelative("Name").stringValue);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("Description");
        prop.FindPropertyRelative("Description").stringValue = EditorGUILayout.TextArea(prop.FindPropertyRelative("Description").stringValue, GUILayout.MaxWidth(600));
    }

    private void DrawMainProperties(SerializedProperty prop, bool drawChildren)
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

    private void DrawSidebar(SerializedProperty prop)
    {
        GUIStyle SideBarHeader = new GUIStyle(EditorStyles.label);
        SideBarHeader.richText = true;
        SideBarHeader.fontSize = 18;
        GUIStyle SelectedSpell = new GUIStyle(EditorStyles.miniButton);
        SelectedSpell.normal.textColor = Color.green;
        SelectedSpell.active.textColor = Color.green;
        SelectedSpell.onActive.textColor = Color.green;
        SelectedSpell.onFocused.textColor = Color.green;
        SelectedSpell.onHover.textColor = Color.green;
        SelectedSpell.onNormal.textColor = Color.green;
        SelectedSpell.hover.textColor = Color.green;
        SelectedSpell.richText = true;
        int spellIndex = 0;
        if (helperDestroySpellDictInit == 0) // Инициализация словаря
        {
            for (int i = 0; i < 50; i++)
            {
                destroySpell[i] = false;
            }
        }
        helperDestroySpellDictInit += 1;

        EditorGUILayout.LabelField("<b>Spells:</b>", SideBarHeader, GUILayout.MaxWidth(150), GUILayout.MinHeight(19));
        foreach (SerializedProperty p in prop)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(150));

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
                    UpdateWindow();
                }
            }
            if (p.propertyPath == selectedPropertyPath)
            {
                if (GUILayout.Button(p.displayName, SelectedSpell))
                {
                    selectedPropertyPath = p.propertyPath;
                }
            }
            else
            {
                if (GUILayout.Button(p.displayName))
                {
                    selectedPropertyPath = p.propertyPath;
                }
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
            UpdateWindow();
        }
    }


    private void DrawSpellsNames(SerializedProperty prop)
    {
        EditorGUILayout.LabelField("Spells:");
        foreach (SerializedProperty p in prop)
        {
            EditorGUILayout.LabelField(p.displayName);
        }
    }

    private void CloseAllFolders(SerializedProperty prop)
    {
        foreach (SerializedProperty p in prop)
        {
            selectedPropertyPath = p.propertyPath;
            if (!string.IsNullOrEmpty(selectedPropertyPath))
            {
                selectedProperty = serializedObject.FindProperty(selectedPropertyPath);
                foreach (SerializedProperty pr in selectedProperty)
                {
                    pr.isExpanded = false;
                }
            }

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

    private void DrawField(string propName, bool relative)
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

    private void DrawHeaders(SerializedProperty prop)
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
                    EditorGUILayout.LabelField(p.name, GUILayout.MinWidth(0));
                    EditorGUILayout.PropertyField(p, new GUIContent(""), false);
                    EditorGUILayout.EndHorizontal();
                }
            }
            alreadyDrawPropertyPaths.Add(p.propertyPath);

        }
        selectedProperty = serializedObject.FindProperty(oldSelectedPropertyPath);
    }

    private void Apply()
    {
        serializedObject.ApplyModifiedProperties();
    }

    private void UpdateWindow()
    {
        helperDestroySpellDictInit = 0;
        for (int i = 0; i < 50; i++)
        {
            destroySpell[i] = false;
        }
    }

    private void DrawDefaultArrays(SerializedProperty p, List<string> alreadyDrawPropertyPaths)
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

    private void DrawWindow()
    {
        currentProperty = serializedObject.FindProperty("SpellsList");

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(false));
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

    private void DrawSelectedPropertiesPanel()
    {

        DrawMainProperties_name(selectedProperty);
        //EditorGUILayout.BeginHorizontal();
        DrawHeaders(selectedProperty);
        //EditorGUILayout.EndHorizontal();


        //DrawProperties(selectedProperty, true);

    }
}