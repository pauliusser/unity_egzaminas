using System.Data;
using UnityEngine;

public class EnemyTankBehaviourFSM : MonoBehaviour
{
    [Header("Initialisation")]
    private IState<EnemyTankBehaviourFSM> currentState; 
    public GameObject tankTurret;
    public AITurretController turretInputs;    

    [Header("current state")]
    public string state = "";

    [Header("Enemy detection Settings")]
    public float detectionRadius = 8f;
    public LayerMask targetLayerMask;
    public LayerMask obstacleMask;
    
    [Header("Firing Settings")]
    public float fireRate = 2f;
    public int defaultProjectileIndex = 0;
    public float aimTolerance = 5f; 
    public float fireRange = 10f;
    void Start()
    {
        currentState = new AttackState();
    }
    void Update()
    {
        currentState = currentState.DoState(this);
    }    
}