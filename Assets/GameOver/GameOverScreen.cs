using UnityEngine;
using UnityEngine.UIElements;

public class GameOverScreen : MonoBehaviour
{
    private Button restartButton;
    private Button mainMenuButton;
    private Label finalScoreLabel;
    private UIDocument uiDocument;
    private bool uiInitialized = false;
    private int? pendingScore = null;
    public bool debug = false;
    
    void Start()
    {
        if (uiDocument == null) 
            uiDocument = GetComponent<UIDocument>();
    }
    
    void OnEnable()
    {
        // Subscribe to events when enabled
        //GameEvents.OnFinalScore.Subscribe(UpdateFinalScore);
    }
    
    void OnDisable()
    {
        // Unsubscribe when disabled
        //GameEvents.OnFinalScore.Unsubscribe(UpdateFinalScore);
        
        // Clean up button events
        //if (restartButton != null)
        //    restartButton.clicked -= RestartClick;
        //if (mainMenuButton != null)
        //    mainMenuButton.clicked -= MainMenuClick;
        
        uiInitialized = false;
    }
    
    // Called by UIManager after enabling the UI Document
    public void InitializeUI()
    {
        if (uiInitialized) return;
        
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                if (debug) Debug.LogError("GameOverScreen: No UIDocument found!");
                return;
            }
        }
        
        if (!uiDocument.enabled)
        {
            if (debug) Debug.LogWarning("GameOverScreen: UIDocument is not enabled");
            return;
        }
        
        if (uiDocument.rootVisualElement == null)
        {
            if (debug) Debug.LogError("GameOverScreen: rootVisualElement is null - UI might not be ready");
            return;
        }

        var root = uiDocument.rootVisualElement;
        
        restartButton = root.Q<Button>("restart-btn");
        if (restartButton != null)
        {
            //restartButton.clicked += RestartClick;
        }
        else
        {
            if (debug) Debug.LogError("GameOverScreen: Could not find 'restart-btn'");
        }
        
        mainMenuButton = root.Q<Button>("MainMenu-btn");
        if (mainMenuButton != null)
        {
            //mainMenuButton.clicked += MainMenuClick;
        }
        else
        {
            if (debug) Debug.LogError("GameOverScreen: Could not find 'MainMenu-btn'");
        }
        
        finalScoreLabel = root.Q<Label>("finalScore-label");
        if (finalScoreLabel == null)
        {
            if (debug) Debug.LogError("GameOverScreen: Could not find 'finalScore-label'");
        }
        
        uiInitialized = true;
        if (debug) Debug.Log("GameOverScreen UI initialized successfully");
        
        // Apply pending score if exists
        if (pendingScore.HasValue)
        {
            UpdateFinalScore(pendingScore.Value);
            pendingScore = null;
        }
    }
    
    void UpdateFinalScore(int finSc)
    {
        // If UI isn't ready yet, store the score for later
        if (!uiInitialized || finalScoreLabel == null)
        {
            if (debug) Debug.Log($"GameOverScreen: UI not ready, storing score {finSc}");
            pendingScore = finSc;
            return;
        }
        
        finalScoreLabel.text = $"Final Score: {finSc:D4}";
        if (debug) Debug.Log($"GameOverScreen: Updated final score to {finSc:D4}");
    }
    
    //private void RestartClick()
    //{
    //    if (GameManager.Instance != null)
    //    {
    //        GameManager.Instance.isRestart = true;
    //        GameEvents.OnGameStart.Invoke();
    //    }
    //}
    
    //private void MainMenuClick()
    //{
    //    GameEvents.OnEnterMainMenu.Invoke();
    //}
}