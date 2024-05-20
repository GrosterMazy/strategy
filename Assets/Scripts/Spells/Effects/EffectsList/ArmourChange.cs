using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmourChange : EffectsDescription
{
    protected int armourChange;
    protected UnitDescription unit;
    protected FacilityDescription facility;
    protected Health health;
    protected void Start()
    {
        if (TryGetComponent<UnitDescription>(out UnitDescription _unit))
        {
            unit = _unit;
            unit.Armor += armourChange;
            unit.DamageReductionPercent = unit.ArmorEfficiencyTable[Mathf.Clamp(unit.Armor, 0, unit.ArmorEfficiencyTable.Count - 1)];
            health = unit.GetComponent<Health>();
            health.damageReductionPercent = unit.ArmorEfficiencyTable[Mathf.Clamp(unit.Armor, 0, unit.ArmorEfficiencyTable.Count - 1)];
        }
        if (TryGetComponent<FacilityDescription>(out FacilityDescription _facility))
        {
            facility = _facility;
            facility.Armor += armourChange;
            facility.DamageReductionPercent = facility.ArmorEfficiencyTable[Mathf.Clamp(facility.Armor, 0, facility.ArmorEfficiencyTable.Count - 1)];
            health = facility.GetComponent<Health>();
            health.damageReductionPercent = facility.ArmorEfficiencyTable[Mathf.Clamp(facility.Armor, 0, facility.ArmorEfficiencyTable.Count - 1)];
        }
    }
    protected void OnDestroy()
    {
        if (unit != null)
        {
            unit.Armor -= armourChange;
            unit.DamageReductionPercent = unit.ArmorEfficiencyTable[Mathf.Clamp(unit.Armor, 0, unit.ArmorEfficiencyTable.Count - 1)];
            health.damageReductionPercent = unit.ArmorEfficiencyTable[Mathf.Clamp(unit.Armor, 0, unit.ArmorEfficiencyTable.Count - 1)];
        }
        if (facility != null)
        {
            facility.Armor -= armourChange;
            facility.DamageReductionPercent = facility.ArmorEfficiencyTable[Mathf.Clamp(facility.Armor, 0, facility.ArmorEfficiencyTable.Count - 1)];
            health.damageReductionPercent = facility.ArmorEfficiencyTable[Mathf.Clamp(facility.Armor, 0, facility.ArmorEfficiencyTable.Count - 1)];
        }
    }
}
