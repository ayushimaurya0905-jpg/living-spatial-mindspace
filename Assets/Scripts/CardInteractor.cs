using UnityEngine;

public class CardInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactRange = 3f;
    public KeyCode interactKey = KeyCode.E;
    public KeyCode grabKey     = KeyCode.G;
    public KeyCode deleteKey   = KeyCode.Delete;
    public Camera playerCamera;

    [Header("Grab Settings")]
    public float holdDistance = 2f;

    // CardSpawner reads this to block spawning while typing
    public bool IsEditing => CardEditUI.IsOpen;

    private KnowledgeCard lookedAtCard;
    private Transform heldCard;
    private float extraRotationY = 0f;

    void Update()
    {
        // If the edit panel is open, don't do anything in 3D world
        // CardEditUI handles all input while IsOpen = true
        if (CardEditUI.IsOpen) return;

        // If holding a card, handle that first
        if (heldCard != null)
        {
            UpdateHeldCardPosition();
            if (Input.GetKeyDown(KeyCode.R)) extraRotationY += 45f;
            if (Input.GetKeyDown(grabKey))   ReleaseCard();
            return;
        }

        // Normal mode — detect what we're looking at
        DetectLookedAtCard();

        if (lookedAtCard != null)
        {
            if (Input.GetKeyDown(interactKey)) StartEditing(lookedAtCard);
            if (Input.GetKeyDown(grabKey))     GrabCard(lookedAtCard);
            if (Input.GetKeyDown(deleteKey))   DeleteCard(lookedAtCard);
        }
    }

    void DetectLookedAtCard()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        KnowledgeCard hitCard = null;
        if (Physics.Raycast(ray, out hit, interactRange))
            hitCard = hit.collider.GetComponentInParent<KnowledgeCard>();

        if (hitCard != lookedAtCard)
        {
            if (lookedAtCard != null) lookedAtCard.SetHighlight(false);
            lookedAtCard = hitCard;
            if (lookedAtCard != null) lookedAtCard.SetHighlight(true);
        }
    }

    void StartEditing(KnowledgeCard card)
    {
        // Clear the highlight — card goes into edit color via BeginEdit()
        if (lookedAtCard != null) lookedAtCard.SetHighlight(false);
        lookedAtCard = null;

        card.BeginEdit();

        // Hand off entirely to CardEditUI
        CardEditUI.instance.BeginEditing(card);

        Debug.Log("Edit panel opened");
    }

    void GrabCard(KnowledgeCard card)
    {
        card.SetHighlight(false);
        lookedAtCard = null;
        heldCard = card.transform;
        extraRotationY = 0f;

        Collider col = heldCard.GetComponentInChildren<Collider>();
        if (col != null) col.enabled = false;
    }

    void UpdateHeldCardPosition()
    {
        Vector3 target = playerCamera.transform.position
                       + playerCamera.transform.forward * holdDistance;

        heldCard.position = Vector3.Lerp(
            heldCard.position, target, Time.deltaTime * 10f);

        Vector3 dir = playerCamera.transform.position - heldCard.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion baseRot = Quaternion.LookRotation(dir);
            heldCard.rotation = Quaternion.Euler(
                0, baseRot.eulerAngles.y + extraRotationY, 0);
        }
    }

    void ReleaseCard()
    {
        Collider col = heldCard.GetComponentInChildren<Collider>();
        if (col != null) col.enabled = true;
        heldCard = null;
        WorldSaveManager.instance.SaveWorld();
    }

    void DeleteCard(KnowledgeCard card)
    {
        lookedAtCard = null;
        Destroy(card.gameObject);
        WorldSaveManager.instance.SaveWorld();
        Debug.Log("Card deleted");
    }
}