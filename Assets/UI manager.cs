using UnityEngine;

public class UImanager : MonoBehaviour
{
// on game over show game over ui document
    public GameObject gameOverScreen;
    private void OnEnable()
    {
        GameEvents.OnGameOver.Subscribe(ShowGameOverScreen);
    }
    private void OnDisable()
    {
        GameEvents.OnGameOver.Unsubscribe(ShowGameOverScreen);
    }
    private void ShowGameOverScreen()
    {
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
            var gameOverUI = gameOverScreen.GetComponent<GameOverScreen>();
            if (gameOverUI != null)
            {
                gameOverUI.InitializeUI();
            }
        }
        else
        {
            Debug.LogError("UImanager: No Game Over Screen assigned!");
        }
    }
}
