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

    public float ValueAt(float x, float y) {
        return Mathf.Clamp(Mathf.PerlinNoise(
            this.seed.x + x/this.scale,
            this.seed.y + y/this.scale
        ), 0f, 1f);
    }
}
