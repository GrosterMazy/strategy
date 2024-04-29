using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HexGrid : MonoBehaviour {
    [Header("Base")]
    [SerializeField] private GameObject hexagonPrefab;
    private Renderer _hexagonPrefabRenderer;

    public Vector2Int size; // размер в клетках
    /* Например поле 5(по x) на 4(по z) будет выглядеть так:
    ^ z
    |
    .-> x

    * * * *
     * b c *
    * g a d
     * f e *
    * * * *
    */

    [NonSerialized] public GameObject[,] pivots;
    [NonSerialized] public HexCell[,] hexCells;
    /*
    Координаты построены как будто у нас квадратное поле, тоесть на нашем примере так:
    * * * *
    * b c *
    * g a d
    * f e *
    * * * *
    */
    [SerializeField] private int spacing;

    [Header("Terrain Generation")]
    [SerializeField] private Noise baseHeight;
    [SerializeField] private Noise erosion; // делает пики гор более острыми
    [SerializeField] private Noise rivers;
    [SerializeField, Range(0f, 1f)] private float riverWidth = 0.1f; // ширина рек (берутся значения вокруг 0.5 из rivers). при riverWidth == 0 реки не генерируются
    
    [SerializeField] private Vector2Int numberOfPlates; // кол-во тектонических плит на карте. должно быть меньше size
    private Vector2Int[,] _plateCenters;
    private bool[,] _plateBorders;


    [SerializeField, Range(0f, 1f)] private float waterLevel = 0.3f; // высота от 0(минимальная) до 1(максимальная), ниже которой перестают создаваться клетки

    [Header("Height Mapping")]
    [SerializeField] private int numberOfHeights = 2; // кол-во ступеней высот. минимальная высота 0, максимальная numberOfHeights-1
    [SerializeField] private bool showHeightOnCells = false; // раставлять клетки в соответствии с их высотой
    [SerializeField] private bool linearHeightStep = true; // true - клетки разница в высоте между клетками с соседними значениями height равна heightStep, false - берётся из heights
    [SerializeField] private float heightStep;
    [SerializeField] private float[] heights;

    [Header("Biome Placement")]
    [SerializeField] private Noise temperature;
    [SerializeField] private Noise wetness;

    private TurnManager _turnManager;

    private void Awake() {
        this._hexagonPrefabRenderer = this.hexagonPrefab.transform.GetChild(0).GetComponent<Renderer>();
        this._turnManager = FindObjectOfType<TurnManager>();

        this._plateCenters = new Vector2Int[this.numberOfPlates.x, this.numberOfPlates.y];
        this._plateBorders = new bool[this.size.x, this.size.y];

        this.GenerateTectonicPlates();

        this.pivots = new GameObject[this.size.x, this.size.y];
        this.hexCells = new HexCell[this.size.x, this.size.y];

        this.baseHeight.Init();
        this.erosion.Init();
        this.rivers.Init();

        this.GenerateMap();

        this.temperature.Init();
        this.wetness.Init();

        this.PlaceBiomes();
    }

    private void GenerateTectonicPlates() {
        Vector2Int step = Vector2Int.RoundToInt(((Vector2)this.size)/this.numberOfPlates);
        // for (int x = 0; x < this.numberOfPlates.x; x++)
        //     for (int baseY = 0; baseY < this.size.y; baseY += step.y) {
                
        //     }
    }

    private void GenerateMap() {
        for (int x = 0; x < this.size.x; x++)
            for (int y = 0; y < this.size.y; y++) {
                float baseHeight = this.baseHeight.ValueAt(x, y);

                if (baseHeight < this.waterLevel) continue;

                int height = Mathf.RoundToInt(Mathf.Lerp(0, this.numberOfHeights-1, baseHeight));

                GameObject pivot = Instantiate(
                    this.hexagonPrefab,
                    this.InUnityCoords(
                        new Vector2Int(x, y),
                        (this.showHeightOnCells ? height : 0)
                    ),
                    this.hexagonPrefab.transform.rotation,
                    this.transform
                );
                HexCell hexCell = pivot.transform.GetChild(0).GetComponent<HexCell>();

                // set variables of LightTransporter(for optimization)
                LightTransporter lightTransporter = hexCell.GetComponent<LightTransporter>();
                lightTransporter.grid = this;
                lightTransporter.turnManager = this._turnManager;
                lightTransporter.cell = hexCell;                

                // scale hexCell and move it's center
                hexCell.transform.localScale = new Vector3(
                    hexCell.transform.localScale.x,
                    hexCell.transform.localScale.y,
                    hexCell.transform.localScale.z
                        + hexCell.transform.position.y / (this._hexagonPrefabRenderer.bounds.size.y / hexCell.transform.localScale.z)
                );
                hexCell.transform.localPosition = new Vector3(
                    hexCell.transform.localPosition.x,
                    -hexCell.transform.position.y/2,
                    hexCell.transform.localPosition.z
                );

                // set variables of hexCell
                hexCell.lightTransporter = lightTransporter;
                hexCell.height = height;
                
                // set maps
                this.pivots[x, y] = pivot;
                this.hexCells[x, y] = hexCell;
            }
    }

    private void PlaceBiomes() {}

    public bool InBoundsOfMap(Vector2Int pos) => pos.x >= 0 && pos.x < this.size.x && pos.y >= 0 && pos.y < this.size.y;

    public Vector3 InUnityCoords(Vector2Int pos, int height = -1/*брать "y" у pivot'a клетки*/) {
        if (!this.InBoundsOfMap(pos))
            throw new System.Exception("локальные координаты "+pos.ToString()+" за пределами поля.");
        
        if (height == -1) {
            if (this.pivots[pos.x, pos.y] == null)
                throw new System.Exception("нету клетки в локальных координатах "+pos.ToString()+", а значит невозможно взять её высоту.");
            return this.pivots[pos.x, pos.y].transform.position;
        }

        return new Vector3(
            pos.x * (this._hexagonPrefabRenderer.bounds.size.x + this.spacing)
                + ((pos.y % 2 == 0) ? 0 : ((this._hexagonPrefabRenderer.bounds.size.x + this.spacing) / 2)),
            (this.showHeightOnCells ?
                (this.linearHeightStep ? height*this.heightStep : this.heights[height])
                : 0
            ),
            pos.y * (this._hexagonPrefabRenderer.bounds.size.x + this.spacing) * Mathf.Cos(Mathf.PI/6)
        );
    }

    public Vector2Int InLocalCoords(Vector3 position) {// игнорирует координату "y"
        int posY = Mathf.RoundToInt(
            position.z
                / (this._hexagonPrefabRenderer.bounds.size.x + this.spacing) / Mathf.Cos(Mathf.PI/6)
        );
        Vector2Int pos = new Vector2Int(
            Mathf.RoundToInt(
                (position.x - ((posY % 2 == 0) ? 0 : ((this._hexagonPrefabRenderer.bounds.size.x + this.spacing) / 2)))
                    / (this._hexagonPrefabRenderer.bounds.size.x + this.spacing)
            ),
            posY
        );
        if (!this.InBoundsOfMap(pos))
            throw new System.Exception("координаты "+position.ToString()+" за пределами поля.");
        return pos;
    }

    public Vector2Int[] Neighbours(Vector2Int pos, int maxHeightDifference = -1/*any height difference*/, bool restrictDownMovement = false) {
        Vector2Int[] potentialMoves = new Vector2Int[6] {
            new Vector2Int(pos.x+1, pos.y),
            new Vector2Int(pos.x-1, pos.y),
            new Vector2Int(pos.x, pos.y+1),
            new Vector2Int(pos.x, pos.y-1),
            new Vector2Int(((pos.y % 2 == 0) ? pos.x-1 : pos.x+1), pos.y+1),
            new Vector2Int(((pos.y % 2 == 0) ? pos.x-1 : pos.x+1), pos.y-1)
        };
        return Array.FindAll(potentialMoves, newpos =>
            newpos.x >= 0 && newpos.x < this.size.x && newpos.y >= 0 && newpos.y < this.size.y 
            && this.pivots[newpos.x, newpos.y] != null
            && (maxHeightDifference == -1 ?
                true
                : (restrictDownMovement ?
                    Mathf.Abs(this.hexCells[newpos.x, newpos.y].height - this.hexCells[pos.x, pos.y].height)
                    : this.hexCells[newpos.x, newpos.y].height - this.hexCells[pos.x, pos.y].height
                ) <= maxHeightDifference
            )
        );
    }
    public Vector2Int[] Neighbours(Vector3 position, int maxHeightDifference = -1/*any height difference*/, bool restrictDownMovement = false) =>
        this.Neighbours(this.InLocalCoords(position), maxHeightDifference, restrictDownMovement);

    public int Distance(Vector2Int pos1, Vector2Int pos2) {
        Vector2Int start = pos1;
        Vector2Int end = pos2;
        int dist = 0;

        while (start != end) {
            Vector2Int delta = end - start;

            if (start.y % 2 == 0) {
                if (delta.y != 0) {
                    if (delta.y > 0) start.y++;
                    else start.y--;

                    if (delta.x < 0) start.x--;// skip
                    dist++;
                    continue;
                }
                //just go by x
                return dist + Mathf.Abs(delta.x);
            }

            if (delta.y != 0) {
                if (delta.y > 0) start.y++;
                else start.y--;

                if (delta.x > 0) start.x++;// skip
                dist++;
                continue;
            }
            //just go by x
            return dist + Mathf.Abs(delta.x);
        }
            
        return dist;
    }
    public int Distance(Vector3 position1, Vector3 position2) => this.Distance(this.InLocalCoords(position1), this.InLocalCoords(position2));
}
