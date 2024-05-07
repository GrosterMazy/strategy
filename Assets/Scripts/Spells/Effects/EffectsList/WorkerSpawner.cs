public class WorkerSpawner : Spawner
{
    new private void Start()
    {
        base.Start();
        foreach(WorkerUnit worker in FindObjectsOfType<WorkerUnit>())
        {
            if (worker.CompareTag("Standard"))
            {
                objToSpawn = worker;
                break;
            }
        }
        SpawnObject();
        Destroy(this);
    }
}
