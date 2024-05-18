public class Lighting : EffectsDescription
{
    protected int lightForce;
    protected int lightDecrease;
    protected int lightDuration;
    protected void Start()
    {
        if (TryGetComponent<LightTransporter>(out LightTransporter lightTransporter))
        {
            lightTransporter.SetLight(lightForce, lightDecrease, lightDuration);
        }
        Destroy(this);
    }
}