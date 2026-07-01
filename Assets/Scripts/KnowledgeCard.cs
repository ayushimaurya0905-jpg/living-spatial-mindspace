using UnityEngine;
using TMPro;

// An enum is just a named list of options — like a dropdown in Figma.
// Defining it outside the class means ALL scripts in the project can use CardType freely.
public enum CardType { Note, Task, Idea }

public class KnowledgeCard : MonoBehaviour
{
    [Header("References")]
    public TextMeshPro textMesh;
    public Renderer cardRenderer;

    [Header("Type")]
    public CardType cardType = CardType.Note;

    // Arrays indexed by CardType — typeColors[0] = Note color, [1] = Task, [2] = Idea.
    // These are the resting colors.
    private static readonly Color[] typeColors = {
        new Color(0.85f, 0.8f, 0.6f),  // Note  — warm parchment yellow
        new Color(0.9f,  0.5f, 0.5f),  // Task  — soft terracotta red
        new Color(0.5f,  0.7f, 0.9f),  // Idea  — calm sky blue
    };

    // Brighter versions of the same hues — used for hover highlight and edit mode.
    private static readonly Color[] highlightColors = {
        new Color(1f,   0.95f, 0.4f),  // Note highlighted
        new Color(1f,   0.6f,  0.4f),  // Task highlighted
        new Color(0.5f, 0.85f, 1f),    // Idea highlighted
    };

    private string content = "";
    private bool isEditing = false;

    void Start()
    {
        ApplyTypeColor();
        RefreshDisplay();
    }

    // Called by CardSpawner right after instantiating the prefab.
    public void SetType(CardType type)
    {
        cardType = type;
        ApplyTypeColor();
        RefreshDisplay(); // update the label immediately
    }

    void ApplyTypeColor()
    {
        if (cardRenderer != null)
            cardRenderer.material.color = typeColors[(int)cardType];
        // (int)cardType converts Note→0, Task→1, Idea→2 so we can use it as an array index.
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
        // Show a small type label above the content — like a sticky note header.
        string typeLabel = "[" + cardType.ToString().ToUpper() + "]";
        string body = string.IsNullOrEmpty(content) ? "(press E to write)" : content;
        if (isEditing) body += " |"; // blinking cursor illusion
        if (textMesh != null)
            textMesh.text = typeLabel + "\n" + body;
    }

    // These two are stubs for Day 5's save system — the save script will call them
    // to read content out and write it back in on load.
    public string GetContent() => content;
    public void SetContent(string s) { content = s; RefreshDisplay(); }
}