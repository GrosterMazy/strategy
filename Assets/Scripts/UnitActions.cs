using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitActions : MonoBehaviour
{
    public int remainingActionsCount;
    private HexGrid _hexGrid => FindObjectOfType<HexGrid>();
    private SelectionController _selectionController => FindObjectOfType<SelectionController>();
    private HighlightingController _highlightedController => FindObjectOfType<HighlightingController>();
    private MouseSelection _mouseSelection => FindObjectOfType<MouseSelection>();
    private UnitDescription _unitDescription => GetComponent<UnitDescription>();

    private void OnEnable()
    {
        TurnManager.onTurnChanged += UpdateActionsCountOnTurnChanged;
    }
    private void OnDisable()
    {
        TurnManager.onTurnChanged -= UpdateActionsCountOnTurnChanged;
    }
    private void Start()
    {
        UpdateActionsCountOnTurnChanged();
    }
    private void Update()
    {
        if (Input.GetMouseButton(1))
        {
            Attack();
        }
    }

    private void Attack()
    {
        if (_mouseSelection.highlighted == null || !_unitDescription.IsSelected 
            || _unitDescription.AttackRange < _hexGrid.Distance(_mouseSelection.selected.position, _mouseSelection.highlighted.position)
            || remainingActionsCount == 0) return;

        if (_highlightedController.isAnyUnitHighlighted && _highlightedController.highlightedUnit.TeamAffiliation != _unitDescription.TeamAffiliation)
        {
            _highlightedController.highlightedUnit.GetComponent<UnitHealth>().ApplyDamage(_unitDescription.AttackDamage); // Атакуем вражеского юнита
            remainingActionsCount -= 1;
        }
        else if (_highlightedController.isAnyFirstFactionFacilityHighlighted && _highlightedController.highlightedFirstFactionFacility.TeamAffiliation != _unitDescription.TeamAffiliation)
        {
            _highlightedController.highlightedFirstFactionFacility.GetComponent<FacilityHealth>().ApplyDamage(_unitDescription.AttackDamage); // Атакуем вражеское здание первой фракции
            remainingActionsCount -= 1;
        }
    }

    private void UpdateActionsCountOnTurnChanged()
    {
        remainingActionsCount = _unitDescription.ActionsPerTurn;
    }
}
