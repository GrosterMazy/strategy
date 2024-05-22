using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetsInDarkness : MonoBehaviour {
    public List<Vector2Int> Targets = new List<Vector2Int>();

    private void OnEnable()
    {
        TurnManager.onTurnChanged += ClearTargets;
    }
    private void OnDisable()
    {
        TurnManager.onTurnChanged -= ClearTargets;
    }
    private void ClearTargets()
    {
        Targets.Clear();
    }
    // кликом выставлять цель для юнитов тьмы
    /*
    private HexGrid _hexGrid;
    public void OnEnable() {
        this._hexGrid = FindObjectOfType<HexGrid>();

        MouseSelection.onSelectionChanged += test;
    }
    public void OnDisable() {
        MouseSelection.onSelectionChanged -= test;
    }

    public void test(Transform newSelection) {
        if (newSelection != null)
            this.AddTarget(this._hexGrid.InLocalCoords(newSelection.transform.position));
    }
    */

    public void AddTarget(Vector2Int _coords) { Targets.Add(_coords); }
    public void RemoveTarget(Vector2Int _coords) { Targets.Remove(_coords); }
}
