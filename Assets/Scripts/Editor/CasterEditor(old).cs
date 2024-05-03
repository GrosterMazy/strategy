/*

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(Caster))]
public class CasterEditor(old) : Editor
{
    private Caster _caster;
    private int _spellsCount;
    List<bool> _spellsOpen = new List<bool>(); // Список, содержащий в себе информацию о том, какие спелы сейчас открыты в инспекторе
    private void OnEnable()
    {
        _caster = (Caster)target;
    }

    public override void OnInspectorGUI()
    {
        //        base.OnInspectorGUI();
        SpellsCountChanger();
        OpenSpellsManager();

        for (int i = 0; i < _caster.SpellsList.Count; i++)
        {
            bool isOpen = _spellsOpen[i];
            
            _spellsOpen[i] = EditorGUILayout.DropdownButton(GUIContent.none, FocusType.Passive);
        }
/*
        for (int i = 0; i < _spellsCount; i++)
        {
            bool isOpen = _spellsOpen[i];
            _spellsOpen[i] = EditorGUILayout.BeginFoldoutHeaderGroup(isOpen, "Spell " + i);

            if (_spellsOpen[i])
            {
                EditorGUI.indentLevel++;
                //                EditorGUILayout.LabelField("Targets");
                EditorGUILayout.BeginFoldoutHeaderGroup(isOpen, "Targets");
                EditorGUI.indentLevel++;
                _caster.SpellsList[i].OnEnemy = EditorGUILayout.Toggle("On Enemy", _caster.SpellsList[i].OnEnemy);

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

    private void SpellsCountChanger()
    {
        _spellsCount = Mathf.Max(0, EditorGUILayout.IntField("Size", _caster.SpellsList.Count));

        while (_spellsCount > _caster.SpellsList.Count)
        {
            _caster.SpellsList.Add(null);

        }

        while (_spellsCount < _caster.SpellsList.Count)
        {
            _caster.SpellsList.RemoveAt(_caster.SpellsList.Count - 1);
            _spellsOpen.RemoveAt(_spellsOpen.Count - 1);
        }
    }

    private void OpenSpellsManager()
    {
        while (_spellsCount > _spellsOpen.Count)
        {
            _spellsOpen.Add(false);
        }

        while (_spellsCount < _spellsOpen.Count)
        {
            _spellsOpen.RemoveAt(_spellsOpen.Count - 1);
        }
    }
}

*/