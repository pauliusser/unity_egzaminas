using UnityEngine;
using UnityEngine.UIElements;

public class HUD : MonoBehaviour
{
    private Label money;
    private UIDocument uiDocument;
    private VisualElement root;

    private void OnEnable()
    {
        GameEvents.OnGuiUpdate.Subscribe(OnMoneyUpdate);
    }
    private void OnDisable()
    {
        GameEvents.OnGuiUpdate.Unsubscribe(OnMoneyUpdate);
    }

    private void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        money = root.Q<Label>("Money");
        money.text = "Money: 0";
    }

    void OnMoneyUpdate(int m)
    {
        money.text = $"Money: {m}";
    }
}
