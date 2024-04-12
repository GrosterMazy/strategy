﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableItem : MonoBehaviour
{
    public int NumberOfItems;
    public float WeightOfOneItem;
    public string Name;
    public ObjectOnGrid _coordsOnGrid => GetComponent<ObjectOnGrid>();
    private PlacementManager _placementManager => FindObjectOfType<PlacementManager>();

    public void GoneAway() { _placementManager.UpdateGrid(_coordsOnGrid.LocalCoords, _coordsOnGrid.LocalCoords, _coordsOnGrid); } // Возвращение collectableitem'a в список всех предметов на сетке

    public void Taken(int _amount) { 
        NumberOfItems -= _amount;
        if (NumberOfItems <= 0) Destroy(gameObject); }

    private void Start() { if (NumberOfItems <= 0 || WeightOfOneItem <= 0 || Name == null) throw new System.Exception("Указаны недопустимые данные у подбираемого предмета"); }
}