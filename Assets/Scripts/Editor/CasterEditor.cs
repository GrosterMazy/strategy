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
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Open Spels Editor"))
        {
            CasterEditorWindow.Open((Caster)target);
        }
    }
}