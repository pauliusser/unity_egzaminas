using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(TrailRenderer))]
[RequireComponent(typeof(Explosion))]
public class ExplosiveProjectile : MonoBehaviour
{
    [Header("Impact Effect")]
    public GameObject impactEffectPrefab;   // Particle system prefab (e.g., sparks)
    public float effectDestroyDelay = 2f;    // Time after which the effect is destroyed
    public int blastDmg;
    public GameObject damageSource;

    [Header("Trail Settings")]
    public float trailTime = 0.5f;
    public float trailStartWidth = 0.1f;
    public float trailEndWidth = 0f;

    [Header("Debug")]
    public bool debugDraw = true;
    public Color debugColor = Color.red;

    private Rigidbody rb;
    private TrailRenderer trail;
    private Explosion explosion;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trail = GetComponent<TrailRenderer>();
        explosion = GetComponent<Explosion>();

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        SetupTrail();
    }

    // void Start()
    // {
    //     // Set the source of the explosion to this projectile
    //     explosion.sourceObject = gameObject;
    // }

    void SetupTrail()
    {
        trail.time = trailTime;
        trail.startWidth = trailStartWidth;
        trail.endWidth = trailEndWidth;
        trail.minVertexDistance = 0.05f;
        trail.emitting = true;

        // Optional gradient (keep as in original)
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.yellow, 0.0f),
                new GradientColorKey(Color.red, 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        trail.colorGradient = gradient;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Get contact info
        ContactPoint contact = collision.contacts[0];
        Vector3 hitPoint = contact.point;
        Vector3 hitNormal = contact.normal;

        // Debug: draw hit point and normal
        if (debugDraw)
        {
            Debug.DrawLine(hitPoint, hitPoint + hitNormal * 0.5f, debugColor, 2f);
            Debug.DrawLine(hitPoint, hitPoint + Vector3.up * 0.2f, Color.green, 2f);
            Debug.Log($"Projectile hit {collision.collider.name} at {hitPoint}");
        }

        // Spawn impact effect (using original orientation: Z axis aligned with normal)
        if (impactEffectPrefab != null)
        {
            GameObject effect = Instantiate(impactEffectPrefab, hitPoint, Quaternion.LookRotation(hitNormal));
            Destroy(effect, effectDestroyDelay);
        }

        explosion.maxDamage = blastDmg;
        explosion.sourceObject = damageSource != null ? damageSource : gameObject;
        // Trigger explosion at impact point
        explosion.ExplodeAt(hitPoint);

        // Destroy the projectile immediately
        Destroy(gameObject);
    }

    // Optional: fallback destroy if projectile goes out of world
    void Update()
    {
        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }
}