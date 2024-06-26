﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UnitActions : MonoBehaviour
{
    [NonSerialized] public int remainingActionsCount;
    private HexGrid _hexGrid;
    private SelectionController _selectionController;
    private HighlightingController _highlightedController;
    private MouseSelection _mouseSelection;
    private UnitDescription _unitDescription;
    private TurnManager _turnManager;

    private void Awake()
    {
        InitComponentLinks();
    }

    private void OnEnable()
    {
        _turnManager.onTurnChanged += UpdateActionsCountOnTurnChanged;
    }
    private void OnDisable()
    {
        _turnManager.onTurnChanged -= UpdateActionsCountOnTurnChanged;
    }
    private void Start()
    {
        UpdateActionsCountOnTurnChanged();
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Attack();
        }
    }

    public void SpendAction(int count)
    {
        remainingActionsCount -= count;
        EventBus.anyUnitSpendAction?.Invoke();
    }

    private void Attack()
    {
        if (_mouseSelection.highlighted == null || !_unitDescription.IsSelected 
            || _unitDescription.AttackRange < _hexGrid.Distance(_mouseSelection.selected.position, _mouseSelection.highlighted.position)
            || remainingActionsCount == 0) return;

        if (_highlightedController.isAnyUnitHighlighted && _highlightedController.highlightedUnit.TeamAffiliation != _unitDescription.TeamAffiliation)
        {
            _highlightedController.highlightedUnit.GetComponent<Health>().ApplyDamage(_unitDescription.AttackDamage); // Атакуем вражеского юнита
            SpendAction(1);
        }
        else if (_highlightedController.isAnyFirstFactionFacilityHighlighted && _highlightedController.highlightedFirstFactionFacility.TeamAffiliation != _unitDescription.TeamAffiliation)
        {
            _highlightedController.highlightedFirstFactionFacility.GetComponent<FacilityHealth>().ApplyDamage(_unitDescription.AttackDamage); // Атакуем вражеское здание первой фракции
            SpendAction(1);
        }
    }

    private void UpdateActionsCountOnTurnChanged()
    {
        remainingActionsCount = _unitDescription.ActionsPerTurn;
    }


    private void InitComponentLinks()
    {
        _hexGrid = FindObjectOfType<HexGrid>();
        _turnManager = FindObjectOfType<TurnManager>();
        _selectionController = FindObjectOfType<SelectionController>();
        _highlightedController = FindObjectOfType<HighlightingController>();
        _mouseSelection = FindObjectOfType<MouseSelection>();
        _unitDescription = GetComponent<UnitDescription>();
    }
}
