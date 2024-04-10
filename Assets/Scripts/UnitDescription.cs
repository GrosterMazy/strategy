using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class UnitDescription
{
    public List<float> ArmorEfficiencyTable;
    public byte Armor;
    public short MovementSpeed;
    public short AttackRange;
    public float Health;
    public float AttackDamage;
    public float FoodConsumption;
    public float ArmorUnitEfficiencyMaxAmount;
    public float ArmorEfficiencyDecreasementPerUnit;
    public Vector2Int LocalCoords;
    [NonSerialized] public float DamageReductionPercent;

    public void ArmorCounter()
    {
        var _maxArmorAccordingToPrimaryRules = ArmorUnitEfficiencyMaxAmount / ArmorEfficiencyDecreasementPerUnit;
        if (_maxArmorAccordingToPrimaryRules != Mathf.RoundToInt(_maxArmorAccordingToPrimaryRules)) 
        { _maxArmorAccordingToPrimaryRules = Mathf.RoundToInt(_maxArmorAccordingToPrimaryRules) + 1; }
        for (int i = 0; i < _maxArmorAccordingToPrimaryRules + 1; i++) ArmorEfficiencyTable.Add(0);
        for (int i = 0; i < _maxArmorAccordingToPrimaryRules + 1; i++)
        {
            var _armorModification = ArmorUnitEfficiencyMaxAmount - ArmorEfficiencyDecreasementPerUnit * (i - 1);
            if (i == 0) ArmorEfficiencyTable[i] = 0;
            else if (i <= _maxArmorAccordingToPrimaryRules && _armorModification > 0) ArmorEfficiencyTable[i] = ArmorEfficiencyTable[i - 1] + _armorModification;
            else ArmorEfficiencyTable.RemoveAt(ArmorEfficiencyTable.Count - 1);
        }
        DamageReductionPercent = ArmorEfficiencyTable[Armor];
    } 

    public void ApplyDamage(float _damage)
    {
        Health -= _damage * (100f - DamageReductionPercent);
    }
}
   