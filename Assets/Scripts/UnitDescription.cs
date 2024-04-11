using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[Serializable] public class UnitDescription : ObjectOnGrid
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
    [NonSerialized] public float DamageReductionPercent;

    public void ArmorCounter()
    {
        if (ArmorEfficiencyDecreasementPerUnit == 0) { throw new Exception("Закон убывающей полезности предполагает убывающую полезность)"); }
        var _maxArmorAccordingToPrimaryRules = ArmorUnitEfficiencyMaxAmount / ArmorEfficiencyDecreasementPerUnit;
        if (_maxArmorAccordingToPrimaryRules != Mathf.RoundToInt(_maxArmorAccordingToPrimaryRules)) { _maxArmorAccordingToPrimaryRules = Mathf.RoundToInt(_maxArmorAccordingToPrimaryRules) + 1; }
        for (int i = 0; i < _maxArmorAccordingToPrimaryRules + 1; i++) ArmorEfficiencyTable.Add(0);
        for (int i = 0; i < _maxArmorAccordingToPrimaryRules + 1; i++) {
            var _armorModification = ArmorUnitEfficiencyMaxAmount - ArmorEfficiencyDecreasementPerUnit * (i - 1);
            if (i == 0) ArmorEfficiencyTable[i] = 0;
            else if (i <= _maxArmorAccordingToPrimaryRules && _armorModification > 0) ArmorEfficiencyTable[i] = Mathf.Clamp(ArmorEfficiencyTable[i - 1] + _armorModification, 0, 100);
            else ArmorEfficiencyTable.RemoveAt(ArmorEfficiencyTable.Count - 1); }
        if (ArmorEfficiencyTable[ArmorEfficiencyTable.Count - 1] == 100) { ArmorEfficiencyTable = (from _percent in ArmorEfficiencyTable where _percent != 100 select _percent).ToList();
            ArmorEfficiencyTable.Add(100); }
        DamageReductionPercent = ArmorEfficiencyTable[Armor];
    } 

    public void ApplyDamage(float _damage) { 
        Health -= _damage * (100f - DamageReductionPercent); }

    private void Start() {
        ArmorCounter(); }

}
   