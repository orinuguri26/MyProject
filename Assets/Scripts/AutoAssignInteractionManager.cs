using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class AutoAssignInteractionManager : MonoBehaviour
{
    void Awake()
    {
        XRGrabInteractable grab = GetComponent<XRGrabInteractable>();
        if (grab != null && grab.interactionManager == null)
        {
            XRInteractionManager manager = Object.FindAnyObjectByType<XRInteractionManager>();

            if (manager != null)
            {
                grab.interactionManager = manager;
                Debug.Log("[AutoAssign] XR Interaction Manager 자동 연결 완료!");
            }
            else
            {
                Debug.LogWarning("[AutoAssign] XR Interaction Manager를 찾을 수 없습니다!");
            }
        }
    }
}