using UnityEngine;

public class DarknessMainVariables : MonoBehaviour
{
    public static float DarknessForce = 1; // сила с которой ночью уменьшается коэффицент освещенности клетки
    public static float LightForce = 1; // сила с которой днем увеличивается коэффицент освещенности клетки
    public static float CriticalLightRate = -1; //пороговое значение для создания тьмы
}
