using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HexGrid : MonoBehaviour {
    [SerializeField] private GameObject hexagonPrefab;
    private Renderer _hexagonPrefabRenderer;

    public Vector2Int  size; // размер в клетках
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

    [NonSerialized] public GameObject[,] cells;
    [NonSerialized] public HexCell[,] childs;
    /*
    Координаты построены как будто у нас квадратное поле, тоесть на нашем примере так:
    * * * *
    * b c *
    * g a d
    * f e *
    * * * *
    */
    [SerializeField] private Vector2 spacing; // расстояние между клетками

    [SerializeField] private bool randomSeed = true; // false - использует поле seed, true - генерирует случайный(его можно увидеть в поле seed после запуска)
    private float _minSeedNumber = 0f;
    private float _maxSeedNumber = 10_000f;
    
    [SerializeField] private Vector2 seed; // ключ генератора карты высот

    [SerializeField] private float smoothness = 10; // насколько ровный ландшафт карты

    [SerializeField, Range(0f, 1f)] private float waterLevel = 0.3f; // высота от 0(минимальная) до 1(максимальная), ниже которой перестают создаваться клетки

    
    [SerializeField] private int numberOfHeights = 2; // кол-во ступеней высот. минимальная высота 0, максимальная numberOfHeights-1
    [SerializeField] private bool showHeightOnCells = false; // раставлять клетки в соответствии с их высотой
    [SerializeField] private bool linearHeightStep = true; // true - клетки разница в высоте между клетками с соседними значениями height равна heightStep, false - берётся из heightSteps
    [SerializeField] private float heightStep;
    [SerializeField] private float[] heightSteps;

    
    private void Awake() {
        this._hexagonPrefabRenderer = this.hexagonPrefab.transform.GetChild(0).GetComponent<Renderer>();

        this.cells = new GameObject[this.size.x, this.size.y];
        this.childs = new HexCell[this.size.x, this.size.y];

        this.GenerateMap();
    }

    private void GenerateMap() {
        if (this.randomSeed)
            this.seed = new Vector2(
                UnityEngine.Random.Range(this._minSeedNumber, this._maxSeedNumber),
                UnityEngine.Random.Range(this._minSeedNumber, this._maxSeedNumber)
            );

        for (int x = 0; x < this.size.x; x++)
            for (int y = 0; y < this.size.y; y++) {
                float heightNormalized = Mathf.Clamp(Mathf.PerlinNoise(
                    x/this.smoothness + this.seed.x,
                    y/this.smoothness + this.seed.y
                ), 0f, 1f);

                if (heightNormalized < this.waterLevel) continue;

                int height = (int)Mathf.Round(Mathf.Lerp(0, this.numberOfHeights-1, heightNormalized));

                GameObject cell = Instantiate(
                    this.hexagonPrefab,
                    this.InUnityCoords(
                        new Vector2Int(x, y),
                        (this.showHeightOnCells ? height : 0)
                    ),
                    this.hexagonPrefab.transform.rotation,
                    this.transform
                );
                HexCell child = cell.transform.GetChild(0).GetComponent<HexCell>();

                child.transform.localScale = new Vector3(
                    child.transform.localScale.x,
                    child.transform.localScale.y,
                    child.transform.localScale.z
                        + child.transform.position.y / (this._hexagonPrefabRenderer.bounds.size.y / child.transform.localScale.z)
                    );

                child.transform.localPosition = new Vector3(
                    child.transform.localPosition.x,
                    -child.transform.position.y/2,
                    child.transform.localPosition.z
                );

                child.height = height;
                
                this.cells[x, y] = cell;
                this.childs[x, y] = child;
            }
    }

    public Vector3 InUnityCoords(Vector2Int pos, int height = -1/*высота клетки*/) {
        if (this.cells[pos.x, pos.y] == null && height == -1)
            throw new System.Exception("нету клетки в локальных координатах "+pos.ToString()+", а значит невозможно взять её высоту.");

        int realHeight = (height == -1) ? this.childs[pos.x, pos.y].height : height;

        return new Vector3(
            pos.x * this._hexagonPrefabRenderer.bounds.size.x
                + ((pos.y % 2 == 0) ? 0 : (this._hexagonPrefabRenderer.bounds.size.x / 2))
                + this.spacing.x * pos.x,
            (this.showHeightOnCells ?
                (this.linearHeightStep ? realHeight*this.heightStep : this.heightSteps[realHeight])
                : 0),
            pos.y * this._hexagonPrefabRenderer.bounds.size.x * Mathf.Cos(Mathf.PI/6)
                + this.spacing.y * pos.y
        );
    }

    public Vector2Int InLocalCoords(Vector3 position) { // игнорирует координату "y" // TODO: более оптимизированная версия
        for (int x = 0; x < this.size.x; x++)
            for (int y = 0; y < this.size.y; y++)
                if (this.cells[x, y] != null) {
                    Vector3 fromLocal = this.InUnityCoords(new Vector2Int(x, y));
                    if (fromLocal.x == position.x && fromLocal.z == position.z)
                        return new Vector2Int(x, y);
                }
        throw new System.Exception("невозможно перевести "+position.ToString()+" в локальные координаты.");
    }

    public Vector2Int[] Neighbours(Vector2Int pos) {
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
            && this.cells[newpos.x, newpos.y] != null
        );
    }
    public Vector2Int[] Neighbours(Vector3 position) {
        return this.Neighbours(this.InLocalCoords(position));
    }

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
    public int Distance(Vector3 position1, Vector3 position2) {
        return this.Distance(this.InLocalCoords(position1), this.InLocalCoords(position2));
    }
}
