﻿using UnityEngine;

public class HexCell : MonoBehaviour 
{
    public int height;
    public float _lightRate = 0; //коэффициент освещенности клетки от которого зависит заспавниться ли на этой клетке тьма в следующем ходу или нет
    private GameObject _darknessnPrefab; // объект для хранения префаба тьмы


    private void Awake()
    {
        TurnManager.NightStarts += NightStarts;   // начало ночи //     Экшены из скрипта
        TurnManager.DayStarts += DayStarts;       // начало дня //         TurnManager
        TurnManager.onTurnChanged += TurnChange;
        _darknessnPrefab = Resources.Load<GameObject>("Prefabs/Darkness"); //подгружаем префаб тьмы из папки Resources
    }
    private void DayStarts() 
    {
        _lightRate += DarknessMainVariables.LightForce; // "осветляем" клетки на числовое значение силы света
        DarknessMainVariables.CriticalLightRate = -1; // устанавливается дневной порог для появления тьмы
    } 
    private void NightStarts() 
    {
        _lightRate -= DarknessMainVariables.DarknessForce; // "затемняем" клетки на значение силы тьмы
        DarknessMainVariables.CriticalLightRate = 0; // здесь соответственно устанавливается ночной порог для появления тьмы
    } 
    private void TurnChange() //проверка спавнить ли тьму.
    {
        if (_lightRate < DarknessMainVariables.CriticalLightRate && GetComponentInChildren<Darkness>() == null) Instantiate(_darknessnPrefab, transform.parent.position, transform.rotation, transform.parent); // создаем тьму с кордами клетки на которой находится скрипт                                                                                                                                                 
        else if(GetComponentInChildren<Darkness>()!=null && _lightRate > DarknessMainVariables.CriticalLightRate) Destroy(GetComponentInChildren<Darkness>().gameObject); // удаляем дочернюю тьму //          // и заодно заносим ее в дочерние объекты родительской клетки //                  
    }


}
