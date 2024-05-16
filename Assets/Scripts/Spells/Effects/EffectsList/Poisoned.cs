﻿public class Poisoned : EffectsDescription
{
    private Health _health;
    private float _damage = 15;
    private void OnEnable()
    {
        TurnManager.onTurnChanged += PoisonDamage;
    }
    private void OnDisable()
    {
        TurnManager.onTurnChanged -= PoisonDamage;
    }
    private void Start()
    {
        remainingLifeTime = 1;
        if (TryGetComponent<Health>(out Health health))
        {
            _health = health;
        }
        PoisonDamage();
    }

    private void PoisonDamage()
    {
        if (_health != null)
        {
            _health.ApplyDamageIgnoringArmour(_damage);
        }
    }
}