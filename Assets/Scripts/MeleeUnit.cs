using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeUnit : MonoBehaviour
{
    public UnitDescription Specifications;

    private void Start()
    {
        Specifications.ArmorCounter();
    }
}
