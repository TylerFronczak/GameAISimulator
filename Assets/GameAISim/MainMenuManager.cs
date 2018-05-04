//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    void Awake()
    {
        ShowScreen_Main();
        IntentManager.Instance.ClearIntents();
    }

    #region Loading
    [SerializeField] int sceneBuildIndex;

    public void Button_Load_NewSimulation()
    {
        SceneManager.LoadScene(sceneBuildIndex, LoadSceneMode.Single);
    }

    public void Button_Load_SavedSimulation(string fileName)
    {
        IntentManager.Instance.CreateIntent_LoadTerrain(fileName);
        SceneManager.LoadScene(sceneBuildIndex, LoadSceneMode.Single);
    }

    public void Button_AddIntent_SimulationMode(int mode)
    {
        IntentManager.Instance.CreateIntent_LoadMode(mode);
    }
    #endregion

    #region Show/Hide Screens
    [SerializeField] GameObject mainScreen;
    [SerializeField] GameObject settingsScreen;
    [SerializeField] GameObject creditsScreen;
    [SerializeField] GameObject loadOptionsScreen;

    public void ShowScreen_Main()
    {
        HideAllScreens();
        mainScreen.SetActive(true);
    }
    public void ShowScreen_Settings()
    {
        HideAllScreens();
        settingsScreen.SetActive(true);
    }
    public void ShowScreen_Credits()
    {
        HideAllScreens();
        creditsScreen.SetActive(true);
    }
    public void ShowScreen_LoadOptions()
    {
        HideAllScreens();
        loadOptionsScreen.SetActive(true);
    }

    private void HideAllScreens()
    {
        mainScreen.SetActive(false);
        creditsScreen.SetActive(false);
        loadOptionsScreen.SetActive(false);
    }
    #endregion

    public void OpenLinkToWebsite()
    {
        Application.OpenURL("https://tylerfronczak.com");
    }

    public void ExitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
