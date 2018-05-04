//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//
// TODO: 1. Saving in Settings_AcceptChanges()
//       2. Adjusting slider positions in OpenSettings()
//       3. Brightness
//       4. Volume
//*************************************************************************************************

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    bool isPauseMenuEnabled;
    [SerializeField] GameObject pauseMenuPanel;
    [SerializeField] GameObject pauseMenuOptions;

    void Start()
    {
        ClosePauseMenu();
        CloseSettings();
    }

    public void TogglePauseMenu()
    {
        if (isPauseMenuEnabled) {
            ClosePauseMenu();
        } else {
            OpenPauseMenu();
        }
    }

    void OpenPauseMenu()
    {
        isPauseMenuEnabled = true;
        pauseMenuPanel.SetActive(true);
    }

    void ClosePauseMenu()
    {
        if (isSettingsMenuEnabled)
        {
            CloseSettings();
        }

        isPauseMenuEnabled = false;
        pauseMenuPanel.SetActive(false);
    }

    #region Settings
    bool isSettingsMenuEnabled;
    [SerializeField] GameObject settingsMenuOptions;

    public void OpenSettings()
    {
        //TODO- Adjusting slider positions

        isSettingsMenuEnabled = true;
        pauseMenuOptions.SetActive(false);
        settingsMenuOptions.SetActive(true);
    }

    void CloseSettings()
    {
        isSettingsMenuEnabled = false;
        settingsMenuOptions.SetActive(false);
        pauseMenuOptions.SetActive(true);
    }

    public void Settings_AcceptChanges()
    {
        //TODO- Save

        CloseSettings();
    }

    public void Settings_CancelChanges()
    {
        CloseSettings();
    }
    #endregion

    #region Volume
    float volume;

    public void SetVolume(float v)
    {
        //TODO
    }
    #endregion

    #region Brightness
    float brightness;

    public void SetBrightness(float b)
    {
        //TODO
    }
    #endregion

    #region Saving/Loading
    [SerializeField] int mainMenuSceneIndex = 0;
    [SerializeField] int newSimulationSceneIndex = 1;

    public void LoadScene_MainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneIndex, LoadSceneMode.Single);
    }

    public void LoadScene_NewSimulation()
    {
        SceneManager.LoadScene(newSimulationSceneIndex, LoadSceneMode.Single);
    }

    [SerializeField] InputField inputField;
    [SerializeField] LevelEditor levelEditor;

    public void Save()
    {
        levelEditor.Save(inputField.text);
    }
    #endregion
}
