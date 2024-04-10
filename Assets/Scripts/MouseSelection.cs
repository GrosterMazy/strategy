﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

[RequireComponent(typeof(Camera))]
public class MouseSelection : MonoBehaviour {
    [SerializeField] private Material highlightMaterial; // от этого материала берётся только цвет.
    [SerializeField] private Material selectionMaterial; // от этого материала берётся только цвет.

    [NonSerialized] public Transform highlighted = null;
    [NonSerialized] public static Action<Transform> onHighlightChanged; // вызывается раньше изменения переменной и перекраски клетки
    
    [NonSerialized] public Transform selected = null;
    [NonSerialized] public static Action<Transform> onSelectionChanged; // вызывается раньше изменения переменной и перекраски клетки

    private Camera _camera => this.GetComponent<Camera>();
    private Color _highlightedOldColor, _selectedOldColor;
    private Transform _lastHighlight = null;

    private void Update() {
        RaycastHit raycastHit;
        bool hasHit = Physics.Raycast(this._camera.ScreenPointToRay(Input.mousePosition), out raycastHit);
        this.UpdateHighlight(hasHit, raycastHit.transform);
        this.UpdateSelection(raycastHit.transform);
    }
    private void UpdateHighlight(bool hasHit, Transform hit) {
        // возвращаем старый цвет highlight'у из прошлого фрейма
        if (this.highlighted != null) { 
            this.highlighted.GetComponent<MeshRenderer>().material.color = this._highlightedOldColor;
            this.highlighted = null;
        }

        if (hasHit && hit.CompareTag("Selectable") && hit != this.selected
                //&& !EventSystem.current.IsPointerOverGameObject()
                ) {
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
        }
    }

    private void UpdateSelection(Transform hit) {
        if (Input.GetMouseButtonDown(0)
                //&& !EventSystem.current.IsPointerOverGameObject()
                ) {
            if (this.highlighted != null) {
                
                // возвращаем старый цвет старому selection'у
                if (this.selected != null)
                    this.selected.GetComponent<MeshRenderer>().material.color = this._selectedOldColor;                

                // меняем переменную
                this.selected = hit;
                MouseSelection.onSelectionChanged?.Invoke(this.selected);

                // сохраняем старый цвет нового selection'а и красим его в новый
                this._selectedOldColor = this._highlightedOldColor;
                this.selected.GetComponent<MeshRenderer>().material.color = this.selectionMaterial.color;
                
                // теперь highlight'а не не должно быть
                this.highlighted = null;
                this._lastHighlight = null;
                MouseSelection.onHighlightChanged?.Invoke(this._lastHighlight);
            }
            // мы кликнули в selection или в пустоту
            else if (this.selected != null) {
                // возвращаем старый цвет старому selection'у
                this.selected.GetComponent<MeshRenderer>().material.color = this._selectedOldColor;

                // меняем переменную
                this.selected = null;
                MouseSelection.onSelectionChanged?.Invoke(this.selected);
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape) && this.selected != null) {
            // возвращаем старый цвет старому selection'у
            this.selected.GetComponent<MeshRenderer>().material.color = this._selectedOldColor;
            
            // меняем переменную
            this.selected = null;
            MouseSelection.onSelectionChanged?.Invoke(this.selected);
        }
    }
}