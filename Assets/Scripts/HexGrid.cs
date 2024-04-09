using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour {
    public Vector2Int  size;
    public Vector2 spacing;

    public GameObject hexagonPrefab;
    private Renderer _hexagonPrefabRenderer;
    /* Например поле 5(по x) на 4(по z) будет выглядеть так:
    .-> z
    |
    v x

    * * * *
     * b c *
    * g a d
     * f e *
    * * * *
    */

    public HexCell[,] cells;
    /*
    Координаты построены как будто у нас квадратное поле, тоесть на нашем примере так:
    * * * *
    * b c *
    * g a d
    * f e *
    * * * *
    */
    
    private void Start() {
        this._hexagonPrefabRenderer = this.hexagonPrefab.GetComponent<Renderer>();

        this.cells = new HexCell[this.size.x, this.size.y];

        for (int x = 0; x < this.size.x; x++)
            for (int y = 0; y < this.size.y; y++)
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
}
