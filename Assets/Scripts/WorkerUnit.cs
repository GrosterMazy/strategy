using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerUnit : MonoBehaviour
{
    public UnitDescription Specifications;

    private void Start()
    {
        Specifications.ArmorCounter();
    }
}
