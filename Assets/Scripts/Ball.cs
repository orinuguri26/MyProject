using UnityEngine;

public class Ball : MonoBehaviour
{
    public float lifetime; // 공이 남아있는 시간(초)
    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    string[] destroyTags = {"Enemy", "Object" };
    private void OnCollisionEnter(Collision collision)
    {
        foreach (string t in destroyTags)
        {
            if (collision.gameObject.CompareTag(t))
            {
                Destroy(gameObject);           // 공 제거
                break;
            }
        }
    }
}