using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRInitial : MonoBehaviour
{
    public XROrigin xrOrigin;                   // Scene의 XR Origin
    public Transform leftModelRoot;             // LeftHand ModelRoot
    public Transform rightModelRoot;            // RightHand ModelRoot

    void Start()
    {
        // 1. XR Origin 초기 X/Z 회전 보정
        Vector3 originEuler = xrOrigin.transform.eulerAngles;
        xrOrigin.transform.eulerAngles = new Vector3(0f, originEuler.y, 0f);

        // 2. Controller 모델 초기 Rotation 0으로 맞추기
        if (leftModelRoot != null)
            leftModelRoot.localRotation = Quaternion.identity;

        if (rightModelRoot != null)
            rightModelRoot.localRotation = Quaternion.identity;

        if (!leftModelRoot.GetComponent<GunController>()?.isAuraMode ?? true)
            leftModelRoot.localRotation = Quaternion.identity;

        // 3. (옵션) HMD forward 기준으로 XR Origin 회전 보정
        Transform cam = xrOrigin.CameraFloorOffsetObject.transform;
        Vector3 camForward = new Vector3(cam.forward.x, 0, cam.forward.z).normalized;
        if (camForward.sqrMagnitude > 0.001f)
            xrOrigin.transform.rotation = Quaternion.LookRotation(camForward);
    }
}
