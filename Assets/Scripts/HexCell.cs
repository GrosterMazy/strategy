using UnityEngine;

public class HexCell : MonoBehaviour 
{
    public float _lightRate = 0; //коэффициент освещенности клетки от которого зависит заспавниться ли на этой клетке тьма в следующем ходу или нет
    private GameObject _darknessnPrefab; // объект для хранения префаба тьмы

    private void Start()
    {
        TurnManager.NightStarts += NightStarts;   // начало ночи //     Экшены из скрипта
        TurnManager.DayStarts += DayStarts;       // начало дня //         TurnManager

        _darknessnPrefab = Resources.Load<GameObject>("Prefabs/Darkness"); //подгружаем префаб тьмы из папки Resources
    }
    private void DayStarts() 
    {
        _lightRate += DarknessMainVariables.LightForce; // "осветляем" клетки на числовое значение силы света
        DarknessMainVariables.CriticalLightRate = -1; // устанавливается дневной порог для появления тьмы
        TurnChange();
    } 
    private void NightStarts() 
    {
        _lightRate -= DarknessMainVariables.DarknessForce; // "затемняем" клетки на значение силы тьмы
        DarknessMainVariables.CriticalLightRate = 0; // здесь соответственно устанавливается ночной порог для появления тьмы
        TurnChange();
    } 
    private void TurnChange() //проверка спавнить ли тьму.
    {
        if (_lightRate < DarknessMainVariables.CriticalLightRate) Instantiate(_darknessnPrefab, transform.position + Vector3.up, transform.rotation, transform); // создаем тьму с кордами клетки на которой находится скрипт                                                                                                                                                 
        else if(GetComponentInChildren<Darkness>()!=null) Destroy(GetComponentInChildren<Darkness>().gameObject); // удаляем дочернюю тьму //                       и заодно заносим ее в дочерние объекты родительской клетки //
    }
   
}
