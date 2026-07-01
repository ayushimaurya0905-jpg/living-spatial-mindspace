using UnityEngine;

public class CardSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject cardPrefab; // drag your KnowledgeCard prefab here

    [Header("Spawn Settings")]
    public float spawnDistance = 2f; // how far in front of you the card appears

    private Camera playerCamera;

    void Awake()
    {
        // GetComponentInChildren searches this object AND all its children —
        // since Camera is a child of Player, this finds it automatically.
        playerCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N)) SpawnCard(CardType.Note);
        if (Input.GetKeyDown(KeyCode.T)) SpawnCard(CardType.Task);
        if (Input.GetKeyDown(KeyCode.I)) SpawnCard(CardType.Idea);
    }

    void SpawnCard(CardType type)
    {
        // Place the card directly in front of wherever the camera is currently pointing.
        Vector3 spawnPos = playerCamera.transform.position
                         + playerCamera.transform.forward * spawnDistance;

        // Rotate it to face back toward the player so you see its front face immediately.
        // We zero out Y on dirToPlayer first so the card stays perfectly upright —
        // without this, if you're looking slightly down when spawning, the card would tilt.
        Vector3 dirToPlayer = playerCamera.transform.position - spawnPos;
        dirToPlayer.y = 0;
        Quaternion facingPlayer = Quaternion.LookRotation(dirToPlayer);

        // Instantiate = Unity's word for "stamp out a copy of this prefab into the scene."
        GameObject newCard = Instantiate(cardPrefab, spawnPos, facingPlayer);
        newCard.GetComponent<KnowledgeCard>().SetType(type);

        Debug.Log("Spawned " + type + " card");
    }
}