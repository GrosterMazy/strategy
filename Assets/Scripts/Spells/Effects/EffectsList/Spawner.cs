using System;
using UnityEngine;
public class Spawner : EffectsDescription
{
    protected ObjectOnGrid objToSpawn;          //
    protected int objLifeTime;                  // Указать в наследнике
    protected int objTeamAffiliation;           //

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
        if (placementManager.gridWithObjectsInformation[hexCell.localPos.x, hexCell.localPos.y] == null)
        {
            placementManager.gridWithObjectsInformation[hexCell.localPos.x, hexCell.localPos.y] = objToSpawn;
            ObjectOnGrid spawnedObj = Instantiate<ObjectOnGrid>(objToSpawn, transform.parent.position, Quaternion.identity);
            spawnedObj.LocalCoords = hexCell.localPos;
        }
    }

    private void InitComponentLinks()
    {
        placementManager = FindObjectOfType<PlacementManager>();
    }
}
