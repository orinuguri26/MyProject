using UnityEngine;
using UnityEngine.UI;

public class BGMPlayer : MonoBehaviour
{
    public AudioSource bgmSource;
    public GameObject startButton;

    public void PlayBGM()
    {
        if (!bgmSource.isPlaying)
            bgmSource.Play();
        
        startButton.SetActive(false);
    }
}
