/*

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(Caster))]
public class CasterEditor(old) : Editor
{
    private Caster _caster;
    private int _spelsCount;
    List<bool> _spelsOpen = new List<bool>(); // Список, содержащий в себе информацию о том, какие спелы сейчас открыты в инспекторе
    private void OnEnable()
    {
        _caster = (Caster)target;
    }

    public override void OnInspectorGUI()
    {
        //        base.OnInspectorGUI();
        SpelsCountChanger();
        OpenSpelsManager();

        for (int i = 0; i < _caster.SpelsList.Count; i++)
        {
            bool isOpen = _spelsOpen[i];
            
            _spelsOpen[i] = EditorGUILayout.DropdownButton(GUIContent.none, FocusType.Passive);
        }
/*
        for (int i = 0; i < _spelsCount; i++)
        {
            bool isOpen = _spelsOpen[i];
            _spelsOpen[i] = EditorGUILayout.BeginFoldoutHeaderGroup(isOpen, "Spel " + i);

            if (_spelsOpen[i])
            {
                EditorGUI.indentLevel++;
                //                EditorGUILayout.LabelField("Targets");
                EditorGUILayout.BeginFoldoutHeaderGroup(isOpen, "Targets");
                EditorGUI.indentLevel++;
                _caster.SpelsList[i].OnEnemy = EditorGUILayout.Toggle("On Enemy", _caster.SpelsList[i].OnEnemy);

                EditorGUI.indentLevel--;


                EditorGUI.indentLevel--;
                EditorGUILayout.EndFoldoutHeaderGroup();
                
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
////
        if (GUI.changed) SetObjectDirty(_caster.gameObject);
    }

    private void SetObjectDirty(GameObject obj)
    {
        EditorUtility.SetDirty(obj);
        EditorSceneManager.MarkSceneDirty(obj.scene);
    }

    private void SpelsCountChanger()
    {
        _spelsCount = Mathf.Max(0, EditorGUILayout.IntField("Size", _caster.SpelsList.Count));

        while (_spelsCount > _caster.SpelsList.Count)
        {
            _caster.SpelsList.Add(null);

        }

        while (_spelsCount < _caster.SpelsList.Count)
        {
            _caster.SpelsList.RemoveAt(_caster.SpelsList.Count - 1);
            _spelsOpen.RemoveAt(_spelsOpen.Count - 1);
        }
    }

    private void OpenSpelsManager()
    {
        while (_spelsCount > _spelsOpen.Count)
        {
            _spelsOpen.Add(false);
        }

        while (_spelsCount < _spelsOpen.Count)
        {
            _spelsOpen.RemoveAt(_spelsOpen.Count - 1);
        }
    }
}

*/