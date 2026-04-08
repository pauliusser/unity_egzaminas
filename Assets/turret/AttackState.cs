public class AttackState : IState<EnemyTankBehaviourFSM>
{
    private bool isTarget = true;

    public IState<EnemyTankBehaviourFSM> DoState(EnemyTankBehaviourFSM machine)
    {
        if (machine.state != "Attack state") machine.state = "Attack state";
        DoAttackBehaviour(machine);

        return isTarget ? this : new IdleState();
    }

    void DoAttackBehaviour(EnemyTankBehaviourFSM machine)
    {
        // Wider radius — tank is on alert
        isTarget = machine.turretInputs.FindNearestTarget(
            machine.targetLayerMask,
            machine.obstacleMask,
            machine.fireRange
        );

        machine.turretInputs.SetProjectileIndex(machine.defaultProjectileIndex);
        machine.turretInputs.AimTurretToCurrentTarget();
        machine.turretInputs.FireWhenAligned(machine.aimTolerance, machine.fireRate);
    }
}