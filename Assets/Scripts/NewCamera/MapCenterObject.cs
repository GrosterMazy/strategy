using UnityEngine;

public class MapCenterObject : ObjectOnGrid
{
    [SerializeField] private HexGrid Grid;
    void Start()
    {
        LocalCoords = new Vector2Int(Grid.size.x / 2, Grid.size.y / 2);
        transform.position = Grid.InUnityCoords(LocalCoords);
    }
}
