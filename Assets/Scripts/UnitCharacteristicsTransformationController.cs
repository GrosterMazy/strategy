using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UnitCharacteristicsTransformationController : MonoBehaviour
{
    public UnitDescription unitDescription;

    [NonSerialized] public List<float> CurrentArmorEfficiencyTable; // Таблица, где номер элемента - количество брони, а значение элемента - соответствующая ей %-ая защита от урона
    [NonSerialized] public int CurrentArmor;
    [NonSerialized] public short CurrentMovementSpeed;
    [NonSerialized] public short CurrentAttackRange;
    [NonSerialized] public float CurrentHealth;
    [NonSerialized] public float CurrentAttackDamage;
    [NonSerialized] public float CurrentFoodConsumption;
    [NonSerialized] public float CurrentArmorUnitEfficiencyMaxAmount;  // Значение максимальной эффективности брони(т.е. на сколько % будет снижен урон за первую единицу брони)
    [NonSerialized] public float CurrentArmorEfficiencyDecreasementPerUnit; // То, насколько будет снижаться эффективность каждой последующей единицы брони(в %)
    [NonSerialized] public float CurrentDamageReductionPercent;

    private void Awake()
    {
        TurnManager.onTurnChanged += TransformationCharacteristicsOnTurnChanged;
        GetInformationAboutUnitCharacteristics();
    }
    private void GetInformationAboutUnitCharacteristics()
    {
        WorkerUnit _workerUnit = GetComponent<WorkerUnit>();
        if (_workerUnit != null)
            unitDescription = _workerUnit;
        RangedUnit _rangedUnit = GetComponent<RangedUnit>();
        if (_rangedUnit != null)
            unitDescription = _rangedUnit;
        MeleeUnit _meleeUnit = GetComponent<MeleeUnit>();
        if (_meleeUnit != null)
            unitDescription = _meleeUnit;
    }
    private void TransformationCharacteristicsOnTurnChanged()
    {
        
    }
}
