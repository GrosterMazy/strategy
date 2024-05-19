using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DarknessUnitAI : UnitDescription
{
    private List<int> _distances = new List<int>();
    private PlacementManager _placementManager;
    private TargetsInDarkness _targetsInDarkness;
    private HexGrid _hexGrid;
    private ObjectOnGrid _objectOnGrid;

    private void Movement() { _distances.Clear();
            foreach (Vector2Int _coords in _targetsInDarkness.Targets) { _distances.Add(_hexGrid.Distance(LocalCoords, _coords)); }
            Vector2Int _target = _targetsInDarkness.Targets[_distances.IndexOf(_distances.Min())];
            int _next_x = LocalCoords.x + Mathf.Clamp(_target.x - LocalCoords.x, -1, 1);
            int _next_y = LocalCoords.y + Mathf.Clamp(_target.y - LocalCoords.y, -1, 1);
        if (_placementManager.gridWithObjectsInformation[_next_x, _next_y] == null) { _placementManager.UpdateGrid(LocalCoords, new Vector2Int(_next_x, _next_y), _objectOnGrid); transform.position = _hexGrid.InUnityCoords(new Vector2Int(_next_x, _next_y)); }
        Attack(); }

    private void Attack() { foreach (Vector2Int _neighbour in _hexGrid.Neighbours(LocalCoords)) { 
            if (_placementManager.gridWithObjectsInformation[_neighbour.x, _neighbour.y] != null) { 
                                UnitHealth _unitHealth = _placementManager.gridWithObjectsInformation[_neighbour.x, _neighbour.y].GetComponent<UnitHealth>();
                                if (_unitHealth != null) { _unitHealth.ApplyDamage(AttackDamage); return; }
                                FacilityHealth _facilityHealth = _placementManager.gridWithObjectsInformation[_neighbour.x, _neighbour.y].GetComponent<FacilityHealth>();
                                if (_facilityHealth != null) { _facilityHealth.ApplyDamage(AttackDamage); return; } } } }

    private void InitComponents() { _placementManager = FindObjectOfType<PlacementManager>(); _targetsInDarkness = FindObjectOfType<TargetsInDarkness>();
        _hexGrid = FindObjectOfType<HexGrid>(); _objectOnGrid = GetComponent<ObjectOnGrid>(); }

    private void Start() { InitComponents(); }

    private void OnEnable() { TurnManager.onTurnChanged += Movement; }
    private void OnDisable() { TurnManager.onTurnChanged -= Movement; }
}
