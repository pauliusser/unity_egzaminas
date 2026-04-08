using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    public InputAction mouseClick;
    private void OnEnable()
    {
        mouseClick.Enable();
        mouseClick.performed += OnClick;
    }
    private void OnDisable()
    {
        mouseClick.Disable();
        mouseClick.performed -= OnClick;
    }
    private void OnClick(InputAction.CallbackContext ctx)
    {
        Debug.Log("Mouse Clicked");
        // when clicked on free platform spawn turret and destroy platform
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            IClickable clickable = hit.collider.GetComponent<IClickable>();
            if (clickable != null)
            {
                clickable.OnClick();
            }
        }
    }
}
