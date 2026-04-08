public static class GameEvents
{
    //public static Event OnGameStart = new Event();
    //public static Event OnPauseToggled = new Event();
    //public static Event OnGameOver = new Event();
    //public static Event<int> OnFinalScore = new Event<int>();
    //public static Event OnEnterMainMenu = new Event();
    //public static Event OnVictory = new Event();


    //public static Event OnRefreshHUD = new Event();
    //public static Event<float> OnHealthUpdate = new Event<float>();
    //public static Event<float> OnShieldUpdate = new Event<float>();
    //public static Event<int> OnLivesUpdate = new Event<int>();
    //public static Event<int> OnScoreUpdate = new Event<int>();
    //public static Event<float> OnCapacitorUpdate = new Event<float>();
    //public static Event<float> OnBatteryUpdate = new Event<float>();

    public static Event<int> OnGuiUpdate = new Event<int>();
    public static Event<int> OnScored = new Event<int>();
    public static Event<int> OnSpend = new Event<int>();
    public static Event OnGameOver = new Event();

}