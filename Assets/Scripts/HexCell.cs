using UnityEngine;

public class HexCell : MonoBehaviour 
{
    public float _lightRate = 0; //коэффицент освещенности клетки от которого зависит заспавниться ли на этой клетке тьма в следующем ходу или нет
    private GameObject _darknessnPrefab;

    private void Awake()
    {
        TurnManager.NightStarts += NightStarts;   // начало ночи //
        TurnManager.DayStarts += DayStarts;       // начало дня //  <--- Экшены
        TurnManager.onTurnChanged += TurnChange;  // смена хода //

        _darknessnPrefab = Resources.Load<GameObject>("Prefabs/Darkness"); //подгружаем префаб тьмы из папки Resources
    }
    private void DayStarts() { _lightRate += DarknessMainVariables.LightForce; DarknessMainVariables.CriticalLightRate = -1; } // "осветляем" клетки силой света
    private void NightStarts() { _lightRate -= DarknessMainVariables.DarknessForce; DarknessMainVariables.CriticalLightRate = 0; } // "затемняем" клетки на значение силы тьмы
    private void TurnChange() //проверка спавнить ли тьму.
    {
        if(_lightRate < DarknessMainVariables.CriticalLightRate) Instantiate(_darknessnPrefab, transform.position + Vector3.up, transform.rotation, transform); // создаем тьму
        else if(GetComponentInChildren<Darkness>()!=null) Destroy(GetComponentInChildren<Darkness>().gameObject); //удаляем тьму
    }
   
}
