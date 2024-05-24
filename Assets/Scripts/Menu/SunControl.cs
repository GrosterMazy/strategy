using UnityEngine;

public class SunControl : MonoBehaviour
{
    [SerializeField] private float SunSpeed;
    private void OnEnable() { TurnManager.NightStarts += StartNight; TurnManager.DayStarts += StartDay; }
    private void OnDisable() { TurnManager.NightStarts -= StartNight; TurnManager.DayStarts -= StartDay; }
    private void StartNight()  { transform.rotation = Quaternion.Euler(0, 150, 0); }
    private void StartDay() { transform.rotation = Quaternion.Euler(95, 150, 0); }
    
        
    
}
