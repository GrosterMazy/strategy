using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmourChange_1_3 : ArmourChange
{
    new protected void Start()
    {
        remainingLifeTime = 3;
        isNegative = false;
        armourChange = 1;
        base.Start();
    }
}
