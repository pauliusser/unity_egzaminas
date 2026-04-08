using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Explosion : MonoBehaviour
{
    [Header("Damage Settings")]
    public float blastRadius = 5f;
    public float maxDamage = 100f;
    public string damageType = "explosive";
    public AnimationCurve damageFalloff = AnimationCurve.Linear(0, 1, 1, 0); // Distance 0-1 mapped to multiplier 1-0
    
    [Header("Physics Force")]
    public float maxForce = 500f;
    public float upwardsModifier = 0f;
    public ForceMode forceMode = ForceMode.Impulse;
    public AnimationCurve forceFalloff = AnimationCurve.Linear(0, 1, 1, 0);
    
    [Header("Targeting")]
    public LayerMask affectedLayers = -1; // Everything by default
    public bool requireLineOfSight = false; // Check if objects are behind cover
    public LayerMask obstructionLayers; // What counts as cover (walls, etc)
    
    [Header("Self & Friendly")]
    public GameObject sourceObject; // Who caused the explosion (for damage attribution)
    public bool damageSource = true; // Should the source object be damaged?
    public bool damageAllies = true; // Should objects on same team be damaged?
    
    [Header("Debug")]
    public bool debugDraw = true;
    public Color gizmoColor = new Color(1f, 0.5f, 0f, 0.3f);
    public GameObject shockwavePrefab; // Assign the sphere prefab with the Shockwave material
    public float shockwaveDuration = 0.5f;

    
    // Optional: Called when explosion happens (for VFX, sound, etc)
    public System.Action<Vector3> OnExploded;
    
    // Cache for line of sight checks
    private RaycastHit[] lineOfSightHits = new RaycastHit[1];
    
    IEnumerator SpawnShockwave(Vector3 position, float blastRadius)
    {
        GameObject shockwave = Instantiate(shockwavePrefab, position, Quaternion.identity);
        Renderer rend = shockwave.GetComponent<Renderer>();
        Material mat = rend.material; // Note: this creates an instance; fine for one‑off effects

        float elapsed = 0f;
        while (elapsed < shockwaveDuration)
        {
            float t = elapsed / shockwaveDuration;

            // Scale sphere from 0 to blastRadius * 2 (diameter = 2 * radius)
            float scale = Mathf.Lerp(0f, blastRadius * 2f, t);
            shockwave.transform.localScale = Vector3.one * scale;

            // Fade alpha
            float alpha = Mathf.Lerp(1f, 0f, t);
            mat.SetFloat("_Alpha", alpha);

            // Optionally increase distortion strength over time
            // float strength = Mathf.Lerp(0.1f, 0.5f, t);
            // mat.SetFloat("_Strength", strength);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(shockwave);
    }
    public void Explode()
    {
        ExplodeAt(transform.position);
        Debug.Log("explosion request");
    }
    
    public void ExplodeAt(Vector3 position)
    {
        if (shockwavePrefab != null)
        {
            GameObject shockwave = Instantiate(shockwavePrefab, position, Quaternion.identity);
            ShockwaveEffect effect = shockwave.GetComponent<ShockwaveEffect>();
            if (effect != null) effect.Initialize(blastRadius);
        }
        // Find all objects in radius
        Collider[] hitColliders = Physics.OverlapSphere(position, blastRadius, affectedLayers);
        HashSet<GameObject> processedObjects = new HashSet<GameObject>(); // Prevent double-dipping objects with multiple colliders
        
        foreach (Collider hit in hitColliders)
        {
            GameObject obj = hit.gameObject;
            
            // Skip if we've already processed this object
            if (processedObjects.Contains(obj)) continue;
            
            // Skip source object if we shouldn't damage it
            if (!damageSource && obj == sourceObject) continue;
            
            // Get closest point on collider for accurate distance calculation
            Vector3 closestPoint = hit.ClosestPoint(position);
            float distance = Vector3.Distance(position, closestPoint);
            
            // Skip if outside radius (closest point might be slightly outside due to collider shape)
            if (distance > blastRadius) continue;
            
            // Calculate falloff (0-1 based on distance)
            float t = distance / blastRadius; // 0 at center, 1 at edge
            float damageMultiplier = damageFalloff.Evaluate(t);
            float forceMultiplier = forceFalloff.Evaluate(t);
            
            // Line of sight check (for cover)
            if (requireLineOfSight && !HasLineOfSight(position, closestPoint))
            {
                if (debugDraw) Debug.DrawLine(position, closestPoint, Color.red, 2f);
                continue;
            }
            
            // Apply damage
            ApplyDamage(obj, damageMultiplier, position, distance);
            
            // Apply force
            ApplyForce(obj, hit, forceMultiplier, position);
            
            processedObjects.Add(obj);
        }
        
        // Trigger any callbacks
        OnExploded?.Invoke(position);
        
        // Debug visualization
        if (debugDraw)
        {
            Debug.Log($"Explosion at {position} affected {processedObjects.Count} objects");
            Debug.DrawRay(position, Vector3.up * 0.5f, Color.red, 2f);
        }
    }
    
    private bool HasLineOfSight(Vector3 from, Vector3 to)
    {
        Vector3 direction = to - from;
        float distance = direction.magnitude;
        
        // Check if anything blocks the line of sight
        int hits = Physics.RaycastNonAlloc(from, direction.normalized, lineOfSightHits, distance, obstructionLayers);
        
        return hits == 0; // No obstructions = has line of sight
    }
    
    private void ApplyDamage(GameObject obj, float multiplier, Vector3 blastCenter, float distance)
    {
        if (obj.TryGetComponent<IDamagable>(out IDamagable damagable))
        {
            Debug.Log("damage request");
            Damage.Request request = new Damage.Request
            {
                damage = Mathf.RoundToInt(maxDamage * multiplier),
                type = damageType,
                source = sourceObject != null ? sourceObject : gameObject
            };
            
            damagable.Damage(request);
            
            if (debugDraw)
            {
                Debug.Log($"Damaged: {obj.name} for {request.damage} ({multiplier:P0} of max) at distance {distance:F1}");
            }
        }
    }
    
    private void ApplyForce(GameObject obj, Collider collider, float multiplier, Vector3 blastCenter)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Direction from blast to object
            Vector3 direction = (obj.transform.position - blastCenter).normalized;
            
            // Calculate force
            float forceMagnitude = maxForce * multiplier;
            Vector3 force = direction * forceMagnitude;
            
            // Add upward modifier
            if (upwardsModifier != 0f)
            {
                force += Vector3.up * upwardsModifier * multiplier;
            }
            
            // Apply at closest point on collider for realistic effect
            Vector3 closestPoint = collider.ClosestPoint(blastCenter);
            rb.AddForceAtPosition(force, closestPoint, forceMode);
            
            if (debugDraw)
            {
                Debug.DrawRay(blastCenter, direction * forceMagnitude * 0.01f, Color.yellow, 2f);
            }
        }
    }
    
    // Visualize in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, blastRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, blastRadius);
    }
}