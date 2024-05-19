public class Burning_5_1 : Burning
{
    new private void Start()
    {
        _damage = 5;
        remainingLifeTime = 1;
        base.Start();
    }
}