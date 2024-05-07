
public class Spawner : EffectsDescription
{
    protected ObjectOnGrid objToSpawn; //
    protected int objLifeTime;         // Указать в наследнике
    protected int objTeamAffiliation;  //

    protected HexCell hexCell;
    protected PlacementManager placementManager;
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
//        if (placementManager.gridWithObjectsInformation[hexCell.LocalCoords.x, hexCell.LocalCoords.y] == null)
    }

    private void InitComponentLinks()
    {
        placementManager = FindObjectOfType<PlacementManager>();
    }
}
