using UnityEngine;

public class InteractController : MonoBehaviour
{
    private PlayerInputController inputController;

    void Awake()
    {
        inputController = GetComponent<PlayerInputController>();

        if (inputController != null)
        {
            inputController.OnInteractKeyPressed.AddListener(interact);
        }
    }

    void OnDestroy()
    {
        if (inputController != null)
        {
            inputController.OnInteractKeyPressed.RemoveListener(interact);
        }    
    }



    private void interact(Vector3Int playerGridPos)
    {

        GridManager.Instance.TryGetGridObjectAt(
            playerGridPos, out GridObject gridObject, ignorePlayer: true);

        IInteract interactable = (IInteract)gridObject;

        if (interactable != null)
        {
            interactable.OnInteract();
        }
    }
}