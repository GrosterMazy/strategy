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
    [NonSerialized] public Renderer[,] hexCellRenderers;
    /*
    Координаты построены как будто у нас квадратное поле, тоесть на нашем примере так:
    * * * *
    * b c *
    * g a d
    * f e *
    * * * *
    */
    [SerializeField] private float spacing;

    [Header("Base/Height Mapping")]
    [SerializeField] private float heightMappingDownshift; // сдвиг всех значений вниз
    [SerializeField] private int numberOfHeights = 2; // кол-во ступеней высот. минимальная высота 0, максимальная numberOfHeights-1
    [SerializeField] private bool showHeightOnCells = false; // раставлять клетки в соответствии с их высотой
    [SerializeField] private bool linearHeightStep = true; // true - клетки разница в высоте между клетками с соседними значениями height равна heightStep, false - берётся из heights
    [SerializeField] private float heightStep;
    [SerializeField] private float[] heights;

    [Header("HeightMap/Base Terrain")]
    [SerializeField] private float overallBaseTerrainHeightInfluence;

    [SerializeField] private Noise baseTerrain; // равнины
    [SerializeField] private float baseTerrainHeightInfluence;

    [SerializeField] private Noise baseTerrainRigidity; // твёрдость почв равнины
    [SerializeField] private float baseTerrainRigidityHeightInfluence;

    [Header("HeightMap/Water")]
    [SerializeField] private Color waterCoastColor;
    [SerializeField, Range(0f, 1f)] private float waterCoastLevel = 0.2f; // высота от 0(минимальная) до 1(максимальная), ниже которой начинает создаваться вода
    
    [SerializeField] private Color waterColor;
    [SerializeField, Range(0f, 1f)] private float waterLevel;

    [SerializeField] private Color deepWaterColor;
    [SerializeField, Range(0f, 1f)] private float deepWaterLevel;

    // [SerializeField] private Noise rivers;
    // [SerializeField, Range(0f, 1f)] private float riverLevel; // высота от 0(минимальная) до 1(максимальная), ниже которой начинают создаваться реки
    // [SerializeField, Range(0f, 1f)] private float riverWidth = 0.1f; // ширина рек (берутся значения вокруг 0.5 из rivers). при riverWidth == 0 реки не генерируются

    [Header("HeightMap/Mountains")]
    [SerializeField] private float mountainsHeightInfluence;

    [SerializeField] private Vector2Int numberOfTectonicPlates; // должно быть меньше size. горы генерируются на стыках тектонических плит
    
    private Vector2Int _tectonicChunkStep;
    private Vector2Int[,] _tectonicPlateCenters; // локальные координаты центра плиты для каждого чанка
    private Vector2Int[,] _tectonicPlateCoords; // локальные координаты центра плиты для каждой локальной координаты

    private float[,] _mountainsMap;
    [SerializeField] private float mountainSlope; // изначальный склон гор

    [SerializeField] private bool regulateMountainsHeight = true;
    [SerializeField] private Noise mountainsHeightMultiplyer; // регулирует высоту гор

    [SerializeField] private Noise mountainsErosion; // делает пики гор более острыми
    [SerializeField] private float mountainsErosionSlopeInfluence;

    // [SerializeField] private Color mountainsColor; // не используется
    [SerializeField, Range(0f, 1f)] private float mountainsLevel; // выше этого значения клетки будут краситься как горы 

    [Header("HeightMap/Highlands")]
    [SerializeField] private float highlandsHeightInfluence;

    [SerializeField] private Vector2Int numberOfHighlandsPlates; // должно быть меньше size
    [SerializeField, Range(0f, 1f)] private float highlandsAmount; // насколько много плит станут плоскогорьями

    private Vector2Int _highlandsChunkStep;
    private Vector2Int[,] _highlandsPlateCenters; // локальные координаты центра плиты для каждого чанка
    private Vector2Int[,] _highlandsPlateCoords; // локальные координаты чанка плиты для каждой локальной координаты
    private bool[,] _highlandsPlateCentersChoosen; // true = плита в этом чанке станет плоскогорьем

    private float[,] _highlandsMap;

    [SerializeField] private bool regulateHighlandsHeight = true;
    [SerializeField] private Noise highlandsHeightMultiplyer; // регулирует высоту плоскогорий

    [SerializeField] private Noise highlandsErosion; // делает края плоскогорий более резкими
    [SerializeField] private float highlandsErosionSlopeInfluence;

    [Header("HeightMap/Overall Rigidity")]
    [SerializeField] private Noise rigidity;
    [SerializeField] private float rigidityHeightInfluence;

    [Header("Biome Placement")]
    [SerializeField] private Noise temperature;
    [SerializeField, Range(0f, 1f)] private float veryHotLevel; // спавнятся только пустыни
    [SerializeField, Range(0f, 1f)] private float hotLevel; // cпавнятся тёплые биомы
    [SerializeField, Range(0f, 1f)] private float normalTemperatureLevel; // спавнятся обычные биомы
    [SerializeField, Range(0f, 1f)] private float coldLevel; // спавнятся холодные биомы
    [SerializeField, Range(0f, 1f)] private float veryColdLevel; // спавнятся только вечные снега

    [SerializeField] private Noise wetness;
    // [SerializeField, Range(0f, 1f)] private float veryWetLevel; // сейчас не используется
    [SerializeField, Range(0f, 1f)] private float wetLevel;
    [SerializeField, Range(0f, 1f)] private float normalWetnessLevel;
    [SerializeField, Range(0f, 1f)] private float dryLevel;
    // [SerializeField, Range(0f, 1f)] private float veryDryLevel; // сейчас не используется

    [SerializeField] private ToughResources treePrefab;
    [SerializeField] private CollectableItem woodPrefab;

    [SerializeField] private ToughResources orePrefab; // появляются на возвышенностях
    [SerializeField, Range(0f, 1f)] private float oreLevel; // уровень, выше которого начинает появляться руда
    [SerializeField, Range(0f, 1f)] private float oreFrequency;

    [SerializeField] private Color beachColor;
    [SerializeField, Range(0f, 1f)] private float beachLevel = 0.27f;

    // [SerializeField] private Color desertColor; // не используется

    // [SerializeField] private Color jungleColor; // не используется
    [SerializeField, Range(0f, 1f)] private float jungleTreeFrequency = 0.9f;
    [SerializeField, Range(0f, 1f)] private float jungleWoodFrequency = 0.01f;
    [SerializeField] private int jungleMinWoodAmount = 1;
    [SerializeField] private int jungleMaxWoodAmount = 5;

    // [SerializeField] private Color steppeColor; // не используется
    [SerializeField, Range(0f, 1f)] private float steppeWoodFrequency = 0.01f;
    [SerializeField] private int steppeMinWoodAmount = 1;
    [SerializeField] private int steppeMaxWoodAmount = 5;

    // [SerializeField] private Color plainsColor; // не используется
    [SerializeField, Range(0f, 1f)] private float plainsTreeFrequency = 0.01f;
    [SerializeField, Range(0f, 1f)] private float plainsWoodFrequency = 0.01f;
    [SerializeField] private int plainsMinWoodAmount = 1;
    [SerializeField] private int plainsMaxWoodAmount = 5;

    // [SerializeField] private Color forestColor; // не используется
    [SerializeField, Range(0f, 1f)] private float forestTreeFrequency = 0.9f;
    [SerializeField, Range(0f, 1f)] private float forestWoodFrequency = 0.01f;
    [SerializeField] private int forestMinWoodAmount = 1;
    [SerializeField] private int forestMaxWoodAmount = 5;

    // [SerializeField] private Color taigaColor; // не используется
    [SerializeField, Range(0f, 1f)] private float taigaTreeFrequency = 1f;
    [SerializeField, Range(0f, 1f)] private float taigaWoodFrequency = 0f;
    [SerializeField] private int taigaMinWoodAmount = 1;
    [SerializeField] private int taigaMaxWoodAmount = 5;

    // [SerializeField] private Color tundraColor; // не используется
    [SerializeField, Range(0f, 1f)] private float tundraWoodFrequency = 0.05f;
    [SerializeField] private int tundraMinWoodAmount = 1;
    [SerializeField] private int tundraMaxWoodAmount = 5;

    // [SerializeField] private Color eternalSnowColor; // не используется

    private Material[] _plainsMats;
    private Material[] _forestMats;
    private Material[] _taigaMats;
    private Material[] _tundraMats;
    private Material[] _jungleMats;
    private Material[] _mountainsMats;
    private Material[] _desertMats;
    private Material[] _snowMats;
    private Material[] _steppeMats;

    private TurnManager _turnManager;
    private PlacementManager _placementManager;
    public Action OnGenerationEnded;
    private void Awake() {
        // Debug.Log("HexGrid awake started");
        //подгрузка материалов
        _plainsMats = Resources.LoadAll<Material>("Materials/Nature/Plains");
        _forestMats = Resources.LoadAll<Material>("Materials/Nature/Forest");
        _taigaMats = Resources.LoadAll<Material>("Materials/Nature/Taiga");
        _tundraMats = Resources.LoadAll<Material>("Materials/Nature/Tundra");
        _jungleMats = Resources.LoadAll<Material>("Materials/Nature/Jungle");
        _desertMats = Resources.LoadAll<Material>("Materials/Nature/Desert");
        _mountainsMats = Resources.LoadAll<Material>("Materials/Nature/Mountains");
        _steppeMats = Resources.LoadAll<Material>("Materials/Nature/Steppe");
        _snowMats= Resources.LoadAll<Material>("Materials/Nature/Snow");

        // Base
        this._turnManager = FindObjectOfType<TurnManager>();
        this._placementManager = FindObjectOfType<PlacementManager>();
        this._hexagonPrefabRenderer = this.hexagonPrefab.transform.GetChild(0).GetComponent<Renderer>();
        this.pivots = new GameObject[this.size.x, this.size.y];
        this.hexCells = new HexCell[this.size.x, this.size.y];
        this.hexCellRenderers = new Renderer[this.size.x, this.size.y];

        // HeightMap/Base Terrain
        this.baseTerrain.Init();
        this.baseTerrainRigidity.Init();

        // HeightMap/Water
        // this.rivers.Init();

        // HeightMap/Mountains
        this._tectonicChunkStep = Vector2Int.RoundToInt(((Vector2)this.size)/this.numberOfTectonicPlates);
        this._tectonicPlateCenters = new Vector2Int[this.numberOfTectonicPlates.x, this.numberOfTectonicPlates.y];
        this._tectonicPlateCoords = new Vector2Int[this.size.x, this.size.y];

        this._mountainsMap = new float[this.size.x, this.size.y];

        this.mountainsHeightMultiplyer.Init();
        this.mountainsErosion.Init();

        // HeightMap/Highlands
        this._highlandsChunkStep = Vector2Int.RoundToInt(((Vector2)this.size)/this.numberOfHighlandsPlates);
        this._highlandsPlateCenters = new Vector2Int[this.numberOfHighlandsPlates.x, this.numberOfHighlandsPlates.y];
        this._highlandsPlateCoords = new Vector2Int[this.size.x, this.size.y];
        this._highlandsPlateCentersChoosen = new bool[this.numberOfHighlandsPlates.x, this.numberOfHighlandsPlates.y];

        this._highlandsMap = new float[this.size.x, this.size.y];

        this.highlandsHeightMultiplyer.Init();
        this.highlandsErosion.Init();

        // HeightMap/Overall Rigidity
        this.rigidity.Init();

        // Biome Placement
        this.temperature.Init();
        this.wetness.Init();
        
        this.GenerateMountainsMap();
        this.GenerateHighlandsMap();
        this.GenerateMap();
        this.OnGenerationEnded?.Invoke();
        // Vector2Int pos, zero = new Vector2Int(0, 0);
        // for (int x = 0; x < this.size.x; x++)
        //     for (int y = 0; y < this.size.y; y++) {
        //         pos = new Vector2Int(x, y);
        //         this.UpdateHeight(pos, 0);
        //     }

        // Debug.Log("HexGrid awake ended");
    }

    private void GenerateMountainsMap() {
        // генерируем центры тектонических плит
        for (int x = 0; x < this.numberOfTectonicPlates.x; x++)
            for (int y = 0; y < this.numberOfTectonicPlates.y; y++)
                this._tectonicPlateCenters[x, y] = new Vector2Int(
                    x*this._tectonicChunkStep.x + UnityEngine.Random.Range(0, this._tectonicChunkStep.x),
                    y*this._tectonicChunkStep.y + UnityEngine.Random.Range(0, this._tectonicChunkStep.y)
                );

        // отмечаем горы
        Vector2Int pos, curpos;
        int minDistanceToPlate, minDistancePlateX, minDistancePlateY, chunkX, chunkY, distance, curdist;
        float curval, erosion;
        Stack<(Vector2Int, int)> toProcess;

        for (int x = 0; x < this.size.x; x++)
            for (int y = 0; y < this.size.y; y++) {
                pos = new Vector2Int(x, y);

                // узнаём, к какой плите мы относимся
                minDistanceToPlate = 1_000_000; 
                minDistancePlateX = -1;
                minDistancePlateY = -1;

                chunkX = x / this._tectonicChunkStep.x;
                chunkY = y / this._tectonicChunkStep.y;
                
                for (int offsetX = -1; offsetX <= 1; offsetX++)
                    for (int offsetY = -1; offsetY <= 1; offsetY++) {
                        if (chunkX+offsetX < 0 || chunkX+offsetX >= this.numberOfTectonicPlates.x
                                || chunkY+offsetY < 0 || chunkY+offsetY >= this.numberOfTectonicPlates.y)
                            continue;
                        
                        distance = this.Distance(this._tectonicPlateCenters[chunkX+offsetX, chunkY+offsetY], pos);

                        if (distance < minDistanceToPlate) {
                            minDistanceToPlate = distance;
                            minDistancePlateX = chunkX+offsetX;
                            minDistancePlateY = chunkY+offsetY;
                        }
                    }
                
                this._tectonicPlateCoords[x, y] = new Vector2Int(minDistancePlateX, minDistancePlateY);
                
                // ставим горы на границах
                if (x > 0 && this._tectonicPlateCoords[x, y] != this._tectonicPlateCoords[x-1, y]
                        || y > 0 && this._tectonicPlateCoords[x, y] != this._tectonicPlateCoords[x, y-1]) {
                    toProcess = new Stack<(Vector2Int, int)>();
                    curpos = pos;
                    curdist = 0;

                    erosion = this.mountainsErosion.ValueAt(curpos.x, curpos.y)*this.mountainsErosionSlopeInfluence;

                    curval = 1f / ((this.mountainSlope + erosion)*curdist + 1); // функция, которая отвечает за форму гор

                    if (this._mountainsMap[curpos.x, curpos.y] < curval) {
                        this._mountainsMap[curpos.x, curpos.y] = curval;
                        foreach (Vector2Int newpos in this.Neighbours(curpos, ignoreEmptyCells: false))
                            toProcess.Push((newpos, curdist+1));
                    }

                    while (toProcess.Count > 0) {
                        (curpos, curdist) = toProcess.Pop();

                        erosion = this.mountainsErosion.ValueAt(curpos.x, curpos.y)*this.mountainsErosionSlopeInfluence;

                        curval = 1f / ((this.mountainSlope + erosion)*curdist + 1); // функция, которая отвечает за форму гор

                        if (curval > this._mountainsMap[curpos.x, curpos.y]+0.25) { // - достаточно маленькое число, чтобы не генерировать горы на всю карту
                            this._mountainsMap[curpos.x, curpos.y] = curval;
                            foreach (Vector2Int newpos in this.Neighbours(curpos, ignoreEmptyCells: false))
                                toProcess.Push((newpos, curdist+1));
                        }
                    }
                }
            }
    }

    private void GenerateHighlandsMap() {
        // генерируем центры плит
        for (int x = 0; x < this.numberOfHighlandsPlates.x; x++)
            for (int y = 0; y < this.numberOfHighlandsPlates.y; y++) {
                this._highlandsPlateCenters[x, y] = new Vector2Int(
                        x*this._highlandsChunkStep.x + UnityEngine.Random.Range(0, this._highlandsChunkStep.x),
                        y*this._highlandsChunkStep.y + UnityEngine.Random.Range(0, this._highlandsChunkStep.y)
                );
                if (UnityEngine.Random.value <= this.highlandsAmount)
                    this._highlandsPlateCentersChoosen[x, y] = true;
            }
        
        // отмечаем плоскогорья
        Vector2Int pos, curpos;
        int minDistanceToPlate, minDistancePlateX, minDistancePlateY, chunkX, chunkY, distance, curdist;
        float erosion, curval;
        Stack<(Vector2Int, int)> toProcess;
         
        for (int x = 0; x < this.size.x; x++)
            for (int y = 0; y < this.size.y; y++) {
                pos = new Vector2Int(x, y);

                minDistanceToPlate = 1_000_000; 
                minDistancePlateX = -1;
                minDistancePlateY = -1;

                chunkX = x / this._highlandsChunkStep.x;
                chunkY = y / this._highlandsChunkStep.y;
                
                for (int offsetX = -1; offsetX <= 1; offsetX++)
                    for (int offsetY = -1; offsetY <= 1; offsetY++) {
                        if (chunkX+offsetX < 0 || chunkX+offsetX >= this.numberOfHighlandsPlates.x
                                || chunkY+offsetY < 0 || chunkY+offsetY >= this.numberOfHighlandsPlates.y)
                            continue;
                        
                        distance = this.Distance(this._highlandsPlateCenters[chunkX+offsetX, chunkY+offsetY], pos);

                        if (distance < minDistanceToPlate) {
                            minDistanceToPlate = distance;
                            minDistancePlateX = chunkX+offsetX;
                            minDistancePlateY = chunkY+offsetY;
                        }
                    }

                this._highlandsPlateCoords[x, y] = new Vector2Int(minDistancePlateX, minDistancePlateY);

                // если мы на выбраной плите
                if (this._highlandsPlateCentersChoosen[minDistancePlateX, minDistancePlateY])
                    this._highlandsMap[x, y] = 1f;

                // если мы на границе выбраной и не выбраной плиты
                if (x > 0 && this._tectonicPlateCoords[x, y] != this._tectonicPlateCoords[x-1, y]
                            && this._highlandsPlateCentersChoosen[minDistancePlateX, minDistancePlateY]
                                != this._highlandsPlateCentersChoosen[this._tectonicPlateCoords[x-1, y].x, this._tectonicPlateCoords[x-1, y].y]
                        || y > 0 && this._tectonicPlateCoords[x, y] != this._tectonicPlateCoords[x, y-1]
                            && this._highlandsPlateCentersChoosen[minDistancePlateX, minDistancePlateY]
                                != this._highlandsPlateCentersChoosen[this._tectonicPlateCoords[x, y-1].x, this._tectonicPlateCoords[x, y-1].y]) {
                    toProcess = new Stack<(Vector2Int, int)>();
                    curpos = pos;
                    curdist = 0;

                    erosion = this.highlandsErosion.ValueAt(curpos.x, curpos.y)*this.highlandsErosionSlopeInfluence;

                    curval = (1 / Mathf.Pow(erosion, 0.25f)) * Mathf.Pow(erosion - curdist, 0.25f); // функция, которая отвечает за форму краёв плоскогорий

                    if (this._highlandsMap[curpos.x, curpos.y] < curval) {
                        this._highlandsMap[curpos.x, curpos.y] = curval;
                        foreach (Vector2Int newpos in this.Neighbours(curpos, ignoreEmptyCells: false))
                            toProcess.Push((newpos, curdist+1));
                    }

                    while (toProcess.Count > 0) {
                        (curpos, curdist) = toProcess.Pop();

                        erosion = this.highlandsErosion.ValueAt(curpos.x, curpos.y)*this.highlandsErosionSlopeInfluence;

                        curval = (1 / Mathf.Pow(erosion, 0.25f)) * Mathf.Pow(erosion - curdist, 0.25f); // функция, которая отвечает за форму краёв плоскогорий

                        if (curval > this._highlandsMap[curpos.x, curpos.y]) {
                            this._highlandsMap[curpos.x, curpos.y] = curval;
                            foreach (Vector2Int newpos in this.Neighbours(curpos, ignoreEmptyCells: false))
                                toProcess.Push((newpos, curdist+1));
                        }
                    }
                }
            }
    }

    private void GenerateMap() {
        Vector2Int pos;
        float heightNormalized;
        int height;
        GameObject pivot;
        HexCell hexCell;
        Renderer hexCellRenderer;
        LightTransporter lightTransporter;
        int waterHeight = Mathf.RoundToInt(Mathf.Lerp(0, this.numberOfHeights-1, this.waterCoastLevel));
        bool filled;

        for (int x = 0; x < this.size.x; x++)
            for (int y = 0; y < this.size.y; y++) {
                pos = new Vector2Int(x, y);

                // совмещаем всё

                // + == Add
                // | == Combine
                // rigidity + (
                //     (baseTerrain + baseTerrainRigidity)
                //     | (mountains * mountainsHeightMultiplyer)
                //     | (highlands * highlandsHeightMultiplyer)
                // )
                heightNormalized = Noise.Add(
                    (this.rigidity.ValueAt(x, y), this.rigidityHeightInfluence),
                    (
                        Noise.Combine(
                            (
                                Noise.Add(
                                    (this.baseTerrain.ValueAt(x, y), this.baseTerrainHeightInfluence),
                                    (this.baseTerrainRigidity.ValueAt(x, y), this.baseTerrainRigidityHeightInfluence)
                                ),
                                this.overallBaseTerrainHeightInfluence
                            ),
                            (
                                this._mountainsMap[x, y]
                                    * (this.regulateMountainsHeight ? this.mountainsHeightMultiplyer.ValueAt(x, y) : 1),
                                this.mountainsHeightInfluence
                            ),
                            (
                                this._highlandsMap[x, y]
                                    * (this.regulateHighlandsHeight ? this.highlandsHeightMultiplyer.ValueAt(x, y) : 1),
                                this.highlandsHeightInfluence
                            )
                        ),
                        1f
                    )
                );

                heightNormalized -= this.heightMappingDownshift;

                // создаём клетку
                height = Mathf.RoundToInt(Mathf.Lerp(0, this.numberOfHeights-1, heightNormalized));

                pivot = Instantiate(
                    this.hexagonPrefab,
                    this.InUnityCoords(pos, (this.showHeightOnCells ?
                        // ((heightNormalized < this.waterCoastLevel) ? waterHeight : height) // клетки ниже уровня воды затапливаются водой
                        height // рельефное дно
                        : 0)),
                    this.hexagonPrefab.transform.rotation,
                    this.transform
                );
                hexCell = pivot.transform.GetChild(0).GetComponent<HexCell>();

                // устанавливаем переменные LightTransporter'a (для оптимизации)
                lightTransporter = hexCell.GetComponent<LightTransporter>();
                lightTransporter.grid = this;
                lightTransporter.turnManager = this._turnManager;
                lightTransporter.Subscribe();
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
                hexCell.turnManager = this._turnManager;
                hexCell.height = height;
                hexCell.heightNormalized = heightNormalized;
                hexCell.localPos = pos;
                hexCell.biome = this.BiomeOn(pos, heightNormalized);

                // берём Renderer
                hexCellRenderer = hexCell.GetComponent<Renderer>();

                filled = false;

                // красим воду
                if (height < waterHeight) {
                    if (heightNormalized < this.deepWaterLevel) hexCellRenderer.material.color = this.deepWaterColor;
                    else if (heightNormalized < this.waterLevel) hexCellRenderer.material.color = this.waterColor;
                    // TODO: rivers
                    else hexCellRenderer.material.color = this.waterCoastColor;
                    hexCell.isWater = true;

                    // добавляем клетку на карту
                    this.pivots[x, y] = pivot;
                    this.hexCells[x, y] = hexCell;
                    this.hexCellRenderers[x, y] = hexCellRenderer;

                    continue;
                }

                // красим горы
                if (heightNormalized > this.mountainsLevel) {
                    hexCellRenderer.material = _mountainsMats[UnityEngine.Random.Range(0, _mountainsMats.Length)];
                    hexCell.isMountain = true;
                }

                // спавним руду
                if (heightNormalized > this.oreLevel && UnityEngine.Random.value < this.oreFrequency) {
                    this.PlaceToughResource(this.orePrefab, pivot, pos);
                    filled = true;
                }

                if (hexCell.isMountain) {
                    // добавляем клетку на карту
                    this.pivots[x, y] = pivot;
                    this.hexCells[x, y] = hexCell;
                    this.hexCellRenderers[x, y] = hexCellRenderer;

                    continue;
                }
                
                // красим землю по биому и спавним растительность на карте
                switch (hexCell.biome) {
                    case Biome.Beach:
                        hexCellRenderer.material.color = this.beachColor;
                        break;
                    case Biome.EternalSnow:
                        hexCellRenderer.material = _snowMats[UnityEngine.Random.Range(0, _snowMats.Length)];
                        break;
                    case Biome.Tundra:
                        hexCellRenderer.material = _tundraMats[UnityEngine.Random.Range(0, _tundraMats.Length)];
                        if (UnityEngine.Random.value < this.tundraWoodFrequency && !filled) {
                            this.PlaceItem(
                                this.woodPrefab,
                                pivot,
                                pos,
                                UnityEngine.Random.Range(this.tundraMinWoodAmount, this.tundraMaxWoodAmount+1)
                            );
                            filled = true;
                        }
                        break;
                    case Biome.Taiga:
                        hexCellRenderer.material = _taigaMats[UnityEngine.Random.Range(0, _taigaMats.Length)];
                        if (UnityEngine.Random.value < this.taigaTreeFrequency && !filled) {
                            PlaceToughResource(this.treePrefab, pivot, pos);
                            filled = true;
                        }
                        else if (UnityEngine.Random.value < this.taigaWoodFrequency && !filled) {
                            this.PlaceItem(
                                this.woodPrefab,
                                pivot,
                                pos,
                                UnityEngine.Random.Range(this.taigaMinWoodAmount, this.taigaMaxWoodAmount+1)
                            );
                            filled = true;
                        }
                        break;
                    case Biome.Forest:
                        hexCellRenderer.material = _forestMats[UnityEngine.Random.Range(0, _forestMats.Length)];
                        if (UnityEngine.Random.value < this.forestTreeFrequency && !filled) {
                            PlaceToughResource(this.treePrefab, pivot, pos);
                            filled = true;
                        }
                        else if (UnityEngine.Random.value < this.forestWoodFrequency && !filled) {
                            this.PlaceItem(
                                this.woodPrefab,
                                pivot,
                                pos,
                                UnityEngine.Random.Range(this.forestMinWoodAmount, this.forestMaxWoodAmount+1)
                            );
                            filled = true;
                        }
                        break;
                    case Biome.Plains:
                        hexCellRenderer.material = _plainsMats[UnityEngine.Random.Range(0, _plainsMats.Length)];
                        if (UnityEngine.Random.value < this.plainsTreeFrequency && !filled) {
                            PlaceToughResource(this.treePrefab, pivot, pos);
                            filled = true;
                        }
                        else if (UnityEngine.Random.value < this.plainsWoodFrequency && !filled) {
                            this.PlaceItem(
                                this.woodPrefab,
                                pivot,
                                pos,
                                UnityEngine.Random.Range(this.plainsMinWoodAmount, this.plainsMaxWoodAmount+1)
                            );
                            filled = true;
                        }
                        break;
                    case Biome.Steppe:
                        hexCellRenderer.material = _steppeMats[UnityEngine.Random.Range(0, _steppeMats.Length)];
                        if (UnityEngine.Random.value < this.steppeWoodFrequency && !filled) {
                            this.PlaceItem(
                                this.woodPrefab,
                                pivot,
                                pos,
                                UnityEngine.Random.Range(this.steppeMinWoodAmount, this.steppeMaxWoodAmount+1)
                            );
                            filled = true;
                        }
                        break;
                    case Biome.Jungle:
                        hexCellRenderer.material = _jungleMats[UnityEngine.Random.Range(0, _jungleMats.Length)];
                        if (UnityEngine.Random.value < this.jungleTreeFrequency && !filled) {
                            PlaceToughResource(this.treePrefab, pivot, pos);
                            filled = true;
                        }
                        else if (UnityEngine.Random.value < this.jungleWoodFrequency && !filled) {
                            this.PlaceItem(
                                this.woodPrefab,
                                pivot,
                                pos,
                                UnityEngine.Random.Range(this.jungleMinWoodAmount, this.jungleMaxWoodAmount+1)
                            );
                            filled = true;
                        }
                        break;
                    case Biome.Desert:
                        hexCellRenderer.material = _desertMats[UnityEngine.Random.Range(0, _desertMats.Length)];
                        break;
                }

                // добавляем клетку на карту
                this.pivots[x, y] = pivot;
                this.hexCells[x, y] = hexCell;
                this.hexCellRenderers[x, y] = hexCellRenderer;
            }
    }
    private void PlaceToughResource(ToughResources prefab, GameObject pivot, Vector2Int pos) {
        ToughResources toughResource = Instantiate(
            prefab,
            pivot.transform.position,
            prefab.transform.rotation,
            this.transform
        );
        toughResource.LocalCoords = pos;
    }
    private void PlaceItem(CollectableItem prefab, GameObject pivot, Vector2Int pos, int amount=1) {
        CollectableItem item = Instantiate(
            prefab,
            pivot.transform.position,
            prefab.transform.rotation,
            this.transform
        );
        item.LocalCoords = pos;
        item.NumberOfItems = amount;
    }

    private Biome BiomeOn(Vector2Int pos, float heightNormalized = -1f/*брать у клетки*/) {
        if (!this.InBoundsOfMap(pos))
            throw new System.Exception("локальные координаты "+pos.ToString()+" за пределами поля.");

        float realHeightNormalized;

        if (heightNormalized == -1f) {
            if (this.hexCells[pos.x, pos.y] == null)
                throw new System.Exception("нету клетки в локальных координатах "+pos.ToString()+", а значит невозможно взять её высоту.");
            realHeightNormalized = this.hexCells[pos.x, pos.y].heightNormalized;
        }
        else realHeightNormalized = heightNormalized;

        if (this.waterCoastLevel <= realHeightNormalized && realHeightNormalized < this.beachLevel)
            return Biome.Beach;
        
        float temperature = this.temperature.ValueAt(pos.x, pos.y);
        float wetness = this.wetness.ValueAt(pos.x, pos.y);

        if (temperature < this.veryColdLevel) return Biome.EternalSnow;
        if (temperature < this.coldLevel) return (wetness < this.dryLevel) ? Biome.Tundra : Biome.Taiga;
        if (temperature < this.normalTemperatureLevel) return (wetness < this.wetLevel) ? Biome.Plains : Biome.Forest;
        if (temperature < this.hotLevel) return (wetness < this.normalWetnessLevel) ? Biome.Steppe : Biome.Jungle;
        if (temperature <= this.veryHotLevel) return Biome.Desert;

        throw new System.Exception("невозможно вычислить биом для координат "+pos.ToString()+".");
    }

    // public void UpdateHeight(Vector2Int pos, int newHeight) {
    //     HexCell hexCell = this.hexCells[pos.x, pos.y];

    //     hexCell.transform.position = this.InUnityCoords(
    //         pos,
    //         (this.showHeightOnCells ? newHeight : 0)
    //     );

    //     // раширяем клету и двигаем её центр (можно убрать эту часть и клетки будут просто подниматься по оси у)
    //     hexCell.transform.localScale = new Vector3(
    //         hexCell.transform.localScale.x,
    //         hexCell.transform.localScale.y,
    //         this.hexagonPrefab.transform.localScale.z
    //             + hexCell.transform.position.y / (this._hexagonPrefabRenderer.bounds.size.y / this.hexagonPrefab.transform.localScale.z)
    //     );
    //     hexCell.transform.localPosition = new Vector3(
    //         hexCell.transform.localPosition.x,
    //         -hexCell.transform.position.y/2,
    //         hexCell.transform.localPosition.z
    //     );
    // }

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

    public Vector2Int[] Neighbours(Vector2Int pos, int maxHeightDifference = -1/*any height difference*/, bool restrictDownMovement = false, bool ignoreEmptyCells = true) {
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
            && (ignoreEmptyCells ? this.pivots[newpos.x, newpos.y] != null: true)
            && (maxHeightDifference == -1 ?
                true
                : (restrictDownMovement ?
                    Mathf.Abs(this.hexCells[newpos.x, newpos.y].height - this.hexCells[pos.x, pos.y].height)
                    : this.hexCells[newpos.x, newpos.y].height - this.hexCells[pos.x, pos.y].height
                ) <= maxHeightDifference
            )
        );
    }
    public Vector2Int[] Neighbours(Vector3 position, int maxHeightDifference = -1/*any height difference*/, bool restrictDownMovement = false, bool ignoreEmptyCells = true) =>
        this.Neighbours(this.InLocalCoords(position), maxHeightDifference, restrictDownMovement, ignoreEmptyCells);

    public int Distance(Vector2Int pos1, Vector2Int pos2) {
        Vector2Int start = pos1;
        Vector2Int end = pos2;
        int dist = 0;

        while (start != end) {
            Vector2Int delta = end - start;

            if (start.y % 2 == 0) {
                if (delta.y != 0) {
                    start.y += Mathf.Clamp(delta.y, -1, 1);
                    if (delta.x < 0) start.x--;// skip
                    
                    dist++;
                    continue;
                }
                //just go by x
                return dist + Mathf.Abs(delta.x);
            }

            if (delta.y != 0) {
                start.y += Mathf.Clamp(delta.y, -1, 1);
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
