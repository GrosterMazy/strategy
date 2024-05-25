using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsDescription : MonoBehaviour // Родитель всех эффектов
{
    public int remainingLifeTime; /* Сколько ещё ходов после наложения будет действовать
                                   (в наследнике на старте нужно задать значение(минимум 1)).
                                   Если указать 100000, то эффект будет бессрочным
                                  */
    public bool isNegative = true; // В наследнике можно изменить значение

    protected TurnManager _turnManager;

    protected void Awake() {
        _turnManager = FindObjectOfType<TurnManager>();
    }


    public void ReduceRemainingLifeTimeOnTurnChanged() // Вызывается после экшена onTurnChanged
    {
        if (remainingLifeTime == 100000) return;
        remainingLifeTime -= 1;
        if (remainingLifeTime <= 0)
        {
            Destroy(this);
        }
    }
}
