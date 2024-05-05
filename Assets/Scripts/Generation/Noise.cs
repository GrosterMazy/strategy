using UnityEngine;
using System;

[Serializable] public class Noise {
    [SerializeField] private float scale;
    [SerializeField] private bool randomSeed = true;  // false - использует поле seed, true - генерирует случайный(его можно увидеть в поле seed после запуска)
    private float _minSeedNumber = 0f;
    private float _maxSeedNumber = 10_000f;
    
    [SerializeField] private Vector2 seed;
    public void Init() {
        if (this.randomSeed)
            this.seed = new Vector2(
                UnityEngine.Random.Range(this._minSeedNumber, this._maxSeedNumber),
                UnityEngine.Random.Range(this._minSeedNumber, this._maxSeedNumber)
            );
    }

    public float ValueAt(float x, float y) =>
        Mathf.Clamp(Mathf.PerlinNoise(
            this.seed.x + x/this.scale,
            this.seed.y + y/this.scale
        ), 0f, 1f);

    public static float Add(params (float, float)[] terms) {
        float sumValue = 0;
        float influenceSum = 0;
        foreach ((float, float) term in terms) {
            sumValue += term.Item1*term.Item2;
            influenceSum += term.Item2;
        }
        return sumValue / influenceSum;
    }
    public static float Combine(params (float, float)[] terms) {
        float maxValue = 0;
        float maxInfluence = 0;
        foreach ((float, float) term in terms) {
            maxValue = Mathf.Max(maxValue, term.Item1*term.Item2);
            maxInfluence = Mathf.Max(maxInfluence, term.Item2);
        }
        return maxValue / maxInfluence;
    }

    public static float AddAbove((float, float) baseTerm, params (float, float)[] terms) {
        float sumValue = baseTerm.Item1*baseTerm.Item2;
        float influenceSum = baseTerm.Item2;
        if (baseTerm.Item1 > 0)
            foreach ((float, float) term in terms) {
                sumValue += term.Item1*term.Item2;
                influenceSum += term.Item2;
            }
        return sumValue / influenceSum;
    }
}
