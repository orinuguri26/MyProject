using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Image fill;

    public void SetMaxHealth(float max)
    {
        fill.fillAmount = 1f;
    }

    public void SetHealth(float value)
    {
        fill.fillAmount = Mathf.Clamp01(value);
    }
}
