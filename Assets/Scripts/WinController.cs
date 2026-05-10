using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

// Управление кнопками на экране победы:
// "Заново" — перезагружает игровой уровень,
// "В меню" — выходит из комнаты Photon и возвращается в главное меню.
public class WinController : MonoBehaviour
{
    public string levelSceneName = "Level1";
    public string menuSceneName = "MainMenu";

    public void RestartLevel()
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
        {
            // Только мастер грузит уровень — остальные подтянутся через AutomaticallySyncScene
            PhotonNetwork.LoadLevel(levelSceneName);
        }
        else if (!PhotonNetwork.InRoom)
        {
            SceneManager.LoadScene(levelSceneName);
        }
    }

    public void BackToMenu()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        SceneManager.LoadScene(menuSceneName);
    }
}
