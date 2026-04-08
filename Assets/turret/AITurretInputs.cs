using UnityEngine;
public class AITurretController : MonoBehaviour
{
    public GameObject tankInstance;
    public GameObject turret;
    private ITurretControllable turretController;
    private float fireCooldown;
    private Transform currentTarget;
    private Transform gunBarrel;
    public bool HasTarget => currentTarget != null;

    private void Awake()
    {
        turretController = turret.GetComponent<ITurretControllable>();
        gunBarrel = turret.GetComponent<TankTurret>().firePoint;
        if (turretController == null)
            Debug.LogError("AITurretInputs: No ITurretControllable found.");
    }

    public void SetProjectileIndex(int index)
    {
        if (turretController == null) return;
        int count = turretController.ProjectileCount;
        if (index >= count || index < 0) return;
        turretController.SetProjectileIndex(index);
    }

    public void AimTurretToCurrentTarget()
    {
        if (turretController == null) return;
        turretController.Target = currentTarget;
    }

    public void FireWhenAligned(float aimTolerance = 5f, float fireRate = 2f)
    {
        if (turretController == null) return;
        if (currentTarget != null)
        {
            fireCooldown -= Time.deltaTime;
            if (fireCooldown <= 0f && turretController.AimError <= aimTolerance)
            {
                turretController.Fire();
                fireCooldown = 1f / fireRate;
            }
        }
        else
        {
            fireCooldown = 0f;
        }
    }

    public bool FindNearestTarget(LayerMask targetLayerMask, LayerMask obstacleMask, float detectionRadius = 20f)
    {
        if (turretController == null) return false;

        Collider[] hits = new Collider[20];
        Vector3 origin = turret.transform.position;
        int count = Physics.OverlapSphereNonAlloc(origin, detectionRadius, hits, targetLayerMask);

        if (count == 0)
        {
            currentTarget = null;
            return false;
        }

        Transform nearest = null;
        float nearestDistSqr = float.MaxValue;
        float projectileRadius = 0.1f;

        for (int i = 0; i < count; i++)
        {
            Transform candidate = hits[i].transform;
            Vector3 direction = candidate.position - origin;
            float distance = direction.magnitude;

            if (Physics.SphereCast(origin, projectileRadius, direction.normalized,
                    out RaycastHit hitInfo, distance, obstacleMask))
            {
                if (hitInfo.transform != candidate) continue;
            }

            float distSqr = direction.sqrMagnitude;
            if (distSqr < nearestDistSqr)
            {
                nearestDistSqr = distSqr;
                nearest = candidate;
            }
        }

        currentTarget = nearest;
        return currentTarget != null;
    }
}