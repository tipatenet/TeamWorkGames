using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    private enum PauseState { Default, Settings}

    [SerializeField]
    private PauseState currentPauseState = PauseState.Default;

    [SerializeField]
    private GameObject defaultPauseMenu;
    [SerializeField]
    private GameObject settingsMenu;

    private void OnEnable()
    {
        // Ensure the default pause menu is active when the script is enabled
        currentPauseState = PauseState.Default;
        defaultPauseMenu.SetActive(true);
        settingsMenu.SetActive(false);

    }

    private void Update()
    {
        switch(currentPauseState)
        {
            case PauseState.Default:
                // Handle default pause menu logic
                defaultPauseMenu.SetActive(true);
                settingsMenu.SetActive(false);
                break;
            case PauseState.Settings:
                // Handle settings menu logic
                defaultPauseMenu.SetActive(false);
                settingsMenu.SetActive(true);
                break;
        }
    }

    public void ToSettings()
    {
        currentPauseState = PauseState.Settings;
    }

    public void ToDefault()
    {
        currentPauseState = PauseState.Default;
    }

    public void ActionQuit()
    {
        SaveGame();
        // Quit the application (Faire repaser au menu principal)
        Application.Quit();
    }

    public void SaveGame()
    {
        GameManager.Instance.SaveCurrentGame();
    }
}
