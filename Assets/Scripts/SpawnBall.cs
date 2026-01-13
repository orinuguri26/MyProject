using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class SpawnBall : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject ballPrefab;     // 공 프리팹
    public Transform spawnPoint;      // RightHand Controller 안의 SpawnPoint 연결
    public InputActionReference triggerAction;
    public float throwForce = 2f;
    public float spawnCooldown = 0.5f; // 생성 간격 (초)
    private float lastSpawnTime = 0f;
    private void OnEnable()
    {
        triggerAction.action.performed += OnTriggerPressed;
        triggerAction.action.Enable();
    }

    private void OnDisable()
    {
        triggerAction.action.performed -= OnTriggerPressed;
        triggerAction.action.Disable();
    }

    private void OnTriggerPressed(InputAction.CallbackContext context)
    {
        Debug.Log("[RightHand] Trigger Pressed!");
        Spawn();
    }

    void Spawn()
    {
        if (ballPrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("ballPrefab or spawnPoint not assigned!");
            return;
        }

        if (Time.time - lastSpawnTime < spawnCooldown)
            return; // 쿨다운 안 지났으면 생성 안 함

        lastSpawnTime = Time.time;
        // SpawnPoint 위치에서 공 생성
        GameObject ball = Instantiate(ballPrefab, spawnPoint.position, spawnPoint.rotation);
        Debug.Log("Ball Spawned in front of hand!");

        // Rigidbody 힘 적용 (던지는 느낌)
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = spawnPoint.forward * throwForce;
            //rb.AddForce(spawnPoint.forward * throwForce, ForceMode.VelocityChange);
            //rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.VelocityChange);
        }
    }

}