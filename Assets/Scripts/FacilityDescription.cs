using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using System.Linq;

public class FacilityDescription : ObjectOnGrid
{
    public List<float> ArmorEfficiencyTable;
    public int TeamAffiliation;
    public int Armor;
    public bool IsSelected; // тыкнул мышкой на него
    public bool IsHighlighted; // Навёл мышку
    public bool WorkerOnSite;
    public float MaxHealth;
    public float ArmorUnitEfficiencyMaxAmount;  // Значение максимальной эффективности брони(т.е. на сколько % будет снижен урон за первую единицу брони)
    public float ArmorEfficiencyDecreasementPerUnit; // То, насколько будет снижаться эффективность каждой последующей единицы брони(в %)
    [NonSerialized] public float DamageReductionPercent;
    public Transform _highlighted => FindObjectOfType<MouseSelection>().highlighted;
    public PlacementManager _placementManager => FindObjectOfType<PlacementManager>();
    public HexGrid _hexGrid => FindObjectOfType<HexGrid>();
    public GameObject WorkerInsideMe;

    public void ArmorCounter() {
        if (ArmorEfficiencyDecreasementPerUnit <= 0) { throw new Exception("Убывающая полезность брони не может быть равна или меньше 0"); }
        var _maxArmorAccordingToPrimaryRules = ArmorUnitEfficiencyMaxAmount / ArmorEfficiencyDecreasementPerUnit;
        if (_maxArmorAccordingToPrimaryRules != Mathf.RoundToInt(_maxArmorAccordingToPrimaryRules)) { _maxArmorAccordingToPrimaryRules = Mathf.RoundToInt(_maxArmorAccordingToPrimaryRules) + 1; }
        for (int i = 0; i < _maxArmorAccordingToPrimaryRules + 1; i++) ArmorEfficiencyTable.Add(0);
        for (int i = 0; i < _maxArmorAccordingToPrimaryRules + 1; i++) {
            var _armorModification = ArmorUnitEfficiencyMaxAmount - ArmorEfficiencyDecreasementPerUnit * (i - 1);
            if (i == 0) ArmorEfficiencyTable[i] = 0;
            else if (i <= _maxArmorAccordingToPrimaryRules && _armorModification > 0) ArmorEfficiencyTable[i] = Mathf.Clamp(ArmorEfficiencyTable[i - 1] + _armorModification, 0, 100);
            else ArmorEfficiencyTable.RemoveAt(ArmorEfficiencyTable.Count - 1); }
        if (ArmorEfficiencyTable[ArmorEfficiencyTable.Count - 1] == 100) { ArmorEfficiencyTable = (from _percent in ArmorEfficiencyTable where _percent != 100 select _percent).ToList(); ArmorEfficiencyTable.Add(100); }
        Armor = Mathf.Clamp(Armor, 0, ArmorEfficiencyTable.Count - 1);
        DamageReductionPercent = ArmorEfficiencyTable[Armor]; }

    public void ThrowAwayWorker() { 
        if (_highlighted == null) { return; }
        Vector2Int _highlightedInLocalCoords = new Vector2Int(_hexGrid.InLocalCoords(_highlighted.position).x, _hexGrid.InLocalCoords(_highlighted.position).y);
        if (IsSelected && WorkerOnSite && _placementManager.gridWithObjectsInformation[_highlightedInLocalCoords.x, _highlightedInLocalCoords.y] == null && Input.GetKeyDown(KeyCode.T)) {
            ObjectOnGrid _workerInsideMeLocalCoords = WorkerInsideMe.GetComponent<ObjectOnGrid>();
            WorkerInsideMe.SetActive(true); WorkerOnSite = false; WorkerInsideMe.transform.position = _highlighted.position; _workerInsideMeLocalCoords.LocalCoords = new Vector2Int(_highlightedInLocalCoords.x, _highlightedInLocalCoords.y);
            _placementManager.UpdateGrid(_highlighted.position, _highlighted.position, _workerInsideMeLocalCoords); } }

    private void Start() {
        ArmorCounter(); }

    private void Update() {
        ThrowAwayWorker(); }
}
