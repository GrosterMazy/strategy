using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ResourcesWeights
{
    public static Dictionary<string, int> ResourcesWeightsPerItemTable = new Dictionary<string, int>();

    static ResourcesWeights() {
        ResourcesWeightsPerItemTable["Tree"] = 5;
        ResourcesWeightsPerItemTable["Wood"] = 4;

        ResourcesWeightsPerItemTable["Ore"] = 8;
        ResourcesWeightsPerItemTable["Steel"] = 10;

        ResourcesWeightsPerItemTable["Seed"] = 1;
        ResourcesWeightsPerItemTable["Food"] = 2;
        
        ResourcesWeightsPerItemTable["Light"] = 0;
        ResourcesWeightsPerItemTable["Darkness"] = 0; }// запишите сюда значения масс всех ресурсов в игре по образцу
}
