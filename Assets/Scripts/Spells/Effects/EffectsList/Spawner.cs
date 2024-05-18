using System;
using System.Collections.Generic;
using UnityEngine;
public class Spawner : EffectsDescription
{
    protected ObjectOnGrid objToSpawn;          // Пока не работает
    protected int objLifeTime;                  // Указать в наследнике

    protected List<ObjectOnGrid> spawnedObjects = new List<ObjectOnGrid>();
    protected HexCell hexCell;
    protected PlacementManager placementManager;
    protected void Awake()
    {
        InitComponentLinks();
    }
    protected void Start()
    {
        if (TryGetComponent<HexCell>(out HexCell _hexCell))
        {
            hexCell = _hexCell;
        }
        else
        {
            Destroy(this);
        }
        remainingLifeTime = 1;
        isNegative = false;
    }

    protected void SpawnObject()
    {
        spawnedObjects.Clear();
        if (placementManager.gridWithObjectsInformation[hexCell.localPos.x, hexCell.localPos.y] == null)
        {
            ObjectOnGrid spawnedObj = Instantiate<ObjectOnGrid>(objToSpawn, transform.parent.position, Quaternion.identity);
            placementManager.UpdateGrid(hexCell.localPos, hexCell.localPos, spawnedObj);
            spawnedObj.LocalCoords = hexCell.localPos;
            spawnedObjects.Add(spawnedObj);
        }
    }

    private void InitComponentLinks()
    {
        placementManager = FindObjectOfType<PlacementManager>();
    }
}
