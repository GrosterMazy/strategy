using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DarknessUnitAI : UnitDescription {
    private PlacementManager _placementManager;
    private TargetsInDarkness _targetsInDarkness;
    private HexGrid _hexGrid;
    private DarknessUnitSpawner _darknessUnitSpawner;

    private ObjectOnGrid _underMe;

    private void OnEnable() =>
        TurnManager.onTurnChanged += OnTurnChanged;
    private void OnDisable() =>
        TurnManager.onTurnChanged -= OnTurnChanged;

    private void Awake() {
        this._placementManager = FindObjectOfType<PlacementManager>();
        this._targetsInDarkness = FindObjectOfType<TargetsInDarkness>();
        this._hexGrid = FindObjectOfType<HexGrid>();
        this._darknessUnitSpawner = FindObjectOfType<DarknessUnitSpawner>();
    }

    private void OnDestroy() {
        this._placementManager.gridWithObjectsInformation[this.LocalCoords.x, this.LocalCoords.y] = this._underMe;
        this._darknessUnitSpawner.mobCapFilled--;
    }
        

    public void OnTurnChanged() {
        // Выбираем ближайшую цель
        int mindist = 1_000_000;
        int dist;
        Vector2Int target = Vector2Int.zero;

        if (this._targetsInDarkness.Targets.Count == 0) return;

        foreach (Vector2Int pos in this._targetsInDarkness.Targets) {
            dist = this._hexGrid.Distance(pos, this.LocalCoords);
            if (dist < mindist) {
                mindist = dist;
                target = pos;
            }
        }

        if (mindist == 1_000_000) return;

        UnitHealth unitHealth = null;
        FacilityHealth facilityHealth = null;
        if (this._placementManager.gridWithObjectsInformation[target.x, target.y] != null) {
            unitHealth = this._placementManager.gridWithObjectsInformation[target.x, target.y]
                .GetComponent<UnitHealth>();
            facilityHealth = this._placementManager.gridWithObjectsInformation[target.x, target.y]
                .GetComponent<FacilityHealth>();
        }

        if ((facilityHealth != null || unitHealth != null)
                && this._hexGrid.Distance(this.LocalCoords, target) <= this.AttackRange) {
            int remainingActions = this.ActionsPerTurn;
            while (remainingActions > 0) {
                
                if (unitHealth != null) {
                    unitHealth.ApplyDamage(this.AttackDamage);
                }
                    
                if (facilityHealth != null) {
                    facilityHealth.ApplyDamage(this.AttackDamage);
                }

                EventBus.anyUnitSpendAction?.Invoke();
                remainingActions--;
            }
            return;
        }

        // выбираем клетку, на которую нам сходить
        Vector2Int newpos = this.LocalCoords;

        int remainingSpeed = this.MovementSpeed;
        while (remainingSpeed > 0) {
            Vector2Int delta = target - newpos;
            Vector2Int absDelta = new Vector2Int(Mathf.Abs(delta.x), Mathf.Abs(delta.y));

            if (newpos.y % 2 == 0) {
                if (delta.y != 0) {
                    newpos.y += Mathf.Clamp(delta.y, -1, 1);
                    if (delta.x < 0) newpos.x--;// skip
                }
                else newpos.x += Mathf.Clamp(delta.x, -1, 1);
            }
            else if (delta.y != 0) {
                newpos.y += Mathf.Clamp(delta.y, -1, 1);
                if (delta.x > 0) newpos.x++;// skip
            }
            else newpos.x += Mathf.Clamp(delta.x, -1, 1);

            remainingSpeed--;

            if ((facilityHealth != null || unitHealth != null)
                    && this._hexGrid.Distance(this.LocalCoords, target) <= this.AttackRange) 
                break;
        }

        // Debug.Log(("from", this.LocalCoords, "to", newpos));

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
        if (newpos != this.LocalCoords)
            this._placementManager.gridWithObjectsInformation[this.LocalCoords.x, this.LocalCoords.y] = this._underMe;
        
        // сохраняем то, на что собираемся наступить 
        if (found)
            this._underMe = this._placementManager.gridWithObjectsInformation[newpos.x, newpos.y];
        else this._underMe = null;

        // двигаемся
        this._placementManager.gridWithObjectsInformation[newpos.x, newpos.y] = this;
        this.LocalCoords = newpos;
        this.transform.position = this._hexGrid.InUnityCoords(this.LocalCoords);

        // убираем цель, если её достигли
        if (this.LocalCoords == target)
            this._targetsInDarkness.RemoveTarget(target);
    }
}
