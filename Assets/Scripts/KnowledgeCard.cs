using UnityEngine;
using TMPro;

public enum CardType { Note, Task, Idea }

public class KnowledgeCard : MonoBehaviour
{
    [Header("References (auto-found if empty)")]
    public TextMeshPro textMesh;
    public Renderer cardRenderer;

    [Header("Type")]
    public CardType cardType = CardType.Note;

    private static readonly Color[] typeColors = {
        new Color(0.85f, 0.8f,  0.6f),
        new Color(0.9f,  0.5f,  0.5f),
        new Color(0.5f,  0.7f,  0.9f),
    };

    private static readonly Color[] highlightColors = {
        new Color(1f,    0.95f, 0.4f),
        new Color(1f,    0.6f,  0.4f),
        new Color(0.5f,  0.85f, 1f),
    };

    private string content = "";
    private bool isEditing = false;

    void Awake()
    {
        // Auto-find components on children if slots weren't filled manually.
        // GetComponentInChildren searches this object AND all its children.
        if (textMesh == null)
            textMesh = GetComponentInChildren<TextMeshPro>();

        if (cardRenderer == null)
        {
            // Find CardBody specifically — we don't want to grab a renderer
            // on any other child accidentally.
            Transform body = transform.Find("CardBody");
            if (body != null)
                cardRenderer = body.GetComponent<Renderer>();
        }

        // Warn loudly in Console if still null after auto-search.
        if (textMesh == null)
            Debug.LogWarning("[KnowledgeCard] No TextMeshPro found on "
                + gameObject.name + " or its children.");

        if (cardRenderer == null)
            Debug.LogWarning("[KnowledgeCard] No Renderer found on CardBody of "
                + gameObject.name);
    }

    void Start()
    {
        ApplyTypeColor();
        RefreshDisplay();
    }

    public void SetType(CardType type)
    {
        cardType = type;
        ApplyTypeColor();
        RefreshDisplay();
    }

    void ApplyTypeColor()
    {
        if (cardRenderer != null)
            cardRenderer.material.color = typeColors[(int)cardType];
    }

    public void SetHighlight(bool on)
    {
        if (isEditing) return;
        if (cardRenderer != null)
            cardRenderer.material.color = on
                ? highlightColors[(int)cardType]
                : typeColors[(int)cardType];
    }

    public void BeginEdit()
    {
        isEditing = true;
        if (cardRenderer != null)
            cardRenderer.material.color = highlightColors[(int)cardType];
        RefreshDisplay();
    }

    public void EndEdit()
    {
        isEditing = false;
        ApplyTypeColor();
        RefreshDisplay();
    }

    // Called by CardEditUI when the user closes the edit panel.
    public void SetContent(string s)
    {
        content = s;
        Debug.Log("[KnowledgeCard] SetContent called on "
            + gameObject.name + " with: \"" + s + "\"");
        RefreshDisplay();
    }

    public string GetContent() => content;

    public void AppendChar(char c)
    {
        content += c;
        RefreshDisplay();
    }

    public void Backspace()
    {
        if (content.Length > 0)
            content = content.Substring(0, content.Length - 1);
        RefreshDisplay();
    }

    void RefreshDisplay()
    {
       

    if (textMesh == null)
    {
        Debug.LogWarning("[KnowledgeCard] textMesh is NULL on "
            + gameObject.name + " — CardText not found.");
        return;
    }

    string typeLabel = "[" + cardType.ToString().ToUpper() + "]";
    string body = string.IsNullOrEmpty(content)
        ? "(press E to write)"
        : content;
    if (isEditing) body += " |";

    textMesh.text = typeLabel + "\n" + body;

    }
}