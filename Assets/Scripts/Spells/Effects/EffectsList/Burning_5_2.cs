public class Burning_5_2 : Burning
{
    new private void Start()
    {
        _damage = 5;
        remainingLifeTime = 2;
        base.Start();
    }
}