using UnityEngine;

public class GamePauseManager : MonoBehaviour
{
    private static int pauseCount = 0;

    public static void RequestPause()
    {
        pauseCount++;
        Time.timeScale = 0f;
    }

    public static void ReleasePause()
    {
        pauseCount = Mathf.Max(0, pauseCount - 1);
        if (pauseCount == 0)
            Time.timeScale = 1f;
    }

    public static bool IsPaused => pauseCount > 0;
}
