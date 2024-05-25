using UnityEngine;

public class SunControl : MonoBehaviour
{
    [SerializeField] private float SunSpeed;

    private TurnManager _turnManager;

    private void Awake() {
        _turnManager = FindObjectOfType<TurnManager>();
    }
    private void OnEnable() {
        _turnManager.NightStarts += StartNight;
        _turnManager.DayStarts += StartDay;
    }
    private void OnDisable() { _turnManager.NightStarts -= StartNight; _turnManager.DayStarts -= StartDay; }
    private void StartNight()  { transform.rotation = Quaternion.Euler(0, 150, 0); }
    private void StartDay() { transform.rotation = Quaternion.Euler(95, 150, 0); }
    
        
    
}
