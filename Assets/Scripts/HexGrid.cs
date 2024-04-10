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

    [NonSerialized] public HexCell[,] cells;
    /*
    Координаты построены как будто у нас квадратное поле, тоесть на нашем примере так:
    * * * *
    * b c *
    * g a d
    * f e *
    * * * *
    */
    [SerializeField] private Vector2 spacing; // расстояние между клетками

    [SerializeField] bool noise = false; // false - поле полностью заполнено, true - некая форма в пределах размера
    [SerializeField] bool randomSeed = true; // false - использует поле seed, true - генерирует случайный(его можно увидеть в поле seed после запуска)
    private float _minSeedNumber = 0f;
    private float _maxSeedNumber = 10_000f;
    
    [SerializeField] Vector2 seed;
    [SerializeField, Range(0f, 1f)] private float waterLevel = 0.3f; // уровень "воды" на карте. работет только при noise == true
    [SerializeField] private float smoothness = 10; // насколько ровный ландшафт карты. работет только при noise == true
    
    private void Start() {
        this._hexagonPrefabRenderer = this.hexagonPrefab.GetComponent<Renderer>();

        this.cells = new HexCell[this.size.x, this.size.y];

        this.GenerateMap();
    }

    private void GenerateMap() {
        if (this.randomSeed)
            this.seed = new Vector2(
                UnityEngine.Random.Range(this._minSeedNumber, this._maxSeedNumber),
                UnityEngine.Random.Range(this._minSeedNumber, this._maxSeedNumber)
            );

        for (int x = 0; x < this.size.x; x++)
            for (int y = 0; y < this.size.y; y++)
                if (!this.noise 
                        || (this.noise && Mathf.PerlinNoise(
                                            x/this.smoothness+this.seed.x,
                                            y/this.smoothness+this.seed.y
                                        ) > waterLevel))
                    this.cells[x, y] = Instantiate(
                        this.hexagonPrefab,
                        this.InUnityCoords(new Vector2Int(x, y)),
                        this.hexagonPrefab.transform.rotation,
                        this.transform
                    ).GetComponent<HexCell>();
    }

    public Vector3 InUnityCoords(Vector2Int pos) {
        return new Vector3(
            pos.x * this._hexagonPrefabRenderer.bounds.size.x
                + ((pos.y % 2 == 0) ? 0 : (this._hexagonPrefabRenderer.bounds.size.x / 2))
                + this.spacing.x * pos.x,
            0,
            pos.y * this._hexagonPrefabRenderer.bounds.size.x * Mathf.Cos(Mathf.PI/6)
                + this.spacing.y * pos.y
        );
    }

    public Vector2Int InLocalCoords(Vector3 position) { // TODO: более оптимизированная версия
        for (int x = 0; x < this.size.x; x++)
            for (int y = 0; y < this.size.y; y++)
                if (this.cells[x, y] != null
                        && this.InUnityCoords(new Vector2Int(x, y)) == new Vector3(position.x, 0, position.z))
                    return new Vector2Int(x, y);
        return new Vector2Int(-1, -1); // нет такой позиции
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
        );
    }
}
