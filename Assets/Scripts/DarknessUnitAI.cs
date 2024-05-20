using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DarknessUnitAI : UnitDescription {
    private PlacementManager _placementManager;
    private TargetsInDarkness _targetsInDarkness;
    private HexGrid _hexGrid;

    private ObjectOnGrid _underMe;

    private void OnEnable() {
        TurnManager.onTurnChanged += OnTurnChanged;
    }
    private void OnDisable() {
        TurnManager.onTurnChanged -= OnTurnChanged;
    }

    private void Start() {
        this._placementManager = FindObjectOfType<PlacementManager>();
        this._targetsInDarkness = FindObjectOfType<TargetsInDarkness>();
        this._hexGrid = FindObjectOfType<HexGrid>();
    }

    private void OnTurnChanged() {
        // Выбираем ближайшую цель
        int mindist = 1_000_000;
        int dist;
        Vector2Int target = Vector2Int.zero;

        foreach (Vector2Int pos in this._targetsInDarkness.Targets) {
            dist = this._hexGrid.Distance(pos, this.LocalCoords);
            if (dist < mindist) {
                mindist = dist;
                target = pos;
            }
        }

        if (mindist == 1_000_000) return;

        // выбираем клетку, на которую нам сходить
        Vector2Int newpos = this.LocalCoords;
        Vector2Int delta = target - newpos;
        Vector2Int absDelta = new Vector2Int(Mathf.Abs(delta.x), Mathf.Abs(delta.y));

        if (newpos.y % 2 == 0) {
            if (delta.y != 0 && absDelta.y >= absDelta.x) {
                newpos.y += Mathf.Clamp(delta.y, -1, 1);
                if (delta.x < 0) newpos.x--;// skip
            }
            else newpos.x += Mathf.Clamp(delta.x, -1, 1);
        }
        else if (delta.y != 0 && absDelta.y >= absDelta.x) {
            newpos.y += Mathf.Clamp(delta.y, -1, 1);
            if (delta.x > 0) newpos.x++;// skip
        }
        else newpos.x += Mathf.Clamp(delta.x, -1, 1);

        bool found = false;
        // если будущей нашей позиции есть что-то
        if (this._placementManager.gridWithObjectsInformation[newpos.x, newpos.y] != null) {
            DarknessUnitAI darknessUnitAI = this._placementManager.gridWithObjectsInformation[newpos.x, newpos.y]
                .GetComponent<DarknessUnitAI>();

            // пропускаем братьев по оружию
            if (darknessUnitAI != null) return;

            found = true;
        }

        // возвращаем то, что было под нами
        if (newpos != this.LocalCoords && this._underMe != null)
            this._placementManager.gridWithObjectsInformation[this.LocalCoords.x, this.LocalCoords.y] = this._underMe;
        
        // сохраняем то, на что собираемся наступить 
        if (found)
            this._underMe = this._placementManager.gridWithObjectsInformation[newpos.x, newpos.y];

        // двигаемся
        this._placementManager.UpdateGrid(this.LocalCoords, newpos, this);
        this.LocalCoords = newpos;
        this.transform.position = this._hexGrid.InUnityCoords(this.LocalCoords);

        // убираем цель, если её достигли
        if (this.LocalCoords == target)
            this._targetsInDarkness.RemoveTarget(target);

        // aтакуем
        foreach (Vector2Int neighbour in this._hexGrid.Neighbours(this.LocalCoords))
            if (this._placementManager.gridWithObjectsInformation[neighbour.x, neighbour.y] != null) { 
                UnitHealth unitHealth = this._placementManager.gridWithObjectsInformation[neighbour.x, neighbour.y]
                    .GetComponent<UnitHealth>();
                if (unitHealth != null) {
                    unitHealth.ApplyDamage(this.AttackDamage);
                    break;
                }

                FacilityHealth facilityHealth = this._placementManager.gridWithObjectsInformation[neighbour.x, neighbour.y]
                    .GetComponent<FacilityHealth>();
                if (facilityHealth != null) {
                    facilityHealth.ApplyDamage(this.AttackDamage);
                    break;
                }
            }
    }
}
