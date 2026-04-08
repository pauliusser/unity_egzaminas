using UnityEngine;

public interface IDamagable
{
    int Health { get; set; }
    void Damage(Damage.Request dr);
}
public static class Damage
{
    public struct Request
    {
        public int damage;
        public string type;
        public GameObject source;
    }
}