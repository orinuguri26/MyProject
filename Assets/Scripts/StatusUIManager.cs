using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class StatusUIManager : MonoBehaviour
{
    [Header("UI 패널")]
    public GameObject statusPanel;

    [Header("탭 순서대로 배열")]
    public GameObject[] tabs; // 0: 플레이어, 1: 권총, 2: 샷건, 3: 스나이퍼, 4: 오라

    [Header("탭에 연결된 텍스트")]
    public TextMeshProUGUI[] tabTexts; // tabs와 같은 순서대로

    [Header("총기 오브젝트")]
    public GunController[] guns; // tabs 순서대로 총기, 플레이어 탭은 null 가능

    [Header("플레이어")]
    public PlayerHealth playerhp;
    public PlayerLevel playerlv;
    public PlayerController playercr;

    [Header("왼손 X 버튼 InputAction")]
    public InputActionProperty xButtonAction;

    private int currentTabIndex = -1; // -1: UI 꺼짐
    private bool wasPressed = false;
    private bool isOpen = false;

    void Update()
    {
        // X 버튼 누를 때 탭 순환
        bool isPressed = xButtonAction.action.IsPressed();
        if (isPressed && !wasPressed)
            CycleTabs();
        wasPressed = isPressed;

        // UI 열려 있으면 현재 탭 스탯 갱신
        if (isOpen && currentTabIndex >= 0)
            UpdateCurrentTab();
    }

    void CycleTabs()
    {
        // 이전 탭 비활성화
        if (currentTabIndex >= 0 && currentTabIndex < tabs.Length)
            tabs[currentTabIndex].SetActive(false);

        // 다음 상태로 이동
        currentTabIndex = (currentTabIndex + 1) % (tabs.Length + 1); // +1: UI 끄기 포함

        if (currentTabIndex == tabs.Length)
        {
            // UI 끄기
            Close();
        }
        else
        {
            // UI 켜기
            Open();
            tabs[currentTabIndex].SetActive(true);
        }
    }

    void Open()
    {
        if (isOpen) return;
        isOpen = true;

        statusPanel.SetActive(true);
        GamePauseManager.RequestPause();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Transform cam = Camera.main.transform;
        statusPanel.transform.position = cam.position + cam.forward * 1f;
        statusPanel.transform.rotation = Quaternion.LookRotation(cam.forward, Vector3.up);
        statusPanel.transform.position += new Vector3(0f, -0.1f, 0f);
        // XR Origin 자식 아니면 카메라 앞에 배치
        //if (statusPanel.transform.parent == null)
        //{

        //}
    }

    void Close()
    {
        if (!isOpen) return;
        isOpen = false;

        statusPanel.SetActive(false);
        GamePauseManager.ReleasePause();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentTabIndex = -1;
    }
    void UpdateCurrentTab()
    {
        // currentTabIndex가 유효 범위 내인지 먼저 체크
        if (currentTabIndex < 0 || currentTabIndex >= tabTexts.Length) return;

        TextMeshProUGUI currentText = tabTexts[currentTabIndex];
        GunController currentGun = null;

        if (currentTabIndex > 0 && currentTabIndex - 1 < guns.Length)
            currentGun = guns[currentTabIndex - 1]; // 🔹 여기서 -1 적용

        if (currentTabIndex == 0)
            UpdatePlayerStats(currentText);
        else
            UpdateGunStats(currentGun, currentText);
    }
    void UpdatePlayerStats(TextMeshProUGUI uiText)
    {
        if (playerhp == null || playerlv == null) return;

        uiText.text =
            $"\t체력 : {(int)playerhp.currentHealth} / {playerhp.maxHealth}\n" +
            $"\t스테미나 : {(int)(playercr.currentStamina * 10f)} / {playercr.maxStamina * 10f}\n" +
            $"\t이동속도 : {playercr.moveSpeed}\n" +
            $"\t레벨 : {playerlv.level}\n" +
            $"\t경험치 : {playerlv.currentExp} / {playerlv.maxExp}\n";
    }

    void UpdateGunStats(GunController gun, TextMeshProUGUI uiText)
    {
        if (gun == null || uiText == null)
        {
            uiText.text = "정보 없음";
            return;
        }

        if (gun.isAuraMode)
        {
            uiText.text =
                $"\t공격력 : {gun.GetFinalDamage()}\n" +
                $"\t공격 속도 : {gun.GetFinalFireRate():F1}\n" +
                $"\t공격 범위 : {(int)gun.auraVisual.localScale.x}";
        }
        else
        {
            uiText.text =
                $"\t공격력 : {gun.GetFinalDamage()}\n" +
                $"\t탄약 : {gun.currentAmmo} / {gun.GetFinalMaxAmmo()}\n" +
                $"\t발사 속도 : {gun.GetFinalFireRate()}\n" +
                $"\t산탄 : {gun.GetFinalPelletCount()}\n" +
                $"\t관통 : {gun.GetFinalPenetration()}";
        }
    }
}
