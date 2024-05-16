﻿using System.Collections;
using UnityEngine.Events;
using UnityEngine;

public class Button3D : MonoBehaviour
{
    [SerializeField] private UnityEvent OnClick;
    [SerializeField] private Vector3 MaxPosition;
    [SerializeField] private Vector3 MinPosition;

    [SerializeField] private float AnimationSpeedUp=1;
    [SerializeField] private float AnimationSpeedDown=1;

    private bool _IsMouseOver=false;
    private HexGrid _grid;
    private Vector3 _cell;

    private void Awake() { _grid = FindObjectOfType<HexGrid>(); }
    private void Start()
    {
        //  Destroy(_grid.hexCells[_grid.InLocalCoords(transform.position).x, _grid.InLocalCoords(transform.position).y].gameObject);
        _cell = _grid.hexCells[_grid.InLocalCoords(transform.position).x, _grid.InLocalCoords(transform.position).y].transform.position;
        transform.position= new Vector3(_cell.x,MinPosition.y,_cell.z);
    }
    private void OnMouseDown()
    {
        OnClick.Invoke();
    }
    private void OnMouseOver()
    {
        if(transform.position.y<= MaxPosition.y) transform.position += -Vector3.down * Time.fixedDeltaTime * AnimationSpeedUp;
    }

    private void OnMouseEnter() { _IsMouseOver = true; }
    private void OnMouseExit() { _IsMouseOver = false; }
    
    private void FixedUpdate()
    {
        if (transform.position.y >= MinPosition.y && _IsMouseOver==false) transform.position += Vector3.down * Time.fixedDeltaTime * AnimationSpeedDown;
    }
}