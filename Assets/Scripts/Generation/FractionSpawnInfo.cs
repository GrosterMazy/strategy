using UnityEngine;
using System;

[Serializable] public class FractionSpawnInfo {
    public ObjectOnGrid mainObject;
    public ObjectOnGrid objectAround;
    public int numberOfObjectsAround = 2;
}
