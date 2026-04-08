using UnityEngine;
using UnityEngine.InputSystem;

public class Turretspawner : MonoBehaviour, IClickable
{
    public GameObject turretPrefab;
    public LayerMask platformLayer;

    public GameObject SpawnTurret()
    {
        GameObject turret = Instantiate(turretPrefab, transform.position, transform.rotation);

        return turret;
    }

    void IClickable.OnClick()
    {
        SpawnTurret();
        Destroy(gameObject);
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
                //Vector3 camPos = Camera.main.transform.position;
                //Vector3 colliderPos = hit.collider.transform.position;
                //Vector3 direction = (hit.point - camPos).normalized;
                //float distance = (camPos - colliderPos).magnitude;
                //transform.position = camPos + direction * distance;
                SpawnTurret();
                Destroy(hit.transform.gameObject);
            }
        }
    }
}