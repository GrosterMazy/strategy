using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedUnit : MonoBehaviour
{
    public UnitDescription Specifications;

    private void Start()
    {
        Specifications.ArmorCounter();
    }
}
