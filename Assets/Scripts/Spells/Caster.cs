using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Caster : MonoBehaviour
{
    public bool updatingHelpProperty; // Вспомогательная переменная для инспектора
    public List<SpellsDescription> SpellsList = new List<SpellsDescription>();

    List<Vector2Int> targetShapeCells = new List<Vector2Int>(); // Координаты всех клеток поля, на которые попадает спел
    List<HexCell> targetCells = new List<HexCell>();
    List<UnitDescription> targetUnits = new List<UnitDescription>();
    List<FacilityDescription> targetFacilities = new List<FacilityDescription>();

    private SpellsDescription _preparedSpell = null;
    private UnitDescription _spellCaster = null;
    private PlacementManager _placementManager;
    private HexGrid _hexGrid;

    private void Awake()
    {
        InitComponentLinks();
    }
    private void OnEnable()
    {
        MouseSelection.onSelectionChanged += CastPreparedSpell;
    }
    private void OnDisable()
    {
        MouseSelection.onSelectionChanged -= CastPreparedSpell;
    }


    private void PaintCells(List<Vector2Int> cells, Color newColor)
    {
        List<Color> oldColors = new List<Color>();
        foreach (Vector2Int cell in cells)
        {
            MeshRenderer cellMR = _hexGrid.hexCells[cell.x, cell.y].GetComponent<MeshRenderer>();
            oldColors.Add(cellMR.material.color);
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
        else
        {
            _preparedSpell = null;
            _spellCaster = null;
        }
    }

    private void CastSpell(SpellsDescription spell, Vector2Int targetPosition, UnitDescription caster)
    {
        if (caster.GetComponent<UnitActions>().remainingActionsCount >= spell.MinRemainingActions)
        {
            ChooseTargets(spell, targetPosition, caster);
            DoDamage(spell, targetUnits, targetFacilities, caster);
            DoHeal(spell, targetUnits, targetFacilities, caster);
            SpendActions(spell, caster);
        }
    }

    private void ChooseTargets(SpellsDescription spell, Vector2Int targetPosition, UnitDescription caster)
    {
        targetShapeCells = new List<Vector2Int>();
        targetCells = new List<HexCell>();
        targetUnits = new List<UnitDescription>();
        targetFacilities = new List<FacilityDescription>();

        ChooseCellsInShape(spell, targetPosition, caster);


        foreach (Vector2Int potentialTarget in targetShapeCells)
        {
            ChooseTargetUnits(spell, potentialTarget, caster);

            ChooseTargetFacilities(spell, potentialTarget, caster);

            ChooseTargetCells(spell, potentialTarget);
        }
    }

    private void ChooseCellsInShape(SpellsDescription spell, Vector2Int targetPosition, UnitDescription caster)
    {
        if (spell.IsThreeNeighbourCells)
        {
            List<Vector2Int> casterNeighbours = new List<Vector2Int>(_hexGrid.Neighbours(caster.transform.position));
            List<Vector2Int> targetNeighbours = new List<Vector2Int>(_hexGrid.Neighbours(targetPosition));
            if (casterNeighbours.Contains(targetPosition))
            {
                targetShapeCells.Add(targetPosition);
                foreach (Vector2Int potentialTarget in casterNeighbours)
                {
                    if (targetNeighbours.Contains(potentialTarget))
                    {
                        targetShapeCells.Add(potentialTarget);
                    }
                }
            }
        }

        if (spell.IsLine)
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
                            if (finalPotentialTargets.Contains(potentialTargetNeighbour) && !targetShapeCells.Contains(potentialTargetNeighbour) && potentialTargets.Contains(potentialTargetNeighbour))
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

        if (spell.IsCircle)
        {
            targetShapeCells.Add(targetPosition);
            for (int i = 0; i < spell.CircleRange; i++)
            {
                List<Vector2Int> targetCellsCopy = new List<Vector2Int>();
                foreach (Vector2Int choosedCell in targetShapeCells) // Создаём копию списка и заполняем её, иначе будет ругаться, что мы меняем список во время перебора его элементов
                {
                    targetCellsCopy.Add(choosedCell);
                }

                foreach (Vector2Int choosedCell in targetCellsCopy)
                {
                    foreach (Vector2Int potentialTarget in _hexGrid.Neighbours(choosedCell))
                    {
                        if (!targetShapeCells.Contains(potentialTarget))
                        {
                            targetShapeCells.Add(potentialTarget);
                        }
                    }
                }
            }
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
            int oldArmour = unit.Armor;
            UnitHealth unitHealth = unit.GetComponent<UnitHealth>();
            if (spell.ArmourPenetration != 0)
            {
                unit.Armor = Mathf.Clamp(unit.Armor - spell.ArmourPenetration, 0, unit.Armor);
                unit.ArmorCounter();
            }
            if (spell.DamageCount != 0) unitHealth.ApplyDamage(spell.DamageCount);
            if (spell.PercentageDamageOfMaxTargetHealth != 0) unitHealth.ApplyPercentageDamageOfMaxHealth(spell.PercentageDamageOfMaxTargetHealth);
            if (spell.PercentageDamageOfCurrentTargetHealth != 0) unitHealth.ApplyPercentageDamageOfCurrentHealth(spell.PercentageDamageOfCurrentTargetHealth);
            if (spell.PercentageDamageOfMissingTargetHealth != 0) unitHealth.ApplyPercentageDamageOfMissingHealth(spell.PercentageDamageOfMissingTargetHealth);
            if (spell.PercentageDamageOfMaxCasterHealth != 0) unitHealth.ApplyDamage(caster.Health * spell.PercentageDamageOfMaxCasterHealth / 100);
            if (spell.PercentageDamageOfCurrentCasterHealth != 0) unitHealth.ApplyDamage(caster.GetComponent<UnitHealth>().currentHealth * spell.PercentageDamageOfMaxCasterHealth / 100);
            if (spell.PercentageDamageOfMissingCasterHealth != 0) unitHealth.ApplyDamage((caster.Health - caster.GetComponent<UnitHealth>().currentHealth) * spell.PercentageDamageOfMaxCasterHealth / 100);
            unit.Armor = oldArmour;
            unit.ArmorCounter();
        }

        foreach (FacilityDescription facility in facilities)
        {
            int oldArmour = facility.Armor;
            FacilityHealth facilityHealth = facility.GetComponent<FacilityHealth>();
            if (spell.ArmourPenetration != 0)
            {
                facility.Armor = Mathf.Clamp(facility.Armor - spell.ArmourPenetration, 0, facility.Armor);
                facility.ArmorCounter();
            }
            if (spell.DamageCount != 0) facilityHealth.ApplyDamage(spell.DamageCount);
            //        if (spell.PercentageDamageOfMaxTargetHealth != 0) facilityHealth.ApplyPercentageDamageOfMaxHealth(spell.PercentageDamageOfMaxTargetHealth);  НУЖНО РЕАЛИЗОВАТЬ ЭТИ МЕТОДЫ В FacilityHealth
            //        if (spell.PercentageDamageOfCurrentTargetHealth != 0) facilityHealth.ApplyPercentageDamageOfCurrentHealth(spell.PercentageDamageOfCurrentTargetHealth);  НУЖНО РЕАЛИЗОВАТЬ ЭТИ МЕТОДЫ В FacilityHealth
            //        if (spell.PercentageDamageOfMissingTargetHealth != 0) facilityHealth.ApplyPercentageDamageOfMissingHealth(spell.PercentageDamageOfMissingTargetHealth);  НУЖНО РЕАЛИЗОВАТЬ ЭТИ МЕТОДЫ В FacilityHealth
            if (spell.PercentageDamageOfMaxCasterHealth != 0) facilityHealth.ApplyDamage(caster.Health * spell.PercentageDamageOfMaxCasterHealth / 100);
            if (spell.PercentageDamageOfCurrentCasterHealth != 0) facilityHealth.ApplyDamage(caster.GetComponent<UnitHealth>().currentHealth * spell.PercentageDamageOfMaxCasterHealth / 100);
            if (spell.PercentageDamageOfMissingCasterHealth != 0) facilityHealth.ApplyDamage((caster.Health - caster.GetComponent<UnitHealth>().currentHealth) * spell.PercentageDamageOfMaxCasterHealth / 100);
            facility.Armor = oldArmour;
            facility.ArmorCounter();
        }
    }

    private void DoHeal(SpellsDescription spell, List<UnitDescription> units, List<FacilityDescription> facilities, UnitDescription caster)
    {
        foreach (UnitDescription unit in units)
        {
            UnitHealth unitHealth = unit.GetComponent<UnitHealth>();
            if (spell.HealCount != 0) unitHealth.ApplyHeal(spell.HealCount);
            if (spell.PercentageHealOfMaxTargetHealth != 0) unitHealth.ApplyPercentageHealOfMaxHealth(spell.PercentageHealOfMaxTargetHealth);
            if (spell.PercentageHealOfCurrentTargetHealth != 0) unitHealth.ApplyPercentageHealOfCurrentHealth(spell.PercentageHealOfCurrentTargetHealth);
            if (spell.PercentageHealOfMissingTargetHealth != 0) unitHealth.ApplyPercentageHealOfMissingHealth(spell.PercentageHealOfMissingTargetHealth);
            if (spell.PercentageHealOfMaxCasterHealth != 0) unitHealth.ApplyHeal(caster.Health * spell.PercentageHealOfMaxCasterHealth / 100);
            if (spell.PercentageHealOfCurrentCasterHealth != 0) unitHealth.ApplyHeal(caster.GetComponent<UnitHealth>().currentHealth * spell.PercentageHealOfCurrentCasterHealth / 100);
            if (spell.PercentageHealOfMissingCasterHealth != 0) unitHealth.ApplyHeal((caster.Health - caster.GetComponent<UnitHealth>().currentHealth) * spell.PercentageHealOfMissingCasterHealth / 100);
        }
        foreach (FacilityDescription facility in facilities)
        {
            FacilityHealth facilityHealth = facility.GetComponent<FacilityHealth>();
            /*
            if (spell.HealCount != 0) facilityHealth.ApplyHeal(spell.HealCount);  НУЖНО РЕАЛИЗОВАТЬ ЭТИ МЕТОДЫ В FacilityHealth
            if (spell.PercentageHealOfMaxTargetHealth != 0) facilityHealth.ApplyPercentageHealOfMaxHealth(spell.PercentageHealOfMaxTargetHealth);  НУЖНО РЕАЛИЗОВАТЬ ЭТИ МЕТОДЫ В FacilityHealth
            if (spell.PercentageHealOfCurrentTargetHealth != 0) facilityHealth.ApplyPercentageHealOfCurrentHealth(spell.PercentageHealOfCurrentTargetHealth);  НУЖНО РЕАЛИЗОВАТЬ ЭТИ МЕТОДЫ В FacilityHealth
            if (spell.PercentageHealOfMissingTargetHealth != 0) facilityHealth.ApplyPercentageHealOfMissingHealth(spell.PercentageHealOfMissingTargetHealth);  НУЖНО РЕАЛИЗОВАТЬ ЭТИ МЕТОДЫ В FacilityHealth
            if (spell.PercentageHealOfMaxCasterHealth != 0) facilityHealth.ApplyHeal(caster.Health * spell.PercentageHealOfMaxCasterHealth / 100);  НУЖНО РЕАЛИЗОВАТЬ ЭТИ МЕТОДЫ В FacilityHealth
            if (spell.PercentageHealOfCurrentCasterHealth != 0) facilityHealth.ApplyHeal(caster.GetComponent<UnitHealth>().currentHealth * spell.PercentageHealOfCurrentCasterHealth / 100);  НУЖНО РЕАЛИЗОВАТЬ ЭТИ МЕТОДЫ В FacilityHealth
            if (spell.PercentageHealOfMissingCasterHealth != 0) facilityHealth.ApplyHeal((caster.Health - caster.GetComponent<UnitHealth>().currentHealth) * spell.PercentageHealOfMissingCasterHealth / 100);  НУЖНО РЕАЛИЗОВАТЬ ЭТИ МЕТОДЫ В FacilityHealth
            */
        }
    }

    private void SpendActions(SpellsDescription spell, UnitDescription caster)
    {
        if (spell.SpendActions != 0)
        {
            caster.GetComponent<UnitActions>().remainingActionsCount -= spell.SpendActions;
        }
    }


    private void InitComponentLinks()
    {
        _placementManager = FindObjectOfType<PlacementManager>();
        _hexGrid = FindObjectOfType<HexGrid>();
    }

}