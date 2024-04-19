using UnityEngine;

public class HexCell : MonoBehaviour 
{
    public float _lightRate = 0; //коэффициент освещенности (КО) клетки от которого зависит заспавниться ли на этой клетке тьма в следующем ходу или нет
    private GameObject _darknessnPrefab; 
    private GameObject _darknessInstance;
    public int height;
    private void Awake()
    {
        TurnManager.NightStarts += NightStarts;   
        TurnManager.DayStarts += DayStarts;       //    Экшены из скрипта TurnManager        
        TurnManager.onTurnChanged += TurnChange;
        _darknessnPrefab = Resources.Load<GameObject>("Prefabs/Darkness");
    }
    private void DayStarts() //начало дня, изменение коэфициента освещенности и его порогового значения
    {
        _lightRate += DarknessMainVariables.LightForce; 
        DarknessMainVariables.CriticalLightRate = -1; 
    } 
    private void NightStarts() //тоже самое что в DayStarts, но только для ночи
    {
        _lightRate -= DarknessMainVariables.DarknessForce; 
        DarknessMainVariables.CriticalLightRate = 0; 
    }
    private void TurnChange() //проверка условий для создания или удаления тьмы
    {
        if (_lightRate < DarknessMainVariables.CriticalLightRate && _darknessInstance == null)
        {
            _darknessInstance = Instantiate(_darknessnPrefab, transform.parent.position + Vector3.up*2, transform.rotation, transform.parent);
        }
        else if (_darknessInstance != null && _lightRate > DarknessMainVariables.CriticalLightRate)
        {
            Destroy(_darknessInstance);
        }
    }  
}