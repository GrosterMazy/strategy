using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class LightTransporter : MonoBehaviour {
    public LightTransporter Source;
    public int LightForce;
    public int LightDecrease;

    [NonSerialized] public HexCell cell;
    [NonSerialized] public HexGrid grid;
    [NonSerialized] public TurnManager turnManager;
    [NonSerialized] public Action<LightTransporter, int> OnLightForceChange;

    private int _oldLightForce;
    private int _oldLightDecrease;

    private void Start() {
        foreach(Vector2Int Coords in grid.Neighbours(transform.position))
            grid.hexCells[Coords.x, Coords.y].GetComponent<LightTransporter>().OnLightForceChange += OnLightSourceChanged;
    }
    private void OnDestroy() {
        foreach (Vector2Int Coords in grid.Neighbours(transform.position))
            grid.hexCells[Coords.x, Coords.y].GetComponent<LightTransporter>().OnLightForceChange -= OnLightSourceChanged;
    }
    private void OnLightSourceChanged(LightTransporter NewSource, int NewLightForce) {
        if (NewSource == Source) {
            if (turnManager.isDay && NewLightForce - NewSource.LightDecrease <= DarknessMainVariables.LightForce
                    || !turnManager.isDay && NewLightForce - NewSource.LightDecrease <= 0) {
                SetLight(_oldLightForce,_oldLightDecrease);
                return;
            }
            LightForce = NewLightForce - NewSource.LightDecrease;
            LightDecrease = NewSource.LightDecrease;
            cell.LightRate = (turnManager.isDay ? DarknessMainVariables.LightForce : 0) + NewLightForce-NewSource.LightDecrease;
            OnLightForceChange?.Invoke(this, NewLightForce-NewSource.LightDecrease);
            return;
        }
        if (turnManager.isDay && NewLightForce - NewSource.LightDecrease <= DarknessMainVariables.LightForce
                || !turnManager.isDay && NewLightForce - NewSource.LightDecrease <= 0)
            return;
        if (NewLightForce - NewSource.LightDecrease > LightForce) {
            if (Source==null) {
                _oldLightForce = LightForce;
                _oldLightDecrease = LightDecrease;
            }
            LightForce = NewLightForce - NewSource.LightDecrease;
            Source = NewSource;
            LightDecrease = NewSource.LightDecrease;
            cell.LightRate = (turnManager.isDay ? DarknessMainVariables.LightForce : 0) + NewLightForce - NewSource.LightDecrease;
            OnLightForceChange?.Invoke(this, NewLightForce - NewSource.LightDecrease);
        }
    }
    public void SetLight(int NewLightRate, int NewDecrease) {
        Source = null;
        LightForce = NewLightRate;
        LightDecrease = NewDecrease;
        cell.LightRate = (turnManager.isDay ? DarknessMainVariables.LightForce : 0) + NewLightRate;
        OnLightForceChange?.Invoke(this, NewLightRate);
    }
}
