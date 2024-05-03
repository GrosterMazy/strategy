using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable] public class SpellsDescription
{
    public string Name;
    public string Description;

    
    public string Target_Header_;

    public bool OnEnemyUnits;
    public bool OnTeammateUnits;
    public bool OnEnemyBuildings;
    public bool OnTeammateBuildings;
    public bool OnGround;
    public string Target_EndHeader_;

    
    public string Shape_Header_;

    public bool IsCircle;
    public bool IsLine;
    public bool IsThreeNeighbourCells;
    public string Shape_EndHeader_;

    public string Conditions_Header_;

    public int MinRemainingActions;
    public int SpendActions;
    public bool IsPassive;
    public bool IsOnButton;
    public bool IsCalling;
    public string Conditions_EndHeader_;

    
    public string Ranges_Header_;

    public int CastRange;
    public int CircleRange;
    public int LineRange;
    public string Ranges_EndHeader_;

    public string Damage_Header_;

    public float DamageCount;
    public float PercentageDamageOfMaxTargetHealth;
    public float PercentageDamageOfCurrentTargetHealth;
    public float PercentageDamageOfMissingTargetHealth;

    public float PercentageDamageOfMaxCasterHealth;
    public float PercentageDamageOfCurrentCasterHealth;
    public float PercentageDamageOfMissingCasterHealth;
    [Tooltip("Сколько единиц брони игнорирует спелл")]
    public int ArmourPenetration;
    public string Damage_EndHeader_;

    public string Heal_Header_;

    public float HealCount;
    public float PercentageHealOfMaxTargetHealth;
    public float PercentageHealOfCurrentTargetHealth;
    public float PercentageHealOfMissingTargetHealth;

    public float PercentageHealOfMaxCasterHealth;
    public float PercentageHealOfCurrentCasterHealth;
    public float PercentageHealOfMissingCasterHealth;
    public string Heal_EndHeader_;

    public string EffectsListSize_Header_;

    [Tooltip("Состояния, присваиваемые цели")]
    public List<string> Effects_DrawAnyway_;
    public string EffectsList_EndHeader_;
}
