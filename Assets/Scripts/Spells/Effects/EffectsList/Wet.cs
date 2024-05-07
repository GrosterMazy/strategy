public class Wet : EffectsDescription
{
    private void Start()
    {
        remainingLifeTime = 1;
        if (TryGetComponent<Burning>(out Burning burningEffect))
        {
            Destroy(burningEffect);
            Destroy(this);
        }
    }
}
