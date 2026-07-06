using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Stores one discovered relationship between two cards.
// [System.Serializable] so we can save these in Day 8.
[System.Serializable]
public class CardConnection
{
    public KnowledgeCard cardA;
    public KnowledgeCard cardB;
    public float strength; // 0-1, based on word overlap ratio
}

public class AICurator : MonoBehaviour
{
    // Singleton — AICurator.instance is reachable from anywhere
    public static AICurator instance;

    [Header("Analysis Settings")]
    // Minimum overlap ratio to count as a connection.
    // 0.2 means at least 20% of the smaller card's words
    // must appear in the other card.
    public float connectionThreshold = 0.2f;

    // How often (in seconds) the curator scans all cards automatically.
    // 30 seconds means it checks every half minute without being asked.
    public float scanInterval = 30f;

    [Header("References")]
    public CuratorOrb orb; // drag the AICurator sphere here

    // All discovered connections. Public so Day 8's line-drawing
    // script can read them directly.
    public List<CardConnection> connections = new List<CardConnection>();

    // Words we ignore when comparing — they appear in almost every
    // sentence and create false connections ("the" linking two unrelated cards).
    private static readonly HashSet<string> stopWords = new HashSet<string>
    {
        "the","a","an","and","or","but","in","on","at","to","for",
        "of","with","is","it","this","that","i","my","we","are",
        "was","be","have","has","do","not","from","by","as","up"
    };

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Start a repeating background scan.
        // InvokeRepeating(method, firstCallDelay, repeatInterval)
        // This is like setInterval in JavaScript — it keeps calling
        // ScanAllCards every scanInterval seconds, forever.
        InvokeRepeating(nameof(ScanAllCards), 5f, scanInterval);
    }

    // Called by WorldSaveManager after every save so the AI
    // stays current when cards change between auto-scans.
    public void ScanAllCards()
    {
        // Find every KnowledgeCard currently in the scene.
        KnowledgeCard[] allCards =
            FindObjectsByType<KnowledgeCard>(FindObjectsSortMode.None);

        if (allCards.Length < 2)
        {
            Debug.Log("[Curator] Not enough cards to find connections yet.");
            return;
        }

        List<CardConnection> newConnections = new List<CardConnection>();

        // Compare every pair of cards exactly once.
        // The nested loop structure (i from 0, j from i+1) ensures
        // we never compare a card to itself, and never compare the
        // same pair twice in reverse order.
        for (int i = 0; i < allCards.Length; i++)
        {
            for (int j = i + 1; j < allCards.Length; j++)
            {
                float strength = ComputeSimilarity(
                    allCards[i].GetContent(),
                    allCards[j].GetContent());

                if (strength >= connectionThreshold)
                {
                    newConnections.Add(new CardConnection
                    {
                        cardA     = allCards[i],
                        cardB     = allCards[j],
                        strength  = strength
                    });

                    Debug.Log(string.Format(
                        "[Curator] Connection found ({0:P0} overlap):\n" +
                        "  Card A: \"{1}\"\n  Card B: \"{2}\"",
                        strength,
                        Truncate(allCards[i].GetContent(), 40),
                        Truncate(allCards[j].GetContent(), 40)));
                }
            }
        }

       bool foundNew = newConnections.Count > connections.Count;
        connections = newConnections;

        // Tell the visual layer to redraw lines for the new connection set.
        // ConnectionRenderer reads our connections list directly.
        if (ConnectionRenderer.instance != null)
            ConnectionRenderer.instance.DrawConnections(connections);

        // Flash the orb if we found NEW connections this scan.
        if (foundNew && orb != null)
            orb.FlashActivity();

        Debug.Log(string.Format(
        "[Curator] Scan complete. {0} cards, {1} connections found.",
        allCards.Length, connections.Count));
    }

    // ── Similarity calculation ─────────────────────────────────────────────

    float ComputeSimilarity(string textA, string textB)
    {
        // Empty cards share nothing.
        if (string.IsNullOrWhiteSpace(textA) ||
            string.IsNullOrWhiteSpace(textB)) return 0f;

        HashSet<string> wordsA = Tokenise(textA);
        HashSet<string> wordsB = Tokenise(textB);

        if (wordsA.Count == 0 || wordsB.Count == 0) return 0f;

        // Count how many words appear in BOTH sets.
        // This is set intersection — the mathematical term for
        // "elements that exist in A and also in B."
        int shared = 0;
        foreach (string word in wordsA)
            if (wordsB.Contains(word)) shared++;

        // Divide by the smaller set's size — this is called
        // "containment similarity." If a short card's every word
        // appears in a longer card, that's a strong connection (1.0),
        // even if the longer card has lots of extra words.
        float smaller = Mathf.Min(wordsA.Count, wordsB.Count);
        return shared / smaller;
    }

    HashSet<string> Tokenise(string text)
    {
        // Convert to lowercase, split on spaces and punctuation,
        // and strip out stop words and very short words.
        HashSet<string> words = new HashSet<string>();

        // Split on any non-letter character (spaces, commas, periods, etc.)
        string[] tokens = System.Text.RegularExpressions.Regex
            .Split(text.ToLower(), @"[^a-z]+");

        foreach (string token in tokens)
        {
            // Skip empty strings, single letters, and stop words.
            if (token.Length < 2)       continue;
            if (stopWords.Contains(token)) continue;
            words.Add(token);
        }

        return words;
    }

    string Truncate(string s, int maxLen)
    {
        if (s == null) return "";
        s = s.Replace("\n", " ");
        return s.Length <= maxLen ? s : s.Substring(0, maxLen) + "...";
    }
}