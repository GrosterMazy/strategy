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
    [SerializeField] private float spacing;

    [Header("Terrain Generation")]
    [SerializeField] private Noise baseHeight;
    private float[,] _multiplyersMap; // карта множителей для базовой высоты
    private float _maxMultiplyer = 1;

    [SerializeField] private Noise erosion; // делает пики гор более острыми
    [SerializeField] private float erosionSlopeInfluence;

    [SerializeField] private Noise rigidity;
    [SerializeField] private float rigidityHeightInfluence;

    [SerializeField] private Noise rivers;
    [SerializeField, Range(0f, 1f)] private float riverWidth = 0.1f; // ширина рек (берутся значения вокруг 0.5 из rivers). при riverWidth == 0 реки не генерируются
    [SerializeField] private float riverDepth;
    
    [SerializeField] private Vector2Int numberOfPlates; // кол-во тектонических плит на карте. должно быть меньше size
    [SerializeField] private float mountainHeight;
    [SerializeField] private float mountainSlope; 

    private Vector2Int[,] _plateCenters; // локальные координаты центра плиты для каждого чанка
    private Vector2Int[,] _plateCoords; // локальные координаты центра плиты для каждой локальной координаты
    private List<Vector2Int> _plateborders;
    private bool[,] _mountainExpanded;
    private Vector2Int _chunkStep;

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
        this._plateCoords = new Vector2Int[this.size.x, this.size.y];
        this._plateborders =  new List<Vector2Int>();
        this.pivots = new GameObject[this.size.x, this.size.y];
        this.hexCells = new HexCell[this.size.x, this.size.y];
        this._multiplyersMap = new float[this.size.x, this.size.y];
        this._mountainExpanded = new bool[this.size.x, this.size.y];
        for (int x = 0; x < this.size.x; x++)
            for (int y = 0; y < this.size.y; y++)
                this._multiplyersMap[x, y] = 1;
        
        this._chunkStep = Vector2Int.RoundToInt(((Vector2)this.size)/this.numberOfPlates);
        
        this.erosion.Init();
        this.rivers.Init();
        this.baseHeight.Init();
        this.rigidity.Init();
        this.temperature.Init();
        this.wetness.Init();


        this.GenerateTectonicPlateCenters();
        this.GenerateTectonicPlateBorders();
        this.GenerateMultiplyersMap();
        this.GenerateMap();
        this.PlaceBiomes();
    }

    private void GenerateTectonicPlateCenters() {
        for (int x = 0; x < this.numberOfPlates.x; x++)
            for (int y = 0; y < this.numberOfPlates.y; y++)
                this._plateCenters[x, y] = new Vector2Int(
                    x*this._chunkStep.x + UnityEngine.Random.Range(0, this._chunkStep.x),
                    y*this._chunkStep.y + UnityEngine.Random.Range(0, this._chunkStep.y)
                );
    }

    private void GenerateTectonicPlateBorders() {
        for (int x = 0; x < this.size.x; x++)
            for (int y = 0; y < this.size.y; y++) {
                Vector2Int pos = new Vector2Int(x, y);

                // узнаём, к какой плите мы относимся
                int minDistanceToPlate = 1_000_000; 
                int minDistancePlateX = -1;
                int minDistancePlateY = -1;

                int chunkX = x / this._chunkStep.x;
                int chunkY = y / this._chunkStep.y;
                
                for (int offsetX = -1; offsetX <= 1; offsetX++)
                    for (int offsetY = -1; offsetY <= 1; offsetY++) {
                        if (chunkX+offsetX < 0 || chunkX+offsetX >= this.numberOfPlates.x
                                || chunkY+offsetY < 0 || chunkY+offsetY >= this.numberOfPlates.y)
                            continue;
                        
                        int distance = this.Distance(this._plateCenters[chunkX+offsetX, chunkY+offsetY], pos);

                        if (distance < minDistanceToPlate) {
                            minDistanceToPlate = distance;
                            minDistancePlateX = chunkX+offsetX;
                            minDistancePlateY = chunkY+offsetY;
                        }
                    }
                
                this._plateCoords[x, y] = new Vector2Int(minDistancePlateX, minDistancePlateY);
                
                // отмечаем границы
                if (x > 0 && this._plateCoords[x, y] != this._plateCoords[x-1, y]
                        || y > 0 && this._plateCoords[x, y] != this._plateCoords[x, y-1]) {
                    this._plateborders.Add(pos);
                    this._mountainExpanded[x, y] = true;
                }    
            }

    }

    private void GenerateMultiplyersMap() {

        foreach (Vector2Int pos in this._plateborders) {
            Stack<(Vector2Int, float)> toMultiply = new Stack<(Vector2Int, float)>();

            Vector2Int curpos = pos;
            float curMultiplyer = this.mountainHeight;
            this._maxMultiplyer = Mathf.Max(this._maxMultiplyer, curMultiplyer);

            this._multiplyersMap[curpos.x, curpos.y] += curMultiplyer;

            foreach (Vector2Int newpos in Array.FindAll(this.Neighbours(curpos), newpos => !this._mountainExpanded[newpos.x, newpos.y])) {
                float newMultiplyer = curMultiplyer
                    - this.mountainSlope - this.erosion.ValueAt(newpos.x, newpos.y)*this.erosionSlopeInfluence;
                if (newMultiplyer > 1) {
                    this._maxMultiplyer = Mathf.Max(this._maxMultiplyer, newMultiplyer);
                    this._mountainExpanded[newpos.x, newpos.y] = true;
                    toMultiply.Push((newpos, newMultiplyer));   
                }
            }

            int c = 0;
            while (toMultiply.Count > 0 && c < 10_000) {
                (curpos, curMultiplyer) = toMultiply.Pop();

                this._multiplyersMap[curpos.x, curpos.y] += curMultiplyer;

                foreach (Vector2Int newpos in Array.FindAll(this.Neighbours(curpos), newpos => !this._mountainExpanded[newpos.x, newpos.y])) {
                    float newMultiplyer = curMultiplyer
                        - this.mountainSlope - this.erosion.ValueAt(newpos.x, newpos.y)*this.erosionSlopeInfluence;
                    if (newMultiplyer > 1) {
                        this._maxMultiplyer = Mathf.Max(this._maxMultiplyer, newMultiplyer);
                        this._mountainExpanded[newpos.x, newpos.y] = true;
                        toMultiply.Push((newpos, newMultiplyer));
                    }
                }
                c++;
            }
            if (c == 10_000) Debug.Log("FUC");
        }

    }

    private void GenerateMap() {
        for (int x = 0; x < this.size.x; x++) {
            for (int y = 0; y < this.size.y; y++) {
                Vector2Int pos = new Vector2Int(x, y);

                float baseHeight = this.baseHeight.ValueAt(x, y);
                float rigidity = this.rigidity.ValueAt(x, y);

                baseHeight /= this._maxMultiplyer;
                baseHeight *= this._multiplyersMap[x, y];

                baseHeight *= rigidity*this.rigidityHeightInfluence;

                // игнорируем клетки ниже уровня воды
                if (baseHeight < this.waterLevel) continue;

                // создаём клетку
                int height = Mathf.RoundToInt(Mathf.Lerp(0, this.numberOfHeights-1, baseHeight));

                GameObject pivot = Instantiate(
                    this.hexagonPrefab,
                    this.InUnityCoords(pos, (this.showHeightOnCells ? height : 0)),
                    this.hexagonPrefab.transform.rotation,
                    this.transform
                );
                HexCell hexCell = pivot.transform.GetChild(0).GetComponent<HexCell>();

                // покраска границ плит
                // if (border) hexCell.GetComponent<Renderer>().material.color = new Vector4(140/256f, 109/256f, 95/256f, 0f);

                // устанавливаем переменные LightTransporter'a (для оптимизации)
                LightTransporter lightTransporter = hexCell.GetComponent<LightTransporter>();
                lightTransporter.grid = this;
                lightTransporter.turnManager = this._turnManager;
                lightTransporter.cell = hexCell;                

                // раширяем клету и двигаем её центр (можно убрать эту часть и клетки будут просто подниматься по оси у)
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

                // устанавливаем переменные клетки
                hexCell.lightTransporter = lightTransporter;
                hexCell.height = height;
                
                // добавляем клетку на карту
                this.pivots[x, y] = pivot;
                this.hexCells[x, y] = hexCell;
            }
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
            this.InBoundsOfMap(newpos)
            // && this.pivots[newpos.x, newpos.y] != null
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
