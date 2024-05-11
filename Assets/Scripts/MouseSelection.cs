﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

[RequireComponent(typeof(Camera))]
public class MouseSelection : MonoBehaviour {
    [SerializeField] private Material highlightMaterial; // от этого материала берётся только цвет.
    [SerializeField] private Material selectionMaterial; // от этого материала берётся только цвет.

    public Transform highlighted {
        get; private set;
    }
    [NonSerialized] public static Action<Transform> onHighlightChanged; // вызывается раньше изменения переменной и перекраски клетки
    
    public Transform selected {
        get; private set;
    }
    [NonSerialized] public static Action<Transform> onSelectionChanged; // вызывается раньше изменения переменной и перекраски клетки

    [NonSerialized] public static Action<Transform> onSelectionHighlighted; // вызывается при наведении мышкой на выделенную клетку
    [NonSerialized] public static Action<Transform> onSelectionClick; // вызывается при клике на выделенную клетку
    [NonSerialized] public static Action onClickOutside; // вызывается при клике в пустоту

    private Camera _camera;
    private Color _highlightedOldColor, _selectedOldColor;
    private Transform _lastHighlight = null;

    private void Awake() {
        this._camera = this.GetComponent<Camera>();
    }

    private void Update() {
        RaycastHit raycastHit;
        bool hasHit = Physics.Raycast(
            this._camera.ScreenPointToRay(Input.mousePosition),
            out raycastHit,
            Mathf.Infinity,
            1 << 8 // коллайдится только с 8-ым слоем
        );
        this.UpdateHighlight(hasHit, raycastHit.transform);
        this.UpdateSelection(hasHit, raycastHit.transform);
    }
    private void UpdateHighlight(bool hasHit, Transform hit) {
        // возвращаем старый цвет highlight'у из прошлого фрейма
        if (this.highlighted != null) { 
            this.highlighted.GetComponent<MeshRenderer>().material.color = this._highlightedOldColor;
            this.highlighted = null;
        }

        if (hasHit && hit != this.selected && !EventSystem.current.IsPointerOverGameObject()) {
            if (hit != this._lastHighlight) {
                this._lastHighlight = hit;
                MouseSelection.onHighlightChanged?.Invoke(this._lastHighlight);
            }
            // сохраняем старый цвет нового highlight'а и красим его в новый
            this._highlightedOldColor = hit.GetComponent<MeshRenderer>().material.color;
            hit.GetComponent<MeshRenderer>().material.color = this.highlightMaterial.color;

            // меняем переменную
            this.highlighted = hit;
        }
        else if (this._lastHighlight != null) {
            this._lastHighlight = null;
            MouseSelection.onHighlightChanged?.Invoke(this._lastHighlight);
            if (hasHit && hit == this.selected && !EventSystem.current.IsPointerOverGameObject())
                MouseSelection.onSelectionHighlighted?.Invoke(this.selected);
        }
    }

    private void UpdateSelection(bool hasHit, Transform hit) {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) {
            if (this.highlighted != null) {
                
                // возвращаем старый цвет старому selection'у
                if (this.selected != null)
                    this.selected.GetComponent<MeshRenderer>().material.color = this._selectedOldColor;
                
                MouseSelection.onSelectionChanged?.Invoke(hit);

                // сохраняем старый цвет нового selection'а и красим его в новый
                this._selectedOldColor = this._highlightedOldColor;
                hit.GetComponent<MeshRenderer>().material.color = this.selectionMaterial.color;

                // меняем переменную
                this.selected = hit;
                
                // теперь highlight'а не не должно быть
                this._lastHighlight = null;
                MouseSelection.onHighlightChanged?.Invoke(this._lastHighlight);
                this.highlighted = null;
            }
            // мы кликнули в selection или в пустоту
            else if (this.selected != null) {
                // возвращаем старый цвет старому selection'у
                this.selected.GetComponent<MeshRenderer>().material.color = this._selectedOldColor;

                // меняем переменную
                MouseSelection.onSelectionChanged?.Invoke(null);
                if (hasHit) MouseSelection.onSelectionClick?.Invoke(this.selected);
                else MouseSelection.onClickOutside?.Invoke();

                this.selected = null;
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape) && this.selected != null) {
            // возвращаем старый цвет старому selection'у
            this.selected.GetComponent<MeshRenderer>().material.color = this._selectedOldColor;
            
            // меняем переменную
            MouseSelection.onSelectionChanged?.Invoke(null);
            this.selected = null;
        }
    }

    public void SetHighlight(Transform newHighlight) {
        // возвращаем старый цвет старому highlight'у
        if (this.highlighted != null)
            this.highlighted.GetComponent<MeshRenderer>().material.color = this._highlightedOldColor;
        
        if (newHighlight != this._lastHighlight) {
            this._lastHighlight = newHighlight;
            MouseSelection.onHighlightChanged?.Invoke(this._lastHighlight);
        }

        // сохраняем старый цвет нового highlight'а и красим его в новый
        this._highlightedOldColor = newHighlight.GetComponent<MeshRenderer>().material.color;
        newHighlight.GetComponent<MeshRenderer>().material.color = this.highlightMaterial.color;

        // меняем переменную
        this.highlighted = newHighlight;
    }
    
    public void SetSelection(Transform newSelection) {
        // возвращаем старый цвет старому selection'у
        if (this.selected != null)
            this.selected.GetComponent<MeshRenderer>().material.color = this._selectedOldColor;
        
        if (newSelection != this.selected)
            MouseSelection.onSelectionChanged?.Invoke(newSelection);
        
        // сохраняем старый цвет нового selection'а и красим его в новый
        if (newSelection == this.highlighted) {
            this._selectedOldColor = this._highlightedOldColor;
            
            // теперь highlight'а не не должно быть
            this._lastHighlight = null;
            MouseSelection.onHighlightChanged?.Invoke(this._lastHighlight);
            this.highlighted = null;
        }
        else this._selectedOldColor = newSelection.GetComponent<MeshRenderer>().material.color;
        newSelection.GetComponent<MeshRenderer>().material.color = this.selectionMaterial.color;

        // меняем переменную
        this.selected = newSelection;
    }
    
}
