using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(TrailRenderer))]
public class KineticProjectile : MonoBehaviour
{
    public float linearDamping = 1.5f;
    public float angularDamping = 2f;

    // Trail settings
    public float trailTime = 0.5f;
    public float trailStartWidth = 0.1f;
    public float trailEndWidth = 0f;
    
    // Post-collision trail fade settings
    public float fadeDuration = 1.5f; // How long the trail takes to fade out after collision
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); // Controls fade progression
    
    // Sinking settings
    public bool enableSinking = true; // Toggle to enable/disable sinking behavior
    public float velocityThreshold = 0.1f; // Velocity below this triggers sinking
    public float sinkDuration = 2f; // How long sinking takes
    public float sinkDistance = 0.5f; // How far the projectile sinks into the ground
    public AnimationCurve sinkCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Controls sink progression
    public int kineticDmg = 10;
    public GameObject damageSource;
    private Rigidbody rb;
    private TrailRenderer trail;
    private Vector3 previousPosition;
    private bool isInAir = true;
    
    // Fade tracking variables
    private float collisionTime;
    private bool isFading = false;
    private float originalTrailTime;
    private float originalStartWidth;
    
    // Sink tracking variables
    private bool isSinking = false;
    private float sinkStartTime;
    private Vector3 sinkStartPosition;
    private Vector3 sinkTargetPosition;
    private Collider projectileCollider;
    Damage.Request kineticDamage;
    
    void Awake()
    {
        if (damageSource == null) damageSource = gameObject;
        rb = GetComponent<Rigidbody>();
        trail = GetComponent<TrailRenderer>();
        projectileCollider = GetComponent<Collider>();

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        previousPosition = transform.position;

        SetupTrail();
        
        // Store original trail values for fade calculations
        originalTrailTime = trailTime;
        originalStartWidth = trailStartWidth;
    }

    void Start()
    {
        kineticDamage.damage = kineticDmg;
        kineticDamage.type = "kinetic";
        kineticDamage.source = damageSource;
    }

    void SetupTrail()
    {
        trail.time = trailTime;
        trail.startWidth = trailStartWidth;
        trail.endWidth = trailEndWidth;
        trail.minVertexDistance = 0.05f;
        trail.emitting = true;

        // Simple color fade
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

    void FixedUpdate()
    {
        Debug.DrawLine(transform.position, previousPosition, isInAir ? Color.blue : Color.red, 5f);
        previousPosition = transform.position;

        // Check for low velocity to trigger sinking (only if sinking is enabled and not already sinking and not in air)
        if (enableSinking && !isInAir && !isSinking && !isFading)
        {
            if (rb.linearVelocity.magnitude < velocityThreshold)
            {
                StartSinking();
            }
        }

        // Handle trail fading after collision
        if (isFading)
        {
            float timeSinceCollision = Time.time - collisionTime;
            
            if (timeSinceCollision < fadeDuration)
            {
                // Calculate fade factor using animation curve
                float fadeFactor = fadeCurve.Evaluate(timeSinceCollision / fadeDuration);
                
                // Progressively shorten and thin the trail
                trail.time = Mathf.Lerp(originalTrailTime, 0f, 1f - fadeFactor);
                trail.startWidth = Mathf.Lerp(originalStartWidth, 0f, 1f - fadeFactor);
            }
            else
            {
                // Fade complete - disable trail renderer
                trail.enabled = false;
                isFading = false;
            }
        }

        // Handle sinking
        if (isSinking)
        {
            float sinkProgress = (Time.time - sinkStartTime) / sinkDuration;
            
            if (sinkProgress < 1f)
            {
                // Apply sink curve
                float curvedProgress = sinkCurve.Evaluate(sinkProgress);
                
                // Move projectile down
                Vector3 newPosition = Vector3.Lerp(sinkStartPosition, sinkTargetPosition, curvedProgress);
                transform.position = newPosition;
            }
            else
            {
                // Sink complete - destroy object
                Destroy(gameObject);
            }
        }

        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }


    void OnCollisionEnter(Collision collision)
    {
        // Debug.Log("First collision with: " + collision.collider.name);

        IDamagable dmg = collision.gameObject.GetComponent<IDamagable>();

        if (isInAir && dmg != null){ 
            dmg.Damage(kineticDamage);
        }

        ContactPoint contact = collision.contacts[0];
        Vector3 hitPoint = contact.point;

        Debug.DrawLine(hitPoint, hitPoint + Vector3.up * 0.2f, Color.green, 5f);
        
        isInAir = false;
        rb.linearDamping = linearDamping;
        rb.angularDamping = angularDamping;

        // Start the trail fade process
        StartTrailFade();
    }
    
    void StartTrailFade()
    {
        collisionTime = Time.time;
        isFading = true;
        
        // Ensure we have the current values as baseline
        originalTrailTime = trail.time;
        originalStartWidth = trail.startWidth;
    }
    
    void StartSinking()
    {
        // Only start sinking if it's enabled
        if (!enableSinking) return;
        
        isSinking = true;
        sinkStartTime = Time.time;
        sinkStartPosition = transform.position;
        
        // Calculate sink target (down by sinkDistance)
        sinkTargetPosition = sinkStartPosition + Vector3.down * sinkDistance;
        
        // Disable Rigidbody to stop physics simulation
        rb.isKinematic = true; // This disables physics without removing the component
        // Alternative: rb.detectCollisions = false; if you want to disable collisions
        
        // Optional: Disable collider to prevent further interactions
        if (projectileCollider != null)
        {
            projectileCollider.enabled = false;
        }
        
        // Stop trail emission during sinking (optional)
        trail.emitting = false;
        
        // Debug.Log("Projectile started sinking...");
    }
}