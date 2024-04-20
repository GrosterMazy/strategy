using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class LightTransporter : MonoBehaviour
{
    private int _oldLightForce;
    private int _oldLightDecrease;
    public int LightForce;
    public int LightDecrease;
    private HexCell _cell;
    public LightTransporter Source;
    private HexGrid _grid;
    private TurnManager _turnManager;
    [NonSerialized] public Action<LightTransporter, int> OnLightForceChange;
    private void Awake()
    {
        _cell = GetComponent<HexCell>();
        _grid = FindObjectOfType<HexGrid>();
        _turnManager = FindObjectOfType<TurnManager>();
    }

    private void Start()
    {
        foreach(Vector2Int Coords in _grid.Neighbours(transform.position))
        {
            _grid.childs[Coords.x, Coords.y].GetComponent<LightTransporter>().OnLightForceChange += OnLightSourceChanged;
        }
    }
    private void OnDestroy()
    {
        foreach (Vector2Int Coords in _grid.Neighbours(transform.position))
        {
            _grid.childs[Coords.x, Coords.y].GetComponent<LightTransporter>().OnLightForceChange -= OnLightSourceChanged;
        }
    }
    private void OnLightSourceChanged(LightTransporter NewSource,int NewLightForce)
    {
       
        if (NewSource == Source)
        {
            if (_turnManager.isDay && NewLightForce - NewSource.LightDecrease <= DarknessMainVariables.LightForce || !_turnManager.isDay && NewLightForce - NewSource.LightDecrease <= 0)
            {
                SetLight(_oldLightForce,_oldLightDecrease);
                return;
            }
            LightForce = NewLightForce - NewSource.LightDecrease;
            LightDecrease = NewSource.LightDecrease;
            _cell.LightRate = (_turnManager.isDay ? DarknessMainVariables.LightForce : 0) + NewLightForce-NewSource.LightDecrease;
            OnLightForceChange?.Invoke(this, NewLightForce-NewSource.LightDecrease);
            return;
        }
        if (_turnManager.isDay && NewLightForce - NewSource.LightDecrease <= DarknessMainVariables.LightForce || !_turnManager.isDay && NewLightForce - NewSource.LightDecrease <= 0)
            return;
        if (NewLightForce - NewSource.LightDecrease > LightForce)
        {
            if(Source==null)
            {
                _oldLightForce = LightForce;
                _oldLightDecrease = LightDecrease;
            }
            LightForce = NewLightForce - NewSource.LightDecrease;
            Source = NewSource;
            LightDecrease = NewSource.LightDecrease;
            _cell.LightRate= (_turnManager.isDay ? DarknessMainVariables.LightForce : 0) + NewLightForce - NewSource.LightDecrease;
            OnLightForceChange?.Invoke(this, NewLightForce - NewSource.LightDecrease);
        }
    }
    public void SetLight(int NewLightRate,int NewDecrease)
    {
        Source = null;
        LightForce = NewLightRate;
        LightDecrease = NewDecrease;
        _cell.LightRate =( _turnManager.isDay ? DarknessMainVariables.LightForce : 0)+NewLightRate;
        OnLightForceChange?.Invoke(this, NewLightRate);
    }
}
