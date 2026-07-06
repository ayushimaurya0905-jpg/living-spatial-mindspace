using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class MainMenuController : MonoBehaviour
{
    [Header("Buttons — drag from Hierarchy")]
    public Button enterButton;
    public Button continueButton;
    public Button resetButton;
    public Button quitButton;

    // The path must match exactly what WorldSaveManager uses.
    // We hardcode the filename here so the menu can check for it
    // without needing WorldSaveManager to exist in this scene.
    private string SavePath =>
        Application.persistentDataPath + "/worldsave.json";

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        bool saveExists = File.Exists(SavePath);

        // "Continue" only makes sense if there's something to continue.
        // We show Enter when there's no save, Continue when there is.
        // Both buttons load the same scene — the world's LoadWorld() handles the rest.
        enterButton.gameObject.SetActive(!saveExists);
        continueButton.gameObject.SetActive(saveExists);

        // Wire up button clicks.
        // AddListener is Unity's way of saying "when this button is clicked,
        // call this function." The () => syntax is a lambda — an inline
        // anonymous function, same idea as an arrow function in JavaScript.
        enterButton.onClick.AddListener(() => LoadWorld());
        continueButton.onClick.AddListener(() => LoadWorld());
        resetButton.onClick.AddListener(() => ResetAndEnter());
        quitButton.onClick.AddListener(() => QuitGame());
    }

    void LoadWorld()
    {
        // SceneManager.LoadScene loads a scene by its build index.
        // Index 1 = SampleScene, as set in Build Settings.
        // The current scene (MainMenu, index 0) is automatically unloaded.
        SceneManager.LoadScene(1);
    }

    void ResetAndEnter()
    {
        // Delete the save file right here, before loading the scene.
        // When SampleScene loads, WorldSaveManager.Start() will call LoadWorld(),
        // find no save file, and start fresh — exactly what we want.
        if (File.Exists(SavePath))
            File.Delete(SavePath);

        SceneManager.LoadScene(1);
    }

    void QuitGame()
    {
        // Application.Quit() does nothing inside the Editor —
        // that's expected. It only works in an actual built .exe.
        // In the Editor, use the Stop button manually.
        Application.Quit();
    }
}