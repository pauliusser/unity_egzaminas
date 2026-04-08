using UnityEngine;
using UnityEngine.InputSystem;

public class Turretspawner : MonoBehaviour, IClickable
{
    public GameObject turretPrefab;
    public LayerMask platformLayer;
    public int price = 50;
    public int moneyLeft = 0;

    void OnEnable()
    {
        GameEvents.OnGuiUpdate.Subscribe(UpdateMoney);
    }
    void OnDisable()
    {
        GameEvents.OnGuiUpdate.Unsubscribe(UpdateMoney);
    }
    void UpdateMoney(int money)
    {
        moneyLeft = money;
        Debug.Log($"Money updated: {moneyLeft}");
    }

    public GameObject SpawnTurret()
    {
        GameObject turret = Instantiate(turretPrefab, transform.position, transform.rotation);

        return turret;
    }

    void IClickable.OnClick()
    {
        if (moneyLeft >= price)
        {
            SpawnTurret();
            Destroy(gameObject);
            GameEvents.OnScored.Invoke(-price);
        }
    }
    private void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;
        bool hitSomething;


        hitSomething = Physics.Raycast(ray, out hit, Mathf.Infinity, platformLayer);


        if (hitSomething)
        {
            if (((1 << hit.collider.gameObject.layer) & platformLayer) != 0)
            {
                SpawnTurret();
                Destroy(hit.transform.gameObject);
            }
        }
    }
}