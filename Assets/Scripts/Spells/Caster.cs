using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Caster : MonoBehaviour
{
    public bool updatingHelpProperty; // Вспомогательная переменная для инспектора
    public List<SpelsDescription> SpelsList = new List<SpelsDescription>();

    List<Vector2Int> targetShapeCells = new List<Vector2Int>(); // Координаты всех клеток поля, на которые попадает спел
    List<HexCell> targetCells = new List<HexCell>();
    List<UnitDescription> targetUnits = new List<UnitDescription>();
    List<FacilityDescription> targetFacilities = new List<FacilityDescription>();

    private SpelsDescription _preparedSpel = null;
    private UnitDescription _spelCaster = null;
    private PlacementManager _placementManager;
    private HexGrid _hexGrid;

    private void Awake()
    {
        InitComponentLinks();
    }
    private void OnEnable()
    {
        MouseSelection.onSelectionChanged += CastPreparedSpel;
    }
    private void OnDisable()
    {
        MouseSelection.onSelectionChanged -= CastPreparedSpel;
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

    public void PrepareSpel(SpelsDescription spel, UnitDescription caster) // Только для спелов, кастуемых через кнопку
    {
        _preparedSpel = spel;
        _spelCaster = caster;
    }

    private void CastPreparedSpel(Transform selected)
    {
        if (_preparedSpel != null && _spelCaster != null && selected != null)
        {
            CastSpel(_preparedSpel, _hexGrid.InLocalCoords(selected.position), _spelCaster);
            PaintCells(targetShapeCells, Color.red);
            _preparedSpel = null;
            _spelCaster = null;
        }
        else
        {
            _preparedSpel = null;
            _spelCaster = null;
        }
    }

    private void CastSpel(SpelsDescription spel, Vector2Int targetPosition, UnitDescription caster)
    {
        if (caster.GetComponent<UnitActions>().remainingActionsCount >= spel.MinRemainingActions)
        {
            ChooseTargets(spel, targetPosition, caster);
            DoDamage(spel, targetUnits, targetFacilities, caster);
            DoHeal(spel, targetUnits, targetFacilities, caster);
            SpendActions(spel, caster);
        }
    }

    private void ChooseTargets(SpelsDescription spel, Vector2Int targetPosition, UnitDescription caster)
    {
        targetShapeCells = new List<Vector2Int>();
        targetCells = new List<HexCell>();
        targetUnits = new List<UnitDescription>();
        targetFacilities = new List<FacilityDescription>();

        ChooseCellsInShape(spel, targetPosition, caster);


        foreach (Vector2Int potentialTarget in targetShapeCells)
        {
            ChooseTargetUnits(spel, potentialTarget, caster);

            ChooseTargetFacilities(spel, potentialTarget, caster);

            ChooseTargetCells(spel, potentialTarget);
        }
    }

    private void ChooseCellsInShape(SpelsDescription spel, Vector2Int targetPosition, UnitDescription caster)
    {
        if (spel.IsThreeNeighbourCells)
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

        if (spel.IsLine)
        {
            List<Vector2Int> casterNeighbours = new List<Vector2Int>(_hexGrid.Neighbours(caster.transform.position));
            List<Vector2Int> targetNeighbours = new List<Vector2Int>(_hexGrid.Neighbours(targetPosition));
            if (casterNeighbours.Contains(targetPosition))
            {
                Vector2Int previousTarget = _hexGrid.InLocalCoords(caster.transform.position);
                targetShapeCells.Add(targetPosition);
                for (int i = 1; i < spel.LineRange; i++)
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

        if (spel.IsCircle)
        {
            targetShapeCells.Add(targetPosition);
            for (int i = 0; i < spel.CircleRange; i++)
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
        if (spel.IsLine) // Тут допущена ошибка, но из-за неё получался прикольный рисунок
        {
            List<Vector2Int> casterNeighbours = new List<Vector2Int>(_hexGrid.Neighbours(caster.transform.position));
            List<Vector2Int> targetNeighbours = new List<Vector2Int>(_hexGrid.Neighbours(targetPosition));
            if (casterNeighbours.Contains(targetPosition))
            {
                Vector2Int previousTarget = _hexGrid.InLocalCoords(caster.transform.position);
                targetShapeCells.Add(targetPosition);
                for (int i = 1; i < spel.LineRange; i++)
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

    private void ChooseTargetUnits(SpelsDescription spel, Vector2Int potentialTarget, UnitDescription caster)
    {
        if (_placementManager.gridWithObjectsInformation[potentialTarget.x, potentialTarget.y] != null
            && _placementManager.gridWithObjectsInformation[potentialTarget.x, potentialTarget.y].TryGetComponent<UnitDescription>(out UnitDescription unitTarget))
        {
            if (spel.OnEnemyUnits && unitTarget.TeamAffiliation != caster.TeamAffiliation)
            {
                targetUnits.Add(unitTarget);
            }
            if (spel.OnTeammateUnits && unitTarget.TeamAffiliation == caster.TeamAffiliation)
            {
                targetUnits.Add(unitTarget);
            }
        }
    }
    private void ChooseTargetFacilities(SpelsDescription spel, Vector2Int potentialTarget, UnitDescription caster)
    {
        if (_placementManager.gridWithObjectsInformation[potentialTarget.x, potentialTarget.y] != null
            && _placementManager.gridWithObjectsInformation[potentialTarget.x, potentialTarget.y].TryGetComponent<FacilityDescription>(out FacilityDescription facilityTarget))
        {
            if (spel.OnEnemyBuildings && facilityTarget.TeamAffiliation != caster.TeamAffiliation)
            {
                targetFacilities.Add(facilityTarget);
            }
            if (spel.OnTeammateBuildings && facilityTarget.TeamAffiliation == caster.TeamAffiliation)
            {
                targetFacilities.Add(facilityTarget);
            }
        }
    }
    private void ChooseTargetCells(SpelsDescription spel, Vector2Int potentialTarget)
    {
        if (spel.OnGround && _hexGrid.hexCells[potentialTarget.x, potentialTarget.y] != null)
        {
            targetCells.Add(_hexGrid.hexCells[potentialTarget.x, potentialTarget.y]);
        }
    }

    private void DoDamage(SpelsDescription spel, List<UnitDescription> units, List<FacilityDescription> facilities, UnitDescription caster)
    {
        foreach (UnitDescription unit in units)
        {
            int oldArmour = unit.Armor;
            UnitHealth unitHealth = unit.GetComponent<UnitHealth>();
            if (spel.ArmourPenetration != 0)
            {
                unit.Armor = Mathf.Clamp(unit.Armor - spel.ArmourPenetration, 0, unit.Armor);
                unit.ArmorCounter();
            }
            if (spel.DamageCount != 0) unitHealth.ApplyDamage(spel.DamageCount);
            if (spel.PercentageDamageOfMaxTargetHealth != 0) unitHealth.ApplyPercentageDamageOfMaxHealth(spel.PercentageDamageOfMaxTargetHealth);
            if (spel.PercentageDamageOfCurrentTargetHealth != 0) unitHealth.ApplyPercentageDamageOfCurrentHealth(spel.PercentageDamageOfCurrentTargetHealth);
            if (spel.PercentageDamageOfMissingTargetHealth != 0) unitHealth.ApplyPercentageDamageOfMissingHealth(spel.PercentageDamageOfMissingTargetHealth);
            if (spel.PercentageDamageOfMaxCasterHealth != 0) unitHealth.ApplyDamage(caster.Health * spel.PercentageDamageOfMaxCasterHealth / 100);
            if (spel.PercentageDamageOfCurrentCasterHealth != 0) unitHealth.ApplyDamage(caster.GetComponent<UnitHealth>().currentHealth * spel.PercentageDamageOfMaxCasterHealth / 100);
            if (spel.PercentageDamageOfMissingCasterHealth != 0) unitHealth.ApplyDamage((caster.Health - caster.GetComponent<UnitHealth>().currentHealth) * spel.PercentageDamageOfMaxCasterHealth / 100);
            unit.Armor = oldArmour;
            unit.ArmorCounter();
        }

        foreach (FacilityDescription facility in facilities)
        {
            int oldArmour = facility.Armor;
            FacilityHealth facilityHealth = facility.GetComponent<FacilityHealth>();
            if (spel.ArmourPenetration != 0)
            {
                facility.Armor = Mathf.Clamp(facility.Armor - spel.ArmourPenetration, 0, facility.Armor);
                facility.ArmorCounter();
            }
            if (spel.DamageCount != 0) facilityHealth.ApplyDamage(spel.DamageCount);
            //        if (spel.PercentageDamageOfMaxTargetHealth != 0) facilityHealth.ApplyPercentageDamageOfMaxHealth(spel.PercentageDamageOfMaxTargetHealth);  НУЖНО РЕАЛИЗОВАТЬ ЭТИ МЕТОДЫ В FacilityHealth
            //        if (spel.PercentageDamageOfCurrentTargetHealth != 0) facilityHealth.ApplyPercentageDamageOfCurrentHealth(spel.PercentageDamageOfCurrentTargetHealth);  НУЖНО РЕАЛИЗОВАТЬ ЭТИ МЕТОДЫ В FacilityHealth
            //        if (spel.PercentageDamageOfMissingTargetHealth != 0) facilityHealth.ApplyPercentageDamageOfMissingHealth(spel.PercentageDamageOfMissingTargetHealth);  НУЖНО РЕАЛИЗОВАТЬ ЭТИ МЕТОДЫ В FacilityHealth
            if (spel.PercentageDamageOfMaxCasterHealth != 0) facilityHealth.ApplyDamage(caster.Health * spel.PercentageDamageOfMaxCasterHealth / 100);
            if (spel.PercentageDamageOfCurrentCasterHealth != 0) facilityHealth.ApplyDamage(caster.GetComponent<UnitHealth>().currentHealth * spel.PercentageDamageOfMaxCasterHealth / 100);
            if (spel.PercentageDamageOfMissingCasterHealth != 0) facilityHealth.ApplyDamage((caster.Health - caster.GetComponent<UnitHealth>().currentHealth) * spel.PercentageDamageOfMaxCasterHealth / 100);
            facility.Armor = oldArmour;
            facility.ArmorCounter();
        }
    }

    private void DoHeal(SpelsDescription spel, List<UnitDescription> units, List<FacilityDescription> facilities, UnitDescription caster)
    {
        foreach (UnitDescription unit in units)
        {
            UnitHealth unitHealth = unit.GetComponent<UnitHealth>();
            if (spel.HealCount != 0) unitHealth.ApplyHeal(spel.HealCount);
            if (spel.PercentageHealOfMaxTargetHealth != 0) unitHealth.ApplyPercentageHealOfMaxHealth(spel.PercentageHealOfMaxTargetHealth);
            if (spel.PercentageHealOfCurrentTargetHealth != 0) unitHealth.ApplyPercentageHealOfCurrentHealth(spel.PercentageHealOfCurrentTargetHealth);
            if (spel.PercentageHealOfMissingTargetHealth != 0) unitHealth.ApplyPercentageHealOfMissingHealth(spel.PercentageHealOfMissingTargetHealth);
            if (spel.PercentageHealOfMaxCasterHealth != 0) unitHealth.ApplyHeal(caster.Health * spel.PercentageHealOfMaxCasterHealth / 100);
            if (spel.PercentageHealOfCurrentCasterHealth != 0) unitHealth.ApplyHeal(caster.GetComponent<UnitHealth>().currentHealth * spel.PercentageHealOfCurrentCasterHealth / 100);
            if (spel.PercentageHealOfMissingCasterHealth != 0) unitHealth.ApplyHeal((caster.Health - caster.GetComponent<UnitHealth>().currentHealth) * spel.PercentageHealOfMissingCasterHealth / 100);
        }
        foreach (FacilityDescription facility in facilities)
        {
            FacilityHealth facilityHealth = facility.GetComponent<FacilityHealth>();
            /*
            if (spel.HealCount != 0) facilityHealth.ApplyHeal(spel.HealCount);  НУЖНО РЕАЛИЗОВАТЬ ЭТИ МЕТОДЫ В FacilityHealth
            if (spel.PercentageHealOfMaxTargetHealth != 0) facilityHealth.ApplyPercentageHealOfMaxHealth(spel.PercentageHealOfMaxTargetHealth);  НУЖНО РЕАЛИЗОВАТЬ ЭТИ МЕТОДЫ В FacilityHealth
            if (spel.PercentageHealOfCurrentTargetHealth != 0) facilityHealth.ApplyPercentageHealOfCurrentHealth(spel.PercentageHealOfCurrentTargetHealth);  НУЖНО РЕАЛИЗОВАТЬ ЭТИ МЕТОДЫ В FacilityHealth
            if (spel.PercentageHealOfMissingTargetHealth != 0) facilityHealth.ApplyPercentageHealOfMissingHealth(spel.PercentageHealOfMissingTargetHealth);  НУЖНО РЕАЛИЗОВАТЬ ЭТИ МЕТОДЫ В FacilityHealth
            if (spel.PercentageHealOfMaxCasterHealth != 0) facilityHealth.ApplyHeal(caster.Health * spel.PercentageHealOfMaxCasterHealth / 100);  НУЖНО РЕАЛИЗОВАТЬ ЭТИ МЕТОДЫ В FacilityHealth
            if (spel.PercentageHealOfCurrentCasterHealth != 0) facilityHealth.ApplyHeal(caster.GetComponent<UnitHealth>().currentHealth * spel.PercentageHealOfCurrentCasterHealth / 100);  НУЖНО РЕАЛИЗОВАТЬ ЭТИ МЕТОДЫ В FacilityHealth
            if (spel.PercentageHealOfMissingCasterHealth != 0) facilityHealth.ApplyHeal((caster.Health - caster.GetComponent<UnitHealth>().currentHealth) * spel.PercentageHealOfMissingCasterHealth / 100);  НУЖНО РЕАЛИЗОВАТЬ ЭТИ МЕТОДЫ В FacilityHealth
            */
        }
    }

    private void SpendActions(SpelsDescription spel, UnitDescription caster)
    {
        if (spel.SpendActions != 0)
        {
            caster.GetComponent<UnitActions>().remainingActionsCount -= spel.SpendActions;
        }
    }


    private void InitComponentLinks()
    {
        _placementManager = FindObjectOfType<PlacementManager>();
        _hexGrid = FindObjectOfType<HexGrid>();
    }

}