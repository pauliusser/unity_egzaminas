using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int money = 100;

    private void OnEnable()
    {
        GameEvents.OnScored.Subscribe(AddMoney);
        GameEvents.OnSpend.Subscribe(SpendMoney);
    }
    private void OnDisable()
    {
        GameEvents.OnScored.Unsubscribe(AddMoney);
        GameEvents.OnSpend.Unsubscribe(SpendMoney);
    }
    void Start()
    {
        GameEvents.OnGuiUpdate.Invoke(money);
    }
    private void AddMoney(int amount)
    {
        money += amount;
        GameEvents.OnGuiUpdate.Invoke(money);
    }
    private void SpendMoney(int amount)
    {
        Debug.Log($"Spending {amount} money. Current money: {money}");
        money -= amount;
        GameEvents.OnGuiUpdate.Invoke(money);
    }
}
