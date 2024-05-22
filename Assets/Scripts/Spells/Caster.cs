using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Reflection;

public class Caster : MonoBehaviour
{
    public bool updatingHelpProperty; // Вспомогательная переменная для инспектора
    public List<SpellsDescription> SpellsList = new List<SpellsDescription>();

    List<Vector2Int> targetShapeCells = new List<Vector2Int>(); // Координаты всех клеток поля, на которые попадает спел
    List<HexCell> targetCells = new List<HexCell>();
    List<UnitDescription> targetUnits = new List<UnitDescription>();
    List<FacilityDescription> targetFacilities = new List<FacilityDescription>();

    private List<(MeshRenderer, Color)> cellsToReturnColor = new List<(MeshRenderer, Color)>();

    private SpellsDescription _preparedSpell = null;
    private UnitDescription _spellCaster = null;
    private PlacementManager _placementManager;
    private HexGrid _hexGrid;
    private EnumEffectsTranslator _enumEffectsTranslator;

    private void Awake()
    {
        InitComponentLinks();
    }
    private void OnEnable()
    {
        MouseSelection.onSelectionChanged += CastPreparedSpell;
        MouseSelection.onSelectionClick += CastPreparedSpell;
        MouseSelection.onClickOutside += ResetPreparedSpell;
        MouseSelection.onHighlightChanged += OnHighlightedChanged;
        MouseSelection.onSelectionHighlighted += OnHighlightedChanged;
    }
    private void OnDisable()
    {
        MouseSelection.onSelectionChanged -= CastPreparedSpell;
        MouseSelection.onSelectionClick -= CastPreparedSpell;
        MouseSelection.onClickOutside -= ResetPreparedSpell;
        MouseSelection.onHighlightChanged += OnHighlightedChanged;
        MouseSelection.onSelectionHighlighted -= OnHighlightedChanged;
    }

    private void OnHighlightedChanged(Transform highlighted)
    {
        if (highlighted == null || _spellCaster == null || _preparedSpell == null) return;
        SpellPreview(_preparedSpell, _hexGrid.InLocalCoords(highlighted.position), _hexGrid.InLocalCoords(_spellCaster.transform.position));
    }
    private void SpellPreview(SpellsDescription spell, Vector2Int targetPosition, Vector2Int caster)
    {
        ChooseCellsInShape(spell, targetPosition, caster);
        PaintCells(targetShapeCells, Color.cyan);
    }


    private void PaintCells(List<Vector2Int> cells, Color newColor)
    {
        if (cellsToReturnColor.Count != 0)
        {
            foreach ((MeshRenderer, Color) cell in cellsToReturnColor)
            {
                cell.Item1.material.color = cell.Item2;
            }
        }

        cellsToReturnColor.Clear();

        foreach (Vector2Int cell in cells)
        {
            MeshRenderer cellMR = _hexGrid.hexCells[cell.x, cell.y].GetComponent<MeshRenderer>();
            cellsToReturnColor.Add((cellMR, cellMR.material.color));
            cellMR.material.color = newColor;
        }
    }

    public void PrepareSpell(SpellsDescription spell, UnitDescription caster) // Только для спелов, кастуемых через кнопку
    {
        _preparedSpell = spell;
        _spellCaster = caster;
    }

    private void CastPreparedSpell(Transform selected)
    {
        if (_preparedSpell != null && _spellCaster != null && selected != null)
        {
            CastSpell(_preparedSpell, _hexGrid.InLocalCoords(selected.position), _spellCaster);
            PaintCells(targetShapeCells, Color.red);
            _preparedSpell = null;
            _spellCaster = null;
        }
        if (_preparedSpell == null || _spellCaster == null)
        {
            ResetPreparedSpell();
        }
    }
    private void ResetPreparedSpell()
    {
        _preparedSpell = null;
        _spellCaster = null;
    }

    private void CastSpell(SpellsDescription spell, Vector2Int targetPosition, UnitDescription caster)
    {
        if (caster.GetComponent<UnitActions>().remainingActionsCount >= spell.MinRemainingActions)
        {
            ChooseTargets(spell, targetPosition, caster);
            if (targetShapeCells.Count == 0) return;
            DoDamage(spell, targetUnits, targetFacilities, caster);
            DoHeal(spell, targetUnits, targetFacilities, caster);
            ApplyEffect(spell, targetUnits, targetFacilities, targetCells);
            SpendActions(spell, caster);
        }
    }

    private void ChooseTargets(SpellsDescription spell, Vector2Int targetPosition, UnitDescription caster)
    {
        targetUnits.Clear();
        targetCells.Clear();
        targetFacilities.Clear();
        ChooseCellsInShape(spell, targetPosition, _hexGrid.InLocalCoords(caster.transform.position));
        if (spell.ExcludeCaster) targetShapeCells.Remove(_hexGrid.InLocalCoords(caster.transform.position));
        if (spell.OnCaster == true) { targetUnits.Add(caster); }
        foreach (Vector2Int potentialTarget in targetShapeCells)
        {
            ChooseTargetUnits(spell, potentialTarget, caster);

            ChooseTargetFacilities(spell, potentialTarget, caster);

            ChooseTargetCells(spell, potentialTarget);
        }
    }

    private void ChooseCellsInShape(SpellsDescription spell, Vector2Int targetPosition, Vector2Int caster)
    {
        targetShapeCells.Clear();
        if (spell.IsThreeLinesForward)
        {
            List<Vector2Int> threeNeighbourCells = FindThreeNeighbourCells(targetPosition, caster);
            if (threeNeighbourCells.Count != 0)
            {
                foreach (Vector2Int cell in threeNeighbourCells)
                {
                    targetShapeCells.AddRange(FindCellsInLine(cell, caster, spell.ThreeLineRange));
                }
            }
        }

        if (spell.IsLine)
        {
            targetShapeCells.AddRange(FindCellsInLine(targetPosition, caster, spell.LineRange));
        }

        if (spell.IsStar && spell.CastRange >= _hexGrid.Distance(caster, targetPosition))
        {
            List<Vector2Int> targetNeighbours = new List<Vector2Int>(_hexGrid.Neighbours(targetPosition));
            targetShapeCells.Add(targetPosition);
            foreach (Vector2Int neighbour in targetNeighbours)
            {
                targetShapeCells.AddRange(FindCellsInLine(neighbour, targetPosition, spell.StarRange));
            }
        }

        if (spell.IsCircle && spell.CastRange >= _hexGrid.Distance(caster, targetPosition))
        {
            targetShapeCells.AddRange(FindCellsInCircle(targetPosition, spell.CircleRange));
        }

        /*
        if (spell.IsLine) // Тут допущена ошибка, но из-за неё получался прикольный рисунок
        {
            List<Vector2Int> casterNeighbours = new List<Vector2Int>(_hexGrid.Neighbours(caster.transform.position));
            List<Vector2Int> targetNeighbours = new List<Vector2Int>(_hexGrid.Neighbours(targetPosition));
            if (casterNeighbours.Contains(targetPosition))
            {
                Vector2Int previousTarget = _hexGrid.InLocalCoords(caster.transform.position);
                targetShapeCells.Add(targetPosition);
                for (int i = 1; i < spell.LineRange; i++)
                {
                    List<Vector2Int> potentialTargets = new List<Vector2Int>();
                    foreach (Vector2Int targetNeighbour in _hexGrid.Neighbours(targetPosition))
                    {
                        if (!new List<Vector2Int>(_hexGrid.Neighbours(previousTarget)).Contains(targetNeighbour) && previousTarget != targetNeighbour)
                        {
                            potentialTargets.Add(targetNeighbour);
                        }
                    }
                    List<Vector2Int> finalPotentialTargets = new List<Vector2Int>();
                    foreach (Vector2Int potentialTarget in potentialTargets)
                    {
                        foreach (Vector2Int potentialTargetNeighbour in _hexGrid.Neighbours(potentialTarget))
                        {
                            if (finalPotentialTargets.Contains(potentialTargetNeighbour) && !targetShapeCells.Contains(potentialTargetNeighbour))
                            {
                                targetShapeCells.Add(potentialTargetNeighbour);
                                previousTarget = targetPosition;
                                targetPosition = potentialTargetNeighbour;
                                break;
                            }
                            finalPotentialTargets.Add(potentialTargetNeighbour);
                        }
                    }
                }
            }
        } 
        */
        targetShapeCells = targetShapeCells.Distinct().ToList();
    }

    private List<Vector2Int> FindThreeNeighbourCells(Vector2Int targetPosition, Vector2Int caster)
    {
        List<Vector2Int> threeNeighbourCells = new List<Vector2Int>();
        List<Vector2Int> casterNeighbours = new List<Vector2Int>(_hexGrid.Neighbours(caster));
        List<Vector2Int> targetNeighbours = new List<Vector2Int>(_hexGrid.Neighbours(targetPosition));
        if (casterNeighbours.Contains(targetPosition))
        {
            threeNeighbourCells.Add(targetPosition);
            foreach (Vector2Int potentialTarget in casterNeighbours)
            {
                if (targetNeighbours.Contains(potentialTarget))
                {
                    threeNeighbourCells.Add(potentialTarget);
                }
            }
        }
        return threeNeighbourCells;
    }
    private List<Vector2Int> FindCellsInLine(Vector2Int targetPosition, Vector2Int caster, int lineRange)
    {
        List<Vector2Int> cellsInLine = new List<Vector2Int>();
        List<Vector2Int> casterNeighbours = new List<Vector2Int>(_hexGrid.Neighbours(caster));
        List<Vector2Int> targetNeighbours = new List<Vector2Int>(_hexGrid.Neighbours(targetPosition));
        if (casterNeighbours.Contains(targetPosition))
        {
            Vector2Int previousTarget = caster;
            cellsInLine.Add(targetPosition);
            for (int i = 1; i < lineRange; i++)
            {
                List<Vector2Int> potentialTargets = new List<Vector2Int>();
                foreach (Vector2Int targetNeighbour in _hexGrid.Neighbours(targetPosition))
                {
                    if (!new List<Vector2Int>(_hexGrid.Neighbours(previousTarget)).Contains(targetNeighbour) && previousTarget != targetNeighbour)
                    {
                        potentialTargets.Add(targetNeighbour);
                    }
                }
                List<Vector2Int> finalPotentialTargets = new List<Vector2Int>();
                foreach (Vector2Int potentialTarget in potentialTargets)
                {
                    foreach (Vector2Int potentialTargetNeighbour in _hexGrid.Neighbours(potentialTarget))
                    {
                        if (finalPotentialTargets.Contains(potentialTargetNeighbour) && !cellsInLine.Contains(potentialTargetNeighbour) && potentialTargets.Contains(potentialTargetNeighbour))
                        {
                            cellsInLine.Add(potentialTargetNeighbour);
                            previousTarget = targetPosition;
                            targetPosition = potentialTargetNeighbour;
                            break;
                        }
                        finalPotentialTargets.Add(potentialTargetNeighbour);
                    }
                }
            }
        }
        return cellsInLine;
    }
    private List<Vector2Int> FindCellsInCircle(Vector2Int targetPosition, float circleRange)
    {
        List<Vector2Int> cellsInCircle = new List<Vector2Int>();
        cellsInCircle.Add(targetPosition);
        for (int i = 0; i < circleRange; i++)
        {
            List<Vector2Int> targetCellsCopy = new List<Vector2Int>();
            foreach (Vector2Int choosedCell in cellsInCircle) // Создаём копию списка и заполняем её, иначе будет ругаться, что мы меняем список во время перебора его элементов
            {
                targetCellsCopy.Add(choosedCell);
            }

            foreach (Vector2Int choosedCell in targetCellsCopy)
            {
                foreach (Vector2Int potentialTarget in _hexGrid.Neighbours(choosedCell))
                {
                    if (!cellsInCircle.Contains(potentialTarget))
                    {
                        cellsInCircle.Add(potentialTarget);
                    }
                }
            }
        }
        return cellsInCircle;
    }

    private void ChooseTargetUnits(SpellsDescription spell, Vector2Int potentialTarget, UnitDescription caster)
    {
        if (_placementManager.gridWithObjectsInformation[potentialTarget.x, potentialTarget.y] != null
            && _placementManager.gridWithObjectsInformation[potentialTarget.x, potentialTarget.y].TryGetComponent<UnitDescription>(out UnitDescription unitTarget))
        {
            if (spell.OnEnemyUnits && unitTarget.TeamAffiliation != caster.TeamAffiliation)
            {
                targetUnits.Add(unitTarget);
            }
            if (spell.OnTeammateUnits && unitTarget.TeamAffiliation == caster.TeamAffiliation)
            {
                targetUnits.Add(unitTarget);
            }
        }
    }
    private void ChooseTargetFacilities(SpellsDescription spell, Vector2Int potentialTarget, UnitDescription caster)
    {
        if (_placementManager.gridWithObjectsInformation[potentialTarget.x, potentialTarget.y] != null
            && _placementManager.gridWithObjectsInformation[potentialTarget.x, potentialTarget.y].TryGetComponent<FacilityDescription>(out FacilityDescription facilityTarget))
        {
            if (spell.OnEnemyBuildings && facilityTarget.TeamAffiliation != caster.TeamAffiliation)
            {
                targetFacilities.Add(facilityTarget);
            }
            if (spell.OnTeammateBuildings && facilityTarget.TeamAffiliation == caster.TeamAffiliation)
            {
                targetFacilities.Add(facilityTarget);
            }
        }
    }
    private void ChooseTargetCells(SpellsDescription spell, Vector2Int potentialTarget)
    {
        if (spell.OnGround && _hexGrid.hexCells[potentialTarget.x, potentialTarget.y] != null)
        {
            targetCells.Add(_hexGrid.hexCells[potentialTarget.x, potentialTarget.y]);
        }
    }

    private void DoDamage(SpellsDescription spell, List<UnitDescription> units, List<FacilityDescription> facilities, UnitDescription caster)
    {
        foreach (UnitDescription unit in units)
        {
            Health unitHealth = unit.GetComponent<Health>();
            if (spell.ArmourPenetration != 0 && unit.ArmorEfficiencyTable.Count > 0)
            {
                unitHealth.damageReductionPercent = unit.ArmorEfficiencyTable[Mathf.Clamp(unit.Armor - spell.ArmourPenetration, 0, unit.Armor)];
            }
            if (spell.DamageCount != 0) unitHealth.ApplyDamage(spell.DamageCount);
            if (spell.PercentageDamageOfMaxTargetHealth != 0) unitHealth.ApplyPercentageDamageOfMaxHealth(spell.PercentageDamageOfMaxTargetHealth);
            if (spell.PercentageDamageOfCurrentTargetHealth != 0) unitHealth.ApplyPercentageDamageOfCurrentHealth(spell.PercentageDamageOfCurrentTargetHealth);
            if (spell.PercentageDamageOfMissingTargetHealth != 0) unitHealth.ApplyPercentageDamageOfMissingHealth(spell.PercentageDamageOfMissingTargetHealth);
            if (spell.PercentageDamageOfMaxCasterHealth != 0) unitHealth.ApplyDamageIgnoringArmour(caster.Health * spell.PercentageDamageOfMaxCasterHealth / 100);
            if (spell.PercentageDamageOfCurrentCasterHealth != 0) unitHealth.ApplyDamageIgnoringArmour(caster.GetComponent<Health>().currentHealth * spell.PercentageDamageOfCurrentCasterHealth / 100);
            if (spell.PercentageDamageOfMissingCasterHealth != 0) unitHealth.ApplyDamageIgnoringArmour((caster.Health - caster.GetComponent<Health>().currentHealth) * spell.PercentageDamageOfMissingCasterHealth / 100);
            if (unit.ArmorEfficiencyTable.Count > 0) unitHealth.damageReductionPercent = unit.ArmorEfficiencyTable[Mathf.Clamp(unit.Armor, 0, unit.ArmorEfficiencyTable.Count - 1)];
        }

        foreach (FacilityDescription facility in facilities)
        {
            FacilityHealth facilityHealth = facility.GetComponent<FacilityHealth>();
            if (spell.ArmourPenetration != 0 && facility.ArmorEfficiencyTable.Count > 0)
            {
                facilityHealth.damageReductionPercent = facility.ArmorEfficiencyTable[Mathf.Clamp(facility.Armor - spell.ArmourPenetration, 0, facility.Armor)];
            }
            if (spell.DamageCount != 0) facilityHealth.ApplyDamage(spell.DamageCount);
            if (spell.PercentageDamageOfMaxTargetHealth != 0) facilityHealth.ApplyPercentageDamageOfMaxHealth(spell.PercentageDamageOfMaxTargetHealth);
            if (spell.PercentageDamageOfCurrentTargetHealth != 0) facilityHealth.ApplyPercentageDamageOfCurrentHealth(spell.PercentageDamageOfCurrentTargetHealth);
            if (spell.PercentageDamageOfMissingTargetHealth != 0) facilityHealth.ApplyPercentageDamageOfMissingHealth(spell.PercentageDamageOfMissingTargetHealth);
            if (spell.PercentageDamageOfMaxCasterHealth != 0) facilityHealth.ApplyDamageIgnoringArmour(caster.Health * spell.PercentageDamageOfMaxCasterHealth / 100);
            if (spell.PercentageDamageOfCurrentCasterHealth != 0) facilityHealth.ApplyDamageIgnoringArmour(caster.GetComponent<UnitHealth>().currentHealth * spell.PercentageDamageOfCurrentCasterHealth / 100);
            if (spell.PercentageDamageOfMissingCasterHealth != 0) facilityHealth.ApplyDamageIgnoringArmour((caster.Health - caster.GetComponent<UnitHealth>().currentHealth) * spell.PercentageDamageOfMissingCasterHealth / 100);
            if (facility.ArmorEfficiencyTable.Count > 0) facilityHealth.damageReductionPercent = facility.ArmorEfficiencyTable[Mathf.Clamp(facility.Armor, 0, facility.ArmorEfficiencyTable.Count - 1)];
        }
    }

    private void DoHeal(SpellsDescription spell, List<UnitDescription> units, List<FacilityDescription> facilities, UnitDescription caster)
    {
        foreach (UnitDescription unit in units)
        {
            Health unitHealth = unit.GetComponent<Health>();
            if (spell.HealCount != 0) unitHealth.ApplyHeal(spell.HealCount);
            if (spell.PercentageHealOfMaxTargetHealth != 0) unitHealth.ApplyPercentageHealOfMaxHealth(spell.PercentageHealOfMaxTargetHealth);
            if (spell.PercentageHealOfCurrentTargetHealth != 0) unitHealth.ApplyPercentageHealOfCurrentHealth(spell.PercentageHealOfCurrentTargetHealth);
            if (spell.PercentageHealOfMissingTargetHealth != 0) unitHealth.ApplyPercentageHealOfMissingHealth(spell.PercentageHealOfMissingTargetHealth);
            if (spell.PercentageHealOfMaxCasterHealth != 0) unitHealth.ApplyHeal(caster.Health * spell.PercentageHealOfMaxCasterHealth / 100);
            if (spell.PercentageHealOfCurrentCasterHealth != 0) unitHealth.ApplyHeal(caster.GetComponent<Health>().currentHealth * spell.PercentageHealOfCurrentCasterHealth / 100);
            if (spell.PercentageHealOfMissingCasterHealth != 0) unitHealth.ApplyHeal((caster.Health - caster.GetComponent<Health>().currentHealth) * spell.PercentageHealOfMissingCasterHealth / 100);
        }
        foreach (FacilityDescription facility in facilities)
        {
            FacilityHealth facilityHealth = facility.GetComponent<FacilityHealth>();
            if (spell.HealCount != 0) facilityHealth.ApplyHeal(spell.HealCount);
            if (spell.PercentageHealOfMaxTargetHealth != 0) facilityHealth.ApplyPercentageHealOfMaxHealth(spell.PercentageHealOfMaxTargetHealth);
            if (spell.PercentageHealOfCurrentTargetHealth != 0) facilityHealth.ApplyPercentageHealOfCurrentHealth(spell.PercentageHealOfCurrentTargetHealth);
            if (spell.PercentageHealOfMissingTargetHealth != 0) facilityHealth.ApplyPercentageHealOfMissingHealth(spell.PercentageHealOfMissingTargetHealth);
            if (spell.PercentageHealOfMaxCasterHealth != 0) facilityHealth.ApplyHeal(caster.Health * spell.PercentageHealOfMaxCasterHealth / 100);
            if (spell.PercentageHealOfCurrentCasterHealth != 0) facilityHealth.ApplyHeal(caster.GetComponent<Health>().currentHealth * spell.PercentageHealOfCurrentCasterHealth / 100);
            if (spell.PercentageHealOfMissingCasterHealth != 0) facilityHealth.ApplyHeal((caster.Health - caster.GetComponent<Health>().currentHealth) * spell.PercentageHealOfMissingCasterHealth / 100);
        }
    }

    private void SpendActions(SpellsDescription spell, UnitDescription caster)
    {
        if (spell.SpendActions != 0)
        {
            caster.GetComponent<UnitActions>().SpendAction(spell.SpendActions);
        }
    }

    private void ApplyEffect(SpellsDescription spell, List<UnitDescription> targetUnits, List<FacilityDescription> targetFacilities, List<HexCell> targetCells)
    {
        if (spell.Effects_DrawAnyway_.Count == 0) return;
        foreach (EnumEffectsTranslator.EffetsEnum effect in spell.Effects_DrawAnyway_)
        {
            Type effectType = _enumEffectsTranslator.Translate((int)effect);
            foreach (UnitDescription unit in targetUnits)
            {
                unit.gameObject.AddComponent(effectType);
            }
            foreach (FacilityDescription facility in targetFacilities)
            {
                facility.gameObject.AddComponent(effectType);
            }
            foreach (HexCell cell in targetCells)
            {
                cell.gameObject.AddComponent(effectType);
            }
        }
    }



    private void InitComponentLinks()
    {
        _placementManager = FindObjectOfType<PlacementManager>();
        _hexGrid = FindObjectOfType<HexGrid>();
        _enumEffectsTranslator = FindObjectOfType<EnumEffectsTranslator>();
    }

}