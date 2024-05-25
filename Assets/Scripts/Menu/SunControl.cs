using UnityEngine;

public class SunControl : MonoBehaviour
{
    [SerializeField] private float SunSpeed;

    private TurnManager _turnManager;
    private Light _light;

    private void Awake() 
    {
        _turnManager = FindObjectOfType<TurnManager>();
        _light = GetComponent<Light>();
    }
    private void OnEnable() {
        _turnManager.NightStarts += StartNight;
        _turnManager.DayStarts += StartDay;
    }
    private void OnDisable() { _turnManager.NightStarts -= StartNight; _turnManager.DayStarts -= StartDay; }
    private void StartNight()  { transform.rotation = Quaternion.Euler(-80, 150, 0); _light.intensity = 0.1f; }
    private void StartDay() { transform.rotation = Quaternion.Euler(80, 150, 0); _light.intensity = 1; }
    
        
    
}
