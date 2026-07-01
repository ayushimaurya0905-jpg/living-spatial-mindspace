using UnityEngine;

public class CardInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactRange = 3f;
    public KeyCode interactKey = KeyCode.E;
    public KeyCode grabKey    = KeyCode.G;
    public Camera playerCamera;

    [Header("Grab Settings")]
    public float holdDistance = 2f; // how far in front card floats while held

    private KnowledgeCard lookedAtCard;
    private KnowledgeCard editingCard;
    private Transform heldCard;
    private float extraRotationY = 0f; // accumulated spin from R key

    void Update()
    {
        // Three mutually exclusive modes, checked in priority order.

        // MODE 1 — typing into a card
        if (editingCard != null)
        {
            HandleTypingInto(editingCard);
            return; // skip everything else this frame
        }

        // MODE 2 — holding/carrying a card
        if (heldCard != null)
        {
            UpdateHeldCardPosition();

            // R snaps the card 45° at a time — useful for placing it on walls at an angle.
            if (Input.GetKeyDown(KeyCode.R))
                extraRotationY += 45f;

            // G again = release and place it where it currently is.
            if (Input.GetKeyDown(grabKey))
                ReleaseCard();

            return; // skip raycasting while holding
        }

        // MODE 3 — normal look-and-interact
        DetectLookedAtCard();

        if (lookedAtCard != null)
        {
            if (Input.GetKeyDown(interactKey)) StartEditing(lookedAtCard);
            if (Input.GetKeyDown(grabKey))     GrabCard(lookedAtCard);
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

    void GrabCard(KnowledgeCard card)
    {
        card.SetHighlight(false);
        lookedAtCard = null;
        heldCard = card.transform;
        extraRotationY = 0f; // reset spin each time you pick up

        // Disable collider while held — otherwise the card blocks your own raycast
        // and you can never look past it to see other cards.
        Collider col = heldCard.GetComponentInChildren<Collider>();
        if (col != null) col.enabled = false;
    }

    void UpdateHeldCardPosition()
    {
        // Target position = always holdDistance units straight ahead of camera.
        Vector3 target = playerCamera.transform.position
                       + playerCamera.transform.forward * holdDistance;

        // Lerp (Linear Interpolation) moves the card smoothly toward the target
        // instead of snapping instantly — the "10f" controls how quickly it catches up.
        // At Time.deltaTime * 10f it reaches ~95% of the way in about 0.3 seconds.
        heldCard.position = Vector3.Lerp(heldCard.position, target, Time.deltaTime * 10f);

        // Keep the card facing the player and perfectly upright regardless of camera tilt.
        Vector3 dir = playerCamera.transform.position - heldCard.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion baseRot = Quaternion.LookRotation(dir);
            heldCard.rotation = Quaternion.Euler(0,
                baseRot.eulerAngles.y + extraRotationY, 0);
        }
    }

    void ReleaseCard()
    {
        Collider col = heldCard.GetComponentInChildren<Collider>();
        if (col != null) col.enabled = true;
        heldCard = null;
    }

    void StartEditing(KnowledgeCard card)
    {
        editingCard = card;
        card.SetHighlight(false);
        card.BeginEdit();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void HandleTypingInto(KnowledgeCard card)
    {
        foreach (char c in Input.inputString)
        {
            if (c == '\b')             card.Backspace();
            else if (c == '\n' || c == '\r') { FinishEditing(); return; }
            else                       card.AppendChar(c);
        }
        if (Input.GetKeyDown(KeyCode.Escape)) FinishEditing();
    }

    void FinishEditing()
    {
        editingCard.EndEdit();
        editingCard = null;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}