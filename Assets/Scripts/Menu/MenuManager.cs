using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Transform SettingsWindow;
    [SerializeField] private Transform MainMenuWindow;
    public void StartGane() { }
    public void OpenSettings() { FindObjectOfType<MenuCamera>().objectToFollow = SettingsWindow; }
    public void CloseSettings() { FindObjectOfType<MenuCamera>().objectToFollow = MainMenuWindow; }
    public void QuitGame() { Application.Quit(); }
}
