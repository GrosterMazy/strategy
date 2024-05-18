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
        foreach (ObjectOnGrid spawnedWorker in spawnedObjects)
        {
        //    spawnedWorker.GetComponent<UnitDescription>().TeamAffiliation =  // Надо реализовать присваивание команды
        }
        Destroy(this);
    }
}
