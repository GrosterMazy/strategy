using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitActions : MonoBehaviour
{
    public byte remainingActionsCount;
    private SelectionController _selectionController => FindObjectOfType<SelectionController>();
    private HighlightedController _highlightedController => FindObjectOfType<HighlightedController>();
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
        if (_mouseSelection.highlighted == null) return;
        if (_unitDescription.IsSelected && _highlightedController.isAnyUnitHighlighted && _unitDescription.AttackRange >= _highlightedController.distanceBetweenSelectedAndHighlighted && remainingActionsCount != 0)
        {
            _highlightedController.highlightedUnit.GetComponent<UnitHealth>().ApplyDamage(_unitDescription.AttackDamage);
            remainingActionsCount -= 1;
        }
    }

    private void UpdateActionsCountOnTurnChanged()
    {
        remainingActionsCount = _unitDescription.ActionsPerTurn;
    }
}
