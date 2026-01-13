using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStartManager : MonoBehaviour
{
    public void RestartGame()
    {
        GamePauseManager.ReleasePause();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }
}
