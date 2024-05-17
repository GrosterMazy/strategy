using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class WorkerSpawner : Spawner
{
    new private void Awake()
    {
        base.Awake();
    }
    new private void Start()
    {
        base.Start();
        objToSpawn = Resources.Load<ObjectOnGrid>("WorkerUnit");
        SpawnObject();
        Destroy(this);
    }
}
