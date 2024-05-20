using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class EnumEffectsTranslator : MonoBehaviour
{
    public enum EffetsEnum // При добавлении нового эффекта, кроме создания его скрипта, также нужно будет прописать его инициализацию в InitEffectTranslator()
    {
        ArmourChange_1_3 = 7,
        ArmourChange_neg1_3 = 8, // Макс индекс 8
        Burning_5_1 = 4,
        Burning_5_2 = 5,
        Burning_3 = 6,
        Lighting_3 = 0,
        Poisoned = 2,
        Wet = 1,
        WorkerSpawner = 3
    }
    [NonSerialized] public int effectsEnumLength = 7;
    public Dictionary<int, Type> EffectTranslator = new Dictionary<int, Type>();

    private void Awake()
    {
        InitEffectTranslator();
    }
    public void InitEffectTranslator()
    {
        EffectTranslator[0] = typeof(Lighting_3);
        EffectTranslator[1] = typeof(Wet);
        EffectTranslator[2] = typeof(Poisoned);
        EffectTranslator[3] = typeof(WorkerSpawner);
        EffectTranslator[4] = typeof(Burning_5_1);
        EffectTranslator[5] = typeof(Burning_5_2);
        EffectTranslator[6] = typeof(Burning_3);
        EffectTranslator[7] = typeof(ArmourChange_1_3);
        EffectTranslator[8] = typeof(ArmourChange_neg1_3);
    }
    public Type Translate(int effect)
    {
        return EffectTranslator[effect];
    }
}

/*
        ОПИСАНИЕ ЭФФЕКТОВ
Burning - наносит урон, игнорируя броню, в момент наложения и в начале каждого хода, пока действует,
при взаимодействии с эффектом Wet они оба уничтожаются

Wet - сам по себе ничего не делает, при взаимодействии с Burning они оба уничтожаются

Poisoned - то же, что и Burning

Spawner - на клеточке с эффектом в момент наложения и в начале каждого хода, пока действует эффект,
если там ничего нет, появляется объект, живущий определённое кол-во ходов и принадлежащий определённой команде

Lighting - LightTransporter с эффектом становится источником света


*/