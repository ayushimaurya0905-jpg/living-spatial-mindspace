using UnityEngine;
using TMPro;

public class CardEditUI : MonoBehaviour
{
    public static CardEditUI instance;
    public static bool IsOpen = false;

    [Header("Drag from Hierarchy")]
    public GameObject editPanel;
    public TextMeshProUGUI typedTextLabel; // plain TMP text — NOT an InputField

    private KnowledgeCard currentCard;
    private string typedText = "";

    void Awake()
    {
        instance = this;

        if (editPanel == null)
            Debug.LogError("[CardEditUI] editPanel slot is empty!");
        if (typedTextLabel == null)
            Debug.LogError("[CardEditUI] typedTextLabel slot is empty!");

        if (editPanel != null)
            editPanel.SetActive(false);
    }

    // Called by CardInteractor when player presses E on a card
    public void BeginEditing(KnowledgeCard card)
    {
        currentCard = card;

        // Load whatever was already written on this card
        typedText = card.GetContent();

        IsOpen = true;
        editPanel.SetActive(true);

        // Unlock cursor so it's visible (cosmetic only — we don't need
        // it for input, but it looks strange locked during a text panel)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UpdateDisplay();

        Debug.Log("[CardEditUI] Editing started. Type freely. Escape to save.");
    }

    // OnGUI() is Unity's event-driven input system.
    // Unlike Update() which polls every frame, OnGUI() is called
    // SPECIFICALLY when a keyboard or mouse event occurs.
    // Event.current.character already has Shift/CapsLock resolved —
    // pressing Shift+A gives 'A', not two separate events.
    // This completely bypasses TMP_InputField and EventSystem focus.
    void OnGUI()
    {
        if (!IsOpen) return;

        Event e = Event.current;

        // We only care about key-down events — ignore mouse, repaint, etc.
        if (e.type != EventType.KeyDown) return;

        // Escape = save and close
        if (e.keyCode == KeyCode.Escape)
        {
            EndEditing();
            e.Use(); // consume the event so nothing else sees it
            return;
        }

        // Backspace = delete last character
        if (e.keyCode == KeyCode.Backspace)
        {
            if (typedText.Length > 0)
                typedText = typedText.Substring(0, typedText.Length - 1);
            e.Use();
            UpdateDisplay();
            return;
        }

        // Enter = newline (allows multi-line notes)
        if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
        {
            typedText += "\n";
            e.Use();
            UpdateDisplay();
            return;
        }

        // Any printable character — e.character handles your keyboard
        // layout, Shift state, and language automatically.
        // char.IsControl filters out arrow keys, function keys, etc.
        if (e.character != '\0' && !char.IsControl(e.character))
        {
            typedText += e.character;
            e.Use();
            UpdateDisplay();
        }
    }

    void UpdateDisplay()
    {
        if (typedTextLabel == null) return;

        // Show the typed string with a blinking cursor character at the end.
        // If nothing typed yet, show a placeholder hint.
        if (typedText.Length == 0)
            typedTextLabel.text = "<color=#888888>Start typing...</color>|";
        else
            typedTextLabel.text = typedText + "|";
    }

    void Update()
    {
        // Nothing needed here — OnGUI handles all input.
        // We keep Update() empty so there's no confusion about
        // where input is being processed.
    }

    public void EndEditing()
    {
        if (currentCard != null)
        {
            Debug.Log("[CardEditUI] Saving to card: \"" + typedText + "\"");
            currentCard.SetContent(typedText);
            currentCard.EndEdit();
            currentCard = null;
        }

        typedText = "";
        IsOpen = false;
        editPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (WorldSaveManager.instance != null)
            WorldSaveManager.instance.SaveWorld();
    }
}