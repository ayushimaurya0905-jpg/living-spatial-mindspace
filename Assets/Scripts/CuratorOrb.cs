using UnityEngine;

public class CuratorOrb : MonoBehaviour
{
    [Header("Wander settings")]
    public float wanderHeight      = 2.8f;  // near the ceiling
    public float roomRadius        = 3.5f;  // how far from center it roams
    public float moveSpeed         = 0.6f;  // units per second
    public float waypointReachDist = 0.4f;  // how close = "arrived"
    public float minPlayerDist     = 3f;    // never get closer than this to player

    [Header("Bob settings")]
    public float bobHeight  = 0.18f;
    public float bobSpeed   = 1.4f;

    [Header("Pulse settings")]
    public float pulseSpeed  = 2f;
    public float pulseAmount = 0.07f;

    private Vector3 currentWaypoint;
    private Transform playerTransform;
    private Renderer orbRenderer;
    private float randomOffset;

    void Start()
    {
        orbRenderer  = GetComponent<Renderer>();
    randomOffset = Random.Range(0f, Mathf.PI * 2f);

    GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
    if (playerObj != null)
        playerTransform = playerObj.transform;
    else
        Debug.LogWarning("[CuratorOrb] Player tag not found. " +
            "Select Player → Tag → Player in Inspector.");

    // Pick first waypoint
    currentWaypoint = PickNewWaypoint();

    // TELEPORT immediately instead of drifting from center —
    // without this the orb spawns at (0,2,0) which is directly
    // in the player's face for the first few seconds
    transform.position = currentWaypoint;

    Debug.Log("[CuratorOrb] Spawned at waypoint: " + currentWaypoint);
    }

    void Update()
    {
        // ── Move toward current waypoint ──────────────────────────
        Vector3 flat = new Vector3(
            currentWaypoint.x, transform.position.y, currentWaypoint.z);

        transform.position = Vector3.MoveTowards(
            transform.position, currentWaypoint, moveSpeed * Time.deltaTime);

        // Pick a new waypoint when we arrive
        if (Vector3.Distance(transform.position, currentWaypoint)
            < waypointReachDist)
        {
            currentWaypoint = PickNewWaypoint();
        }

        // ── Bob up and down ───────────────────────────────────────
        float bob = Mathf.Sin(Time.time * bobSpeed + randomOffset) * bobHeight;
        transform.position = new Vector3(
            transform.position.x,
            wanderHeight + bob,
            transform.position.z);

        // ── Pulse scale ───────────────────────────────────────────
        float pulse = 1f + Mathf.Sin(
            Time.time * pulseSpeed + randomOffset) * pulseAmount;
        transform.localScale = Vector3.one * 0.3f * pulse;
    }

    Vector3 PickNewWaypoint()
    {
        // Try up to 10 times to find a waypoint far enough from the player
        for (int attempt = 0; attempt < 10; attempt++)
        {
            // Random angle around the room, random distance from center
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float dist  = Random.Range(roomRadius * 0.5f, roomRadius);

            Vector3 candidate = new Vector3(
                Mathf.Cos(angle) * dist,
                wanderHeight,
                Mathf.Sin(angle) * dist);

            // If player reference exists, reject waypoints too close to player
            if (playerTransform != null)
            {
                float distToPlayer = Vector3.Distance(
                    new Vector3(candidate.x, 0, candidate.z),
                    new Vector3(playerTransform.position.x,
                                0,
                                playerTransform.position.z));

                if (distToPlayer < minPlayerDist) continue;
            }

            return candidate;
        }

        // Fallback — opposite corner of the room if all attempts failed
        return new Vector3(-roomRadius, wanderHeight, -roomRadius);
    }

    // Called by AICurator when it finds a new connection
    public void FlashActivity()
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    System.Collections.IEnumerator FlashRoutine()
    {
        MaterialPropertyBlock props = new MaterialPropertyBlock();
        orbRenderer.GetPropertyBlock(props);

        float elapsed  = 0f;
        float duration = 0.6f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float intensity = Mathf.Lerp(4f, 1f, t);
            props.SetColor("_EmissionColor",
                new Color(0.47f, 0.24f, 1f) * intensity);
            orbRenderer.SetPropertyBlock(props);

            yield return null;
        }
    }
}