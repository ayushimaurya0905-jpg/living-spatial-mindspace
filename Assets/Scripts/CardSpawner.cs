using UnityEngine;

public class CardSpawner : MonoBehaviour
{
    [Header("Effects")]
    public GameObject spawnEffectPrefab; // drag CardSpawnEffect prefab here

    [Header("Prefab")]
    public GameObject cardPrefab;

    [Header("Spawn Settings")]
    public float spawnDistance = 2f;

    private Camera playerCamera;
    private CardInteractor interactor;

    void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();

        // Grab the CardInteractor from the same Player object
        // so we can check IsEditing before spawning
        interactor = GetComponent<CardInteractor>();
    }

    void Update()
    {
        // CRITICAL: if the user is currently typing into a card,
        // completely ignore all spawn keys this frame.
        // Without this, pressing N while typing would both add
        // the letter 'n' to the card AND spawn a new Note card.
        if (interactor != null && interactor.IsEditing) return;

        if (Input.GetKeyDown(KeyCode.N)) SpawnCard(CardType.Note);
        if (Input.GetKeyDown(KeyCode.T)) SpawnCard(CardType.Task);
        if (Input.GetKeyDown(KeyCode.I)) SpawnCard(CardType.Idea);
    }

    void SpawnCard(CardType type)
    {
        Vector3 spawnPos = playerCamera.transform.position
                         + playerCamera.transform.forward * spawnDistance;

        Vector3 dirToPlayer = playerCamera.transform.position - spawnPos;
        dirToPlayer.y = 0;
        Quaternion facingPlayer = Quaternion.LookRotation(dirToPlayer);

        GameObject newCard = Instantiate(cardPrefab, spawnPos, facingPlayer);
        newCard.GetComponent<KnowledgeCard>().SetType(type);

        // Burst of particles at spawn position — satisfying tactile feedback
        
        WorldSaveManager.instance.SaveWorld();
        Debug.Log("Spawned " + type);
        if (spawnEffectPrefab != null)
        {
            GameObject effect = Instantiate(
                spawnEffectPrefab, spawnPos, Quaternion.identity);

            // Auto-destroy after 1 second — particles are temporary
            Destroy(effect, 1f);
        }
    }
}