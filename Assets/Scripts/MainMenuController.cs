using UnityEngine;
using UnityEngine.SceneManagement;

// Логика главного меню: кнопки "Играть" и "Выход"
public class MainMenuController : MonoBehaviour
{
    public string firstLevelName = "Level1";

    public void PlayGame()
    {
        SceneManager.LoadScene(firstLevelName);
    }

    public void QuitGame()
    {
        Application.Quit();
        // В редакторе Application.Quit() ничего не делает — это нормально
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
