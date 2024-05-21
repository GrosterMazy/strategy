using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FractionSpawner : MonoBehaviour {
    [SerializeField] private int minDistanceBetweenFractions = 1; // больше 0 и меньше максимального расстояния между двумя точками на hexGrid
    [SerializeField] private int maxDistanceBetweenFractions = -1; // больше 0 и меньше максимального расстояния между двумя точками на hexGrid (-1 == нет ограничений)

    [SerializeField] private int minDistanceFromBorders = 3; // больше 0 и меньше половины максимального размера hexGrid

    public FractionSpawnInfo[] fractionPrefabs;
    private List<Vector2Int> _buildingCoords = new List<Vector2Int>();
    private List<List<Vector2Int>> _unitAroundCoords = new List<List<Vector2Int>>();

    private HexGrid _hexGrid;
    private PlacementManager _placementManager;

    private void OnEnable() =>
        PlacementManager.onGridCreated += OnGridCreated;

    private void OnDestroy() =>
        PlacementManager.onGridCreated -= OnGridCreated;
    
    private void OnGridCreated() {
        this._hexGrid = FindObjectOfType<HexGrid>();
        this._placementManager = FindObjectOfType<PlacementManager>();

        // проверка, что maxDistanceBetweenFractions и maxDistanceBetweenFractions меньше максимального расстояния между двумя точками на hexGrid
        int maxDist = this._hexGrid.Distance(new Vector2Int(0, 0), new Vector2Int(this._hexGrid.size.x-1, this._hexGrid.size.y-1));
        if (minDistanceBetweenFractions >= maxDist || minDistanceBetweenFractions >= maxDist)
            throw new System.Exception("minDistanceBetweenFractions и maxDistanceBetweenFractions должны быть меньше максимального расстояния между двумя точками на доске(в данном случае "+maxDist.ToString()+").");

        this.GenerateBuildingCoords();
        this.SpawnBuildings();
    }

    private void GenerateBuildingCoords() { // TODO: алгоритм, спавнящий фракции в максимально равных условиях с учётом их особенностей
        if (this.fractionPrefabs.Length == 0) return;

        Vector2Int maxOfMinDistPos = Vector2Int.zero, pos;
        int maxOfMinDist, minDist, dist;
        Vector2Int[] neighbours;

        pos = new Vector2Int(
            UnityEngine.Random.Range(0, this._hexGrid.size.x),
            UnityEngine.Random.Range(0, this._hexGrid.size.y)
        );

        neighbours = Array.FindAll(this._hexGrid.Neighbours(pos), neighbourPos =>
            this._placementManager.gridWithObjectsInformation[neighbourPos.x, neighbourPos.y] == null                
        );

        while (this._placementManager.gridWithObjectsInformation[pos.x, pos.y] != null
                || this._hexGrid.hexCells[pos.x, pos.y].isWater || this._hexGrid.hexCells[pos.x, pos.y].isMountain
                || neighbours.Length < this.fractionPrefabs[0].numberOfObjectsAround) {
            pos = new Vector2Int(
                UnityEngine.Random.Range(0, this._hexGrid.size.x),
                UnityEngine.Random.Range(0, this._hexGrid.size.y)
            );
            neighbours = Array.FindAll(this._hexGrid.Neighbours(pos), neighbourPos =>
                this._placementManager.gridWithObjectsInformation[neighbourPos.x, neighbourPos.y] == null                
            );
        }

        this._buildingCoords.Add(pos);
        this._unitAroundCoords.Add(new List<Vector2Int>());
        int[] exclude = new int[6];
        int count = 0, ri;
        while (count < this.fractionPrefabs[0].numberOfObjectsAround) {
            ri = UnityEngine.Random.Range(0, neighbours.Length);
            if (Array.Exists(exclude, excluded => ri == excluded)) continue;
            exclude[count] = ri;
            this._unitAroundCoords[this._unitAroundCoords.Count-1].Add(neighbours[ri]);
            count++;
        }
        
        if (this.fractionPrefabs.Length == 1) return;

        for (int i = 1; i < this.fractionPrefabs.Length; i++) {
            maxOfMinDist = -1;
            
            for (int x = this.minDistanceFromBorders; x < this._hexGrid.size.x-this.minDistanceFromBorders; x++)
                for (int y = this.minDistanceFromBorders; y < this._hexGrid.size.y-this.minDistanceFromBorders; y++) {
                    if (this._hexGrid.hexCells[x, y].isWater || this._hexGrid.hexCells[x, y].isMountain
                        || this._placementManager.gridWithObjectsInformation[x, y] != null) continue;

                    pos = new Vector2Int(x, y);
                    minDist = 1_000_000;

                    foreach (Vector2Int buildingCoord in this._buildingCoords) {
                        dist = this._hexGrid.Distance(pos, buildingCoord);
                        if (dist < this.minDistanceBetweenFractions
                                || this.maxDistanceBetweenFractions != -1 && dist > this.maxDistanceBetweenFractions) {
                            minDist = 1_000_000;
                            break;
                        }
                        
                        if (dist < minDist) minDist = dist;
                    }
                    
                    if (minDist == 1_000_000) continue;
                    
                    if (minDist > maxOfMinDist) {
                        maxOfMinDist = minDist;
                        maxOfMinDistPos = pos;
                    }
                }
            
            if (maxOfMinDist == -1) {
                this._buildingCoords.Clear();
                this._unitAroundCoords.Clear();
                this.GenerateBuildingCoords();
                return;
            }

            neighbours = Array.FindAll(this._hexGrid.Neighbours(maxOfMinDistPos), neighbourPos =>
                this._placementManager.gridWithObjectsInformation[neighbourPos.x, neighbourPos.y] == null                
            );
            
            this._buildingCoords.Add(maxOfMinDistPos);
            this._unitAroundCoords.Add(new List<Vector2Int>());
            exclude = new int[6];
            count = 0;
            while (count < this.fractionPrefabs[i].numberOfObjectsAround) {
                ri = UnityEngine.Random.Range(0, neighbours.Length);
                if (Array.Exists(exclude, excluded => ri == excluded)) continue;
                exclude[count] = ri;
                this._unitAroundCoords[this._unitAroundCoords.Count-1].Add(neighbours[ri]);
                count++;
            }
        }
    }
    private void SpawnBuildings() {
        ObjectOnGrid mainBuilding, unitAround;
        int count;

        for (int i = 0; i < this._buildingCoords.Count; i++) {
            mainBuilding = Instantiate(
                this.fractionPrefabs[i].mainObject,
                this._hexGrid.pivots[this._buildingCoords[i].x, this._buildingCoords[i].y].transform.position,
                this.fractionPrefabs[i].mainObject.transform.rotation
            );
            mainBuilding.LocalCoords = this._buildingCoords[i];
            this._placementManager.UpdateGrid(this._buildingCoords[i], this._buildingCoords[i], mainBuilding);

            foreach (Vector2Int unitPos in this._unitAroundCoords[i]) {
                unitAround = Instantiate(
                    this.fractionPrefabs[i].objectAround,
                    this._hexGrid.pivots[unitPos.x, unitPos.y].transform.position,
                    this.fractionPrefabs[i].objectAround.transform.rotation
                );
                unitAround.LocalCoords = unitPos;
                this._placementManager.UpdateGrid(unitPos, unitPos, unitAround);
            }
        }
    }
}
