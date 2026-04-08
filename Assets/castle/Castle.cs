using UnityEngine;

public class Castle : MonoBehaviour, IDamagable
{
    public int maxHealth = 1000;
    public int Health { get; set; }
    public int h = 0;
    private void Start()
    {
        Health = maxHealth;
        h = Health;
    }
    public void Damage(Damage.Request d)
    {
        if (d.type == "explosive") Health -= d.damage;
        h = Health;
        Debug.Log($"castle damage: {d.damage} health: {Health} type: {d.type} source: {d.source}");
        if (Health <= 0)
        {
            //PlayerEvents.OnPlayerDied.Invoke();
            Destroy(gameObject);
        }
    }
}

