using UnityEngine;

public interface ITurretControllable
{
    /// <summary>The current target the turret should aim at.</summary>
    Transform Target { get; set; }

    /// <summary>Current selected projectile index (0‑based).</summary>
    int CurrentProjectileIndex { get; }

    /// <summary>Total number of projectile types available.</summary>
    int ProjectileCount { get; }

    /// <summary>Angular error between gun forward and direction to aim point (degrees).</summary>
    float AimError { get; }

    /// <summary>Trigger a single shot.</summary>
    void Fire();

    /// <summary>Add to the pitch offset (positive = up). Typically from scroll wheel.</summary>
    void AddPitchDelta(float delta);

    /// <summary>Set the selected projectile index (0‑based).</summary>
    void SetProjectileIndex(int index);
}