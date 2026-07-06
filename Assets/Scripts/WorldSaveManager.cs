using UnityEngine;
using System.IO;
using System.Collections.Generic;

// These two classes are pure data containers — no MonoBehaviour, no Unity magic.
// [System.Serializable] tells Unity's JsonUtility that it's allowed to convert
// these classes to and from JSON text.
[System.Serializable]
public class CardSaveData
{
    public string type;     // "Note", "Task", or "Idea"
    public string content;  // whatever the user typed
    public float posX, posY, posZ;   // world position
    public float rotY;               // only Y rotation matters — cards always stay upright
}

[System.Serializable]
public class WorldSaveData
{
    // A List is like an array but can grow — we don't know how many cards
    // the user will create, so we can't use a fixed-size array.
    public List<CardSaveData> cards = new List<CardSaveData>();
}

public class WorldSaveManager : MonoBehaviour
{
    // The singleton: a static reference to the one instance of this script.
    // Static means it belongs to the CLASS, not to any specific object —
    // so WorldSaveManager.instance works from anywhere without a reference.
    public static WorldSaveManager instance;

    [Header("Prefab")]
    public GameObject cardPrefab; // drag KnowledgeCard_Note prefab here

    // The full file path where we'll write the JSON.
    private string SavePath => Application.persistentDataPath + "/worldsave.json";

    void Awake()
    {
        // Awake() runs before Start() — important because we want the singleton
        // set up BEFORE any other script tries to call SaveWorld().
        instance = this;
    }

    void Start()
    {
        // Every time the app opens, immediately try to load the save file.
        LoadWorld();
    }

    // ─── SAVING ───────────────────────────────────────────────────────────────

    public void SaveWorld()
    {
        WorldSaveData data = new WorldSaveData();

        // FindObjectsByType searches the entire scene for every active KnowledgeCard.
        // This is intentionally simple — it just grabs everything that exists right now.
        KnowledgeCard[] allCards = FindObjectsByType<KnowledgeCard>(FindObjectsSortMode.None);

        foreach (KnowledgeCard card in allCards)
        {
            CardSaveData entry = new CardSaveData();
            entry.type    = card.cardType.ToString(); // enum → string e.g. "Note"
            entry.content = card.GetContent();
            entry.posX    = card.transform.position.x;
            entry.posY    = card.transform.position.y;
            entry.posZ    = card.transform.position.z;
            entry.rotY    = card.transform.eulerAngles.y;
            data.cards.Add(entry);
        }

        // JsonUtility.ToJson converts the WorldSaveData object into a JSON string.
        // prettyPrint: true adds line breaks so the file is human-readable —
        // useful for debugging. In a released game you'd set it false to save space.
        string json = JsonUtility.ToJson(data, prettyPrint: true);

        // File.WriteAllText writes that string to disk, creating the file if needed
        // or overwriting it if it already exists.
        File.WriteAllText(SavePath, json);

        // Tell the AI to re-analyse after every save,
        // so connections update whenever you add or edit a card.
        if (AICurator.instance != null)
            AICurator.instance.ScanAllCards();

        Debug.Log("World saved to: " + SavePath);
    }

    // ─── LOADING ──────────────────────────────────────────────────────────────

    public void LoadWorld()
    {
        // File.Exists checks whether a save file actually exists yet.
        // On the very first run there won't be one — that's normal, not an error.
        if (!File.Exists(SavePath))
        {
            Debug.Log("No save file found — starting fresh.");
            return;
        }

        string json = File.ReadAllText(SavePath);

        // JsonUtility.FromJson is the reverse of ToJson —
        // it reads a JSON string and fills a WorldSaveData object with the values.
        WorldSaveData data = JsonUtility.FromJson<WorldSaveData>(json);

        if (data == null || data.cards == null)
        {
            Debug.LogWarning("Save file was empty or corrupted.");
            return;
        }

        foreach (CardSaveData entry in data.cards)
        {
            // Reconstruct position and rotation from saved numbers.
            Vector3 pos    = new Vector3(entry.posX, entry.posY, entry.posZ);
            Quaternion rot = Quaternion.Euler(0, entry.rotY, 0);

            // Stamp out a new card prefab at that position and rotation.
            GameObject obj = Instantiate(cardPrefab, pos, rot);
            KnowledgeCard card = obj.GetComponent<KnowledgeCard>();

            // Parse the saved type string back into the CardType enum.
            // System.Enum.Parse converts "Note" → CardType.Note, etc.
            if (System.Enum.TryParse(entry.type, out CardType parsedType))
                card.SetType(parsedType);

            // Restore the text the user had typed.
            card.SetContent(entry.content);
        }

        Debug.Log("Loaded " + data.cards.Count + " cards from save file.");
    }

    // ─── RESET ────────────────────────────────────────────────────────────────

    public void ResetWorld()
    {
        // Delete the save file from disk.
        if (File.Exists(SavePath))
            File.Delete(SavePath);

        // Destroy every card currently in the scene.
        KnowledgeCard[] allCards = FindObjectsByType<KnowledgeCard>(FindObjectsSortMode.None);
        foreach (KnowledgeCard card in allCards)
            Destroy(card.gameObject);

         if (ConnectionRenderer.instance != null)
        ConnectionRenderer.instance.ClearAll();

        Debug.Log("World reset.");
    }
}