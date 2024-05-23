using UnityEngine;
using System;

public class HexCell : MonoBehaviour 
{
    [NonSerialized] public float LightRate = 0; //коэффициент освещенности (КО) клетки от которого зависит заспавниться ли на этой клетке тьма в следующем ходу или нет
    private GameObject _darknessnPrefab;
    private GameObject _darknessInstance;
    [NonSerialized] public LightTransporter lightTransporter;
    [NonSerialized] public Vector2Int localPos;
    [NonSerialized] public int height;
    [NonSerialized] public float heightNormalized; // высота в диапазоне от 0 до 1
    [NonSerialized] public Biome biome;
    [NonSerialized] public bool isMountain = false;
    [NonSerialized] public bool isWater = false;
    [NonSerialized] public bool InDarkness = false;
    
    private void Start()
    {
        TurnManager.NightStarts += NightStarts;   
        TurnManager.DayStarts += DayStarts;       //    Экшены из скрипта TurnManager        
        _darknessnPrefab = Resources.Load<GameObject>("Prefabs/Darkness");
        lightTransporter.OnLightForceChange += OnLightForceChanged;
        _darknessInstance = Instantiate(_darknessnPrefab, transform.parent.position + Vector3.up*1.5f, transform.rotation, transform.parent);
        _darknessInstance.SetActive(false);

    }
    private void DayStarts() //начало дня, изменение коэфициента освещенности и его порогового значения
    {
        LightRate += DarknessMainVariables.LightForce; 
        DarknessMainVariables.CriticalLightRate = -1;
        DarknessUpdate();
    } 
    private void NightStarts() //тоже самое что в DayStarts, но только для ночи
    {
        LightRate -= DarknessMainVariables.LightForce; 
        DarknessMainVariables.CriticalLightRate = 1;
        DarknessUpdate();
    }
    private void DarknessUpdate() //проверка условий для создания или удаления тьмы
    {
        if (LightRate <= DarknessMainVariables.CriticalLightRate && _darknessInstance.activeSelf==false)
        {
            _darknessInstance.SetActive(true); InDarkness = true;
        }
        else if (_darknessInstance != null && LightRate > DarknessMainVariables.CriticalLightRate)
        {
            _darknessInstance.SetActive(false); InDarkness = false;
        }
    }  
    private void OnLightForceChanged(LightTransporter NewSource, int NewLightForce)
    {
        DarknessUpdate();
    }
}