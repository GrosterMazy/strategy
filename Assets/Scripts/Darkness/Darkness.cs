using UnityEngine;

public class Darkness : ObjectOnGrid
{
    public float Damage;
    private bool _isSomethingInDarkness=false;
    private GameObject _objectInDarkness;
    private TurnManager _turnManager;

    private void Awake()
    {
        _turnManager = FindObjectOfType<TurnManager>();
    
    }
    private void OnEnable()
    {
        _turnManager.onTurnChanged += OnTrurnChanged;
    }
    private void OnDisable()
    {
        _turnManager.onTurnChanged -= OnTrurnChanged;
    }
    private void OnTriggerStay(Collider other) // Проверка есть ли что-либо в этой тьме
    {
        _objectInDarkness = other.gameObject;
        _isSomethingInDarkness = true;
    }

    private void OnTrurnChanged()
    {
        if (_isSomethingInDarkness == true && _objectInDarkness!=null)
        {
            if (_objectInDarkness.TryGetComponent(out FacilityHealth facilityHealth) != false) // Пробуем найти у объекта во тьме компонент FacilityHealth
            {
                facilityHealth.ApplyDamage(Damage);
            }
            else if (_objectInDarkness.TryGetComponent(out UnitHealth unitHealth)) // Иначе пробуем найти у него компонент UnitHealth
            {
                unitHealth.ApplyDamage(Damage);
            }
            _isSomethingInDarkness = false;
        }
    }
}
