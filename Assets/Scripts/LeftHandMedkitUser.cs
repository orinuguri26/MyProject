using UnityEngine;
using UnityEngine.InputSystem;

public class LeftHandMedkitUser : MonoBehaviour
{
    public InputActionProperty useMedkitButton; // Grab 버튼
    private MedkitInventory inventory;

    void Start()
    {
        // 플레이어(XR Origin) 찾기
        inventory = GetComponentInParent<MedkitInventory>();
    }

    void Update()
    {
        if (useMedkitButton.action.WasPressedThisFrame())
        {
            if (inventory != null)
                inventory.UseMedkit();
        }
    }
}
