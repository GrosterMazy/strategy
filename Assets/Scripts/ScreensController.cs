using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreensController : MonoBehaviour
{
    public MenuScreen showOnStart = null;
    private List<MenuScreen> _menuScreensOnTheScene;
    private void Start()
    {
        _menuScreensOnTheScene = new List<MenuScreen>(FindObjectsOfType<MenuScreen>());
        ShowScreen(showOnStart);
    }
    public void LoadScene(Scene sceneToLoad)
    {
        SceneManager.LoadScene(sceneToLoad.name);
    }
    public void ShowScreen(MenuScreen menuScreenToShow)
    {
        foreach (MenuScreen menuScreen in _menuScreensOnTheScene)
        {
            menuScreen.gameObject.SetActive(false);
        }
        menuScreenToShow?.gameObject.SetActive(true);
    }
    public void Quit()
    {
        Application.Quit();
    }
}
