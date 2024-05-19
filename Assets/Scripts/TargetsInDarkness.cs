using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetsInDarkness : MonoBehaviour 
{
    public List<Vector2Int> Targets = new List<Vector2Int>();

    public void AddTarget(Vector2Int _coords) { Targets.Add(_coords); }
    public void RemoveTarget(Vector2Int _coords) { Targets.Remove(_coords); }
}
