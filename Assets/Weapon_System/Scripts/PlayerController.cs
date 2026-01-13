using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference moveAction;
    public InputActionReference runAction;  // 달리기 버튼 입력

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float runMultiplier = 2f;        // 달리기 시 속도 배율
    public Transform headTransform;

    [Header("Stamina UI")]
    public Image staminaBar;                 // 좌하단 스테미너 UI Image (Filled)

    [Header("Stamina Settings")]
    public float maxStamina = 3f;            // 최대 달리기 시간 (초)
    public float staminaRecoverRate = 1f;    // 초당 회복 속도

    [Header("EXP UI")]
    public Image expBar;

    private Rigidbody rb;
    private Vector2 input;
    private bool isRunning = false;
    public float currentStamina;
    private bool canRun = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        currentStamina = maxStamina;
    }

    void OnEnable()
    {
        moveAction.action.Enable();
        runAction.action.Enable();

        runAction.action.performed += ctx => { if (canRun) isRunning = true; };
        runAction.action.canceled += ctx => isRunning = false;
    }

    void OnDisable()
    {
        moveAction.action.Disable();
        runAction.action.Disable();

        runAction.action.performed -= ctx => { if (canRun) isRunning = true; };
        runAction.action.canceled -= ctx => isRunning = false;
    }

    void Update()
    {
        // 이동 입력 가져오기
        input = moveAction.action.ReadValue<Vector2>();

        // 스테미너 감소/회복 처리
        if (isRunning && input.sqrMagnitude > 0.01f)
        {
            currentStamina -= Time.deltaTime;
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                isRunning = false;
                canRun = false; // 스테미너 소진
            }
        }
        else
        {
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRecoverRate * Time.deltaTime;
                if (currentStamina >= maxStamina)
                {
                    currentStamina = maxStamina;
                    canRun = true; // 회복 완료
                }
            }
        }

        // UI 갱신
        UpdateStaminaUI();
    }
    void FixedUpdate()
    {
        // 시점 기준 방향 계산
        Vector3 headForward = headTransform.forward;
        Vector3 headRight = headTransform.right;

        headForward.y = 0;
        headRight.y = 0;

        headForward.Normalize();
        headRight.Normalize();

        Vector3 direction = headForward * input.y + headRight * input.x;

        if (direction.sqrMagnitude > 0.01f)
        {
            float speed = moveSpeed * (isRunning ? runMultiplier : 1f);
            Vector3 move = direction * speed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + move);
        }
    }

    void UpdateStaminaUI()
    {
        if (staminaBar != null)
            staminaBar.fillAmount = currentStamina / maxStamina;
    }
    public void UpdateExpUI(float currentExp, float maxExp)
    {
        if (expBar != null)
            expBar.fillAmount = currentExp / maxExp;
    }
}
