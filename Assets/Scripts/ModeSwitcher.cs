using UnityEngine;
using UnityEngine.InputSystem;

public class ModeSwitcher : MonoBehaviour
{
    public InputActionProperty fireAction;
    public InputActionProperty uiSelectAction;

    public void EnterUIMode()
    {
        fireAction.action.Disable();     // 총 발사 비활성화
        uiSelectAction.action.Enable();  // UI 선택 활성화
    }

    public void ExitUIMode()
    {
        uiSelectAction.action.Disable();
        fireAction.action.Enable();
    }
}
