using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IDamagable
{
    public int maxHealth = 100;
    public int Health { get; set; }
    public Transform target;
    private NavMeshAgent agent;
    public float UpdateInterval = 0.5f;
    private Explosion explosion;
    public GameObject impactEffectPrefab;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        explosion = GetComponent<Explosion>();
    }
    private void Start()
    {
        StartCoroutine(MoveToTarget());
        Health = maxHealth;
    }
    private IEnumerator MoveToTarget()
    {
        WaitForSeconds wait = new WaitForSeconds(UpdateInterval);
        while (enabled)
        {
            agent.SetDestination(target.position);
            yield return wait;
        }
    }
    public void Damage(Damage.Request d)
    {
        Debug.Log($"current health {Health}");
        if (d.type == "explosive") Health -= d.damage;
        Debug.Log($"damage: {d.damage} health: {Health} type: {d.type} source: {d.source}");
        if (Health <= 0)
        {
            Destroy(gameObject);
            //if (d.source.tag == "Player")
            //{
                //PlayerEvents.OnPlayerScored.Invoke(score);
            //}
        }
    }
    float DistanceToTarget()
    {
        float distance = Vector3.Distance(transform.position, target.position);
       return distance;
    }
    void Update() {         
        if (DistanceToTarget() < 1f)
        {
            //PlayerEvents.OnPlayerDied.Invoke();
            Destroy(gameObject);
            explosion.maxDamage = 100;
            explosion.sourceObject = gameObject;
            explosion.maxForce = 0f;
            explosion.affectedLayers = LayerMask.GetMask("castle");
            explosion.blastRadius = 5f;
            if (impactEffectPrefab != null)
            {
                GameObject effect = Instantiate(impactEffectPrefab, transform.position, Quaternion.LookRotation(Vector3.up));
                Destroy(effect, 5f);
            }


            explosion.ExplodeAt(transform.position);
        }
    }
}
