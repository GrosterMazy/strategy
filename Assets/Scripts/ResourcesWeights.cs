using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ResourcesWeights
{
    public static Dictionary<string, int> ResourcesWeightsPerItemTable = new Dictionary<string, int>();

    static ResourcesWeights() {
        ResourcesWeightsPerItemTable["Tree"] = 1;  ResourcesWeightsPerItemTable["Wood"] = 1; ResourcesWeightsPerItemTable["Ore"] = 1;
        ResourcesWeightsPerItemTable["Seed"] = 1; ResourcesWeightsPerItemTable["Food"] = 1; ResourcesWeightsPerItemTable["Light"] = 0;
        ResourcesWeightsPerItemTable["Darkness"] = 0;} // запишите сюда значения масс всех ресурсов в игре по образцу
}