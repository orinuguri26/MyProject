using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class LeftHandDebug : MonoBehaviour
{
    void Start()
    {
        if (interactor == null)
            Debug.LogError("[LeftHand] Interactor is not assigned!");
        else
            Debug.Log("[LeftHand] Interactor script active!");
    }

    public XRDirectInteractor interactor;

    void OnEnable()
    {
        interactor.selectEntered.AddListener(OnSelectEntered);
        interactor.selectExited.AddListener(OnSelectExited);
    }

    void OnDisable()
    {
        interactor.selectEntered.RemoveListener(OnSelectEntered);
        interactor.selectExited.RemoveListener(OnSelectExited);
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        Debug.Log("[LeftHand] Object grabbed: " + args.interactableObject.transform.name);
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        Debug.Log("[LeftHand] Object released: " + args.interactableObject.transform.name);
    }
}