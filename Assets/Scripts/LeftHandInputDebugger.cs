using UnityEngine;
using UnityEngine.InputSystem;

public class LeftHandInputDebugger : MonoBehaviour
{
    public InputActionProperty selectAction;
    public InputActionProperty activateAction;

    void OnEnable()
    {
        selectAction.action.Enable();
        activateAction.action.Enable();
    }

    void Update()
    {
        float selectValue = selectAction.action.ReadValue<float>();
        float activateValue = activateAction.action.ReadValue<float>();

        if (selectValue > 0.1f)
            Debug.Log($"[LeftHand] Select Pressed ({selectValue})");
        if (activateValue > 0.1f)
            Debug.Log($"[LeftHand] Activate Pressed ({activateValue})");
    }
}