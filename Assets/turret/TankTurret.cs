using UnityEngine;

[System.Serializable]
public class ProjectileConfig
{
    public GameObject prefab;
    public int damage;
    public bool isKinetic;   // true = kinetic (fixed velocity), false = mortar (high arc)
}

public class TankTurret : MonoBehaviour, ITurretControllable
{
    [Header("Turret Control")]
    public bool isEnabled = true;
    public Transform shotTarget;                // Set externally (e.g., mouse target)
    public int projectileIndex = 0;              // Current selected projectile (0‑based)

    [Header("References")]
    public Transform turretPivot;                // rotates around Y (turret)
    public Transform gunPivot;                   // rotates around X (gun)
    public Transform firePoint;                   // projectile spawn point (child of gun)

    [Header("Turret Motion")]
    public float maxTurretSpeed = 60f;            // degrees per second
    public float deadZone = 0.001f;

    [Header("Gun Limits")]
    public float minGunPitch = -10f;              // down (negative)
    public float maxGunPitch = 25f;               // up (positive)

    [Header("Kinetic Mode (Fixed Velocity)")]
    public float fixedVelocity = 50f;

    [Header("Mortar Mode (Adaptive Velocity)")]
    public float maxMortarVelocity = 100f;
    public float defaultAimOffset = 2f;        // initial height offset
    public float minAimOffset = 0.5f;
    public float maxAimOffset = 10f;

    [Header("Projectile Types")]
    public GameObject damageSourceObject;
    public ProjectileConfig[] projectiles;         // array of available projectile types
    public float damageMultiplier = 1f;

    [Header("Debug")]
    public bool drawTrajectory = true;
    public Color trajectoryColor = Color.green;
    public Color aimLineColor = Color.yellow;
    public Color targetToAimColor = Color.cyan;
    public float trajectoryStep = 0.1f;
    public float maxTrajectoryTime = 5f;

    [Header("Balistic trajectory render")]
    public Vector3 forceVector;
    public bool isBalisticLine = true;

    // Internal state
    private float mortarOffset;                    // current vertical offset (mortar mode only)
    private Vector3 aimPoint;                       // world position the gun should point at
    private ProjectileConfig currentProjectile;

    // Interface properties
    public Transform Target
    {
        get => shotTarget;
        set => shotTarget = value;
    }

    public int CurrentProjectileIndex => projectileIndex;
    public int ProjectileCount => projectiles?.Length ?? 0;

    public float AimError
    {
        get
        {
            if (gunPivot == null || aimPoint == null) return 180f;
            Vector3 toAim = (aimPoint - gunPivot.position).normalized;
            return Vector3.Angle(gunPivot.forward, toAim);
        }
    }

    private void Start()
    {
        mortarOffset = defaultAimOffset;

        if (projectiles != null && projectiles.Length > 0)
            SwitchProjectile(projectileIndex);
    }

    private void FixedUpdate()
    {
        if(!isEnabled) return;
        if (shotTarget == null && turretPivot != null && gunPivot != null)
        {
            // Debug.Log("rotating home");
            if (turretPivot.localEulerAngles.y != 0) RotateTurretToHome();
            if (gunPivot.localEulerAngles.x != 0) RotateGunToHome();
        }

        if (shotTarget == null || currentProjectile == null) return;

        UpdateAimPoint();
        RotateTurretTowardAimPoint();
        RotateGunTowardAimPoint();

        Debug.DrawLine(firePoint.position, aimPoint, aimLineColor);
        Debug.DrawLine(shotTarget.position, aimPoint, targetToAimColor);

        if (currentProjectile.isKinetic)
        {
            Vector3 vel = firePoint.forward * fixedVelocity;
            DrawTrajectory(firePoint.position, vel);
        }
        else
        {
            float speed = ComputeMortarSpeed();
            Vector3 vel = firePoint.forward * speed;
            DrawTrajectory(firePoint.position, vel);
        }
        forceVector = CalculateForceVector();

    }

    // ---------- Interface Methods ----------
    public void Fire()
    {
        if (!isEnabled) return;
        if (firePoint == null || currentProjectile == null || currentProjectile.prefab == null) return;

        GameObject projectile = Instantiate(currentProjectile.prefab, firePoint.position, firePoint.rotation);

        if (currentProjectile.isKinetic)
        {
            var kinetic = projectile.GetComponent<KineticProjectile>();
            if (kinetic != null) 
            {
                kinetic.kineticDmg = (int) (currentProjectile.damage * damageMultiplier);
                kinetic.damageSource = damageSourceObject;
            }
        }
        else
        {
            var explosive = projectile.GetComponent<ExplosiveProjectile>();
            if (explosive != null)
            {
                explosive.blastDmg = (int) (currentProjectile.damage * damageMultiplier);
                explosive.damageSource = damageSourceObject;
            }
        }

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb == null) return;

        rb.linearVelocity = GravityAjustedForceVector(forceVector);
    }

    public void AddPitchDelta(float delta)
    {
        if (currentProjectile == null) return;
        if (!currentProjectile.isKinetic) // only mortar mode
        {
            mortarOffset += delta;
            mortarOffset = Mathf.Clamp(mortarOffset, minAimOffset, maxAimOffset);
        }
    }

    public void SetProjectileIndex(int index)
    {
        if (index < 0 || index >= projectiles.Length) return;
        if (index != projectileIndex)
            SwitchProjectile(index);
    }

    // ---------- Internal Methods ----------
    private void SwitchProjectile(int newIndex)
    {
        currentProjectile = projectiles[newIndex];
        projectileIndex = newIndex;
        Debug.Log($"Switched to projectile {newIndex} ({(currentProjectile.isKinetic ? "Kinetic" : "Mortar")})");
    }

    private Vector3 CalculateForceVector()
    {
        if (currentProjectile.isKinetic)
            return firePoint.forward * fixedVelocity;
        else
        {
            float speed = ComputeMortarSpeed();
            return firePoint.forward * speed;
        }
    }

    private Vector3 GravityAjustedForceVector(Vector3 fv)
    {
        fv.y += 0.5f * Mathf.Abs(Physics.gravity.y) * Time.fixedDeltaTime;
        return fv;
    }

    // ---------- Aiming Logic (mortar offset stable) ----------
    private void UpdateAimPoint()
    {
        if (!currentProjectile.isKinetic) // Mortar mode
        {
            Vector3 gunPos = gunPivot.position;
            Vector3 targetPos = shotTarget.position;
            Vector3 toTarget = targetPos - gunPos;
            float R = new Vector3(toTarget.x, 0, toTarget.z).magnitude;

            if (R > 0.001f)
            {
                float aimHeight = targetPos.y + mortarOffset;
                float heightDiff = aimHeight - gunPos.y;
                float requiredPitch = Mathf.Atan2(heightDiff, R) * Mathf.Rad2Deg;
                requiredPitch = Mathf.Clamp(requiredPitch, minGunPitch, maxGunPitch);

                float aimY = gunPos.y + R * Mathf.Tan(requiredPitch * Mathf.Deg2Rad);

                // Ensure we aim above the target (mortar should arc over)
                if (aimY < targetPos.y + 0.5f)
                    aimY = targetPos.y + 0.5f;

                aimPoint = new Vector3(targetPos.x, aimY, targetPos.z);
                // Do NOT update mortarOffset here – it remains user‑controlled.
            }
            else
            {
                aimPoint = targetPos + Vector3.up * mortarOffset;
            }
        }
        else // Kinetic mode
        {
            aimPoint = ComputeKineticAimPoint();
        }
    }

    private Vector3 ComputeKineticAimPoint()
    {
        Vector3 start = firePoint.position;
        Vector3 target = shotTarget.position;
        float g = Mathf.Abs(Physics.gravity.y);
        float V = fixedVelocity;

        Vector3 toTarget = target - start;
        float R = new Vector3(toTarget.x, 0, toTarget.z).magnitude;
        float H = toTarget.y;

        if (R < 0.001f)
        {
            float sign = H > 0 ? 1 : -1;
            return start + Vector3.up * (start.y + sign * 10f);
        }

        float A = g * R * R / (2f * V * V);
        float B = -R;
        float C = A + H;

        float discriminant = B * B - 4f * A * C;

        if (discriminant < 0)
        {
            // Debug.LogWarning("Target unreachable. Aiming directly.");
            return target;
        }

        float sqrtD = Mathf.Sqrt(discriminant);
        float tan1 = (-B + sqrtD) / (2f * A);
        float tan2 = (-B - sqrtD) / (2f * A);
        float tanTheta = Mathf.Min(tan1, tan2);

        float aimY = start.y + R * tanTheta;
        return new Vector3(target.x, aimY, target.z);
    }

    private float ComputeMortarSpeed()
    {
        Vector3 start = firePoint.position;
        Vector3 target = shotTarget.position;
        Vector3 dir = aimPoint;

        float deltaX = target.x - start.x;
        float deltaZ = target.z - start.z;
        float horDist = Mathf.Sqrt(deltaX * deltaX + deltaZ * deltaZ);
        float vertH = dir.y - start.y;
        float deltaH = dir.y - target.y;

        if (deltaH <= 0)
            return maxMortarVelocity;

        float gravity = Mathf.Abs(Physics.gravity.y);
        return Mathf.Sqrt(gravity * (horDist * horDist + vertH * vertH) / (2 * deltaH));
    }

    // ---------- Rotation Methods (unchanged) ----------
    public void RotateTurretToHome()
    {
        Vector3 localEuler = turretPivot.localEulerAngles;
        float error = Mathf.Abs(localEuler.y);
        if (error > 5f)
        {
            float side = localEuler.y % 360 < 180 ? -1 : 1;
            float rotationStep = maxTurretSpeed * side * Time.fixedDeltaTime;
            turretPivot.Rotate(0f, rotationStep, 0f, Space.Self);
        }
        else
        {
            turretPivot.localEulerAngles = Vector3.zero;
        }
    }
    public void RotateGunToHome()
    {
        Vector3 localEuler = gunPivot.localEulerAngles;
        float error = Mathf.Abs(localEuler.x);
        // Debug.Log(error);
        if (error > 5f)
        {
            float side = localEuler.x % 360 < 180 ? -1 : 1;
            float rotationStep = maxTurretSpeed * side * Time.fixedDeltaTime * 0.5f;
            gunPivot.Rotate(rotationStep, 0f, 0f, Space.Self);
        }
        else
        {
            gunPivot.localEulerAngles = Vector3.zero;
        }
    }
    private void RotateTurretTowardAimPoint()
    {
        Vector3 toAim = aimPoint - turretPivot.position;
        Vector3 flatToAim = Vector3.ProjectOnPlane(toAim, turretPivot.up);

        if (flatToAim.sqrMagnitude < 0.0001f) return;

        flatToAim.Normalize();

        float alignment = Vector3.Dot(turretPivot.forward, flatToAim);
        float side = Vector3.Dot(turretPivot.right, flatToAim);

        if (alignment > 1f - deadZone)
        {
            AimTurretDirectly(flatToAim);
            return;
        }

        float rotationStrength = Mathf.Clamp01((1f - alignment) * 5f + 0.5f);
        float rotationSpeed = rotationStrength * maxTurretSpeed;
        float rotationStep = rotationSpeed * Mathf.Sign(side) * Time.fixedDeltaTime;

        turretPivot.Rotate(0f, rotationStep, 0f, Space.Self);
    }


    private void AimTurretDirectly(Vector3 flatDir)
    {
        Quaternion lookRot = Quaternion.LookRotation(flatDir, turretPivot.up);
        Vector3 localEuler = turretPivot.localEulerAngles;
        Quaternion localLook = Quaternion.Inverse(turretPivot.parent ? turretPivot.parent.rotation : Quaternion.identity) * lookRot;
        localEuler.y = localLook.eulerAngles.y;
        turretPivot.localEulerAngles = localEuler;
    }

    private void RotateGunTowardAimPoint()
    {
        if (gunPivot == null) return;

        Vector3 toAim = aimPoint - gunPivot.position;
        Vector3 flatToAim = Vector3.ProjectOnPlane(toAim, gunPivot.right);

        if (flatToAim.sqrMagnitude < 0.0001f) return;

        flatToAim.Normalize();

        float alignment = Vector3.Dot(gunPivot.forward, flatToAim);
        float side = Vector3.Dot(gunPivot.up, flatToAim);

        if (alignment > 1f - deadZone)
        {
            AimGunDirectly();
            return;
        }

        if (alignment < 0) return;

        float rotationStrength = Mathf.Clamp01((1f - alignment) * 5f + 0.5f);
        float rotationSpeed = rotationStrength * maxTurretSpeed;
        float rotationStep = rotationSpeed * Mathf.Sign(side) * Time.fixedDeltaTime;

        Vector3 localEuler = gunPivot.localEulerAngles;
        float currentPitch = NormalizeAngle(localEuler.x);
        float newPitch = Mathf.Clamp(currentPitch - rotationStep, -maxGunPitch, -minGunPitch);
        localEuler.x = newPitch;
        gunPivot.localEulerAngles = localEuler;
    }

    private void AimGunDirectly()
    {
        Vector3 toAim = aimPoint - gunPivot.position;
        Vector3 gunRight = gunPivot.right;
        Vector3 gunUp = gunPivot.up;

        Vector3 flatDir = Vector3.ProjectOnPlane(toAim, gunRight);
        if (flatDir.sqrMagnitude < 0.0001f) return;
        flatDir.Normalize();

        Quaternion targetRot = Quaternion.LookRotation(flatDir, gunUp);
        Quaternion localTarget = Quaternion.Inverse(gunPivot.parent.rotation) * targetRot;
        float targetPitch = NormalizeAngle(localTarget.eulerAngles.x);
        targetPitch = Mathf.Clamp(targetPitch, -maxGunPitch, -minGunPitch);

        Vector3 localEuler = gunPivot.localEulerAngles;
        localEuler.x = targetPitch;
        gunPivot.localEulerAngles = localEuler;
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }

    private void DrawTrajectory(Vector3 start, Vector3 velocity)
    {
        if (!drawTrajectory || shotTarget == null) return;

        Vector3 toTarget = shotTarget.position - start;
        float R = new Vector3(toTarget.x, 0, toTarget.z).magnitude;
        if (R < 0.001f) return;

        float horizSpeed = new Vector3(velocity.x, 0, velocity.z).magnitude;
        if (horizSpeed < 0.001f) return;

        float tFlight = R / horizSpeed;
        tFlight = Mathf.Min(tFlight, maxTrajectoryTime);

        Vector3 prevPos = start;
        float step = trajectoryStep;
        Vector3 gravity = Physics.gravity;

        for (float t = step; t <= tFlight; t += step)
        {
            Vector3 currentPos = start + velocity * t + 0.5f * gravity * t * t;
            Debug.DrawLine(prevPos, currentPos, trajectoryColor);
            prevPos = currentPos;
        }

        Vector3 finalPos = start + velocity * tFlight + 0.5f * gravity * tFlight * tFlight;
        Debug.DrawLine(prevPos, finalPos, trajectoryColor);
    }
}