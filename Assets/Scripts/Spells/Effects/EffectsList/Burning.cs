public class Burning : EffectsDescription
{
    private Health _health;
    protected float _damage;
    private void OnEnable()
    {
        TurnManager.onTurnChanged += BurnDamage;
    }
    private void OnDisable()
    {
        TurnManager.onTurnChanged -= BurnDamage;
    }
    protected void Start()
    {
        if (TryGetComponent<Wet>(out Wet wetEffect))
        {
            Destroy(wetEffect);
            Destroy(this);
        }
        else if (TryGetComponent<Health>(out Health health))
        {
            _health = health;
        }
        BurnDamage();
    }

    private void BurnDamage()
    {
        if (_health != null)
        {
            _health.ApplyDamageIgnoringArmour(_damage);
        }
    }
}
