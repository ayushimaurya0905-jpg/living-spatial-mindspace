using UnityEngine;
using System.Collections.Generic;

public class ConnectionRenderer : MonoBehaviour
{
    public static ConnectionRenderer instance;

    [Header("Line appearance")]
    public Material lineMaterial;
    public float lineWidth    = 0.015f;  // thin but visible
    public float minAlpha     = 0.15f;   // weak connections are faint
    public float maxAlpha     = 0.85f;   // strong connections are bright

    [Header("Animation")]
    public float pulseSpeed   = 1.5f;    // how fast lines breathe in/out
    public float pulseAmount  = 0.3f;    // how much brightness changes

    // All currently drawn lines — we replace the whole set on each scan.
    private List<LineRenderer> activeLines = new List<LineRenderer>();

    // Keeps the Hierarchy tidy — all line objects live under this parent.
    private GameObject linesParent;

    void Awake()
    {
        instance = this;
        linesParent = new GameObject("_ConnectionLines");
    }

    void Update()
    {
        // Every frame, gently pulse all active lines' opacity.
        // This gives the world a breathing, alive quality even when
        // nothing is changing — the connections feel organic, not static.
        if (activeLines.Count == 0) return;

        // A sine wave between 0 and 1, cycling at pulseSpeed Hz.
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

        foreach (LineRenderer lr in activeLines)
        {
            if (lr == null) continue;

            // Read the line's current color, modify only the alpha.
            Color c = lr.startColor;
            c.a = Mathf.Lerp(c.a * (1f - pulseAmount),
                             c.a * (1f + pulseAmount), pulse);

            // Clamp so we don't go above 1 or below 0.
            c.a = Mathf.Clamp(c.a, 0.05f, 1f);
            lr.startColor = c;
            lr.endColor   = c;
        }
    }

    // Called by AICurator every time a scan completes.
    public void DrawConnections(List<CardConnection> connections)
    {
        // Destroy every existing line — we redraw from scratch.
        // This is simpler and safer than trying to update existing ones,
        // because the number of connections can change between scans.
        foreach (LineRenderer lr in activeLines)
        {
            if (lr != null)
                Destroy(lr.gameObject);
        }
        activeLines.Clear();

        if (connections == null || connections.Count == 0)
        {
            Debug.Log("[ConnectionRenderer] No connections to draw.");
            return;
        }

        foreach (CardConnection conn in connections)
        {
            // A card might have been deleted since the last scan.
            // Null-check prevents MissingReferenceException.
            if (conn.cardA == null || conn.cardB == null) continue;

            // Connect the center-top of each card so the line
            // appears to flow from card to card at eye level.
            Vector3 from = conn.cardA.transform.position + Vector3.up * 0.2f;
            Vector3 to   = conn.cardB.transform.position + Vector3.up * 0.2f;

            DrawSingleLine(from, to, conn.strength);
        }

        Debug.Log("[ConnectionRenderer] Drew "
            + activeLines.Count + " connection lines.");
    }

    void DrawSingleLine(Vector3 from, Vector3 to, float strength)
    {
        // Each line is its own GameObject — LineRenderer requires
        // a Transform to exist in the scene.
        GameObject lineObj = new GameObject("Line");
        lineObj.transform.SetParent(linesParent.transform);

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();

        // Two points: start and end. For curved lines later you'd
        // add more points, but straight is fine for now.
        lr.positionCount = 2;
        lr.SetPosition(0, from);
        lr.SetPosition(1, to);

        // Stronger connections get slightly thicker lines —
        // a visual encoding of relationship strength.
        float width = lineWidth * Mathf.Lerp(0.5f, 1.5f, strength);
        lr.startWidth = width;
        lr.endWidth   = width;

        // Alpha linearly mapped from connection strength.
        // A 20% overlap gives a faint line; 100% overlap gives a bright one.
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, strength);
        Color lineColor = new Color(0.6f, 0.3f, 1f, alpha); // purple
        lr.startColor = lineColor;
        lr.endColor   = lineColor;

        if (lineMaterial != null)
            lr.material = lineMaterial;

        // Don't cast shadows — lines are UI-like glowing elements,
        // not solid objects. Shadows would look very wrong.
        lr.shadowCastingMode =
            UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;

        // useWorldSpace = true means positions are in world coordinates,
        // not relative to the parent GameObject's position.
        lr.useWorldSpace = true;

        activeLines.Add(lr);
    }

    // Called by WorldSaveManager when cards are deleted or reset,
    // so stale lines don't linger after their cards are gone.
    public void ClearAll()
    {
        foreach (LineRenderer lr in activeLines)
            if (lr != null) Destroy(lr.gameObject);
        activeLines.Clear();
    }
}