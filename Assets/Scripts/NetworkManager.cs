using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

// Лобби: подключение к Photon + создание/вход в комнату
public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public TMP_InputField roomNameInput;   // Поле ввода имени комнаты
    public TMP_Text statusText;            // Текст статуса (что сейчас делаем)
    public Button createRoomButton;
    public Button joinRoomButton;

    [Header("Scene")]
    public string firstLevelName = "Level1";

    void Start()
    {
        SetInteractable(false);
        if (statusText != null) statusText.text = "Подключение к Photon...";

        // Синхронизация сцен: когда мастер грузит уровень — все автоматом переходят
        PhotonNetwork.AutomaticallySyncScene = true;

        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
        else
            OnConnectedToMaster();
    }

    public override void OnConnectedToMaster()
    {
        if (statusText != null) statusText.text = "Подключено. Создай или войди в комнату.";
        SetInteractable(true);
        PhotonNetwork.JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (statusText != null) statusText.text = "Отключено: " + cause;
        SetInteractable(false);
    }

    // Кнопка "Создать комнату"
    public void CreateRoom()
    {
        string roomName = roomNameInput != null ? roomNameInput.text.Trim() : "";
        if (string.IsNullOrEmpty(roomName))
        {
            if (statusText != null) statusText.text = "Введи имя комнаты!";
            return;
        }
        var options = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(roomName, options);
        if (statusText != null) statusText.text = "Создаём комнату...";
    }

    // Кнопка "Войти в комнату"
    public void JoinRoom()
    {
        string roomName = roomNameInput != null ? roomNameInput.text.Trim() : "";
        if (string.IsNullOrEmpty(roomName))
        {
            if (statusText != null) statusText.text = "Введи имя комнаты!";
            return;
        }
        PhotonNetwork.JoinRoom(roomName);
        if (statusText != null) statusText.text = "Заходим в комнату...";
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        if (statusText != null) statusText.text = "Не создать: " + message;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (statusText != null) statusText.text = "Не войти: " + message;
    }

    public override void OnJoinedRoom()
    {
        if (statusText != null) statusText.text = "В комнате " + PhotonNetwork.CurrentRoom.Name +
            " (" + PhotonNetwork.CurrentRoom.PlayerCount + "/2). Ждём игрока...";

        // Если мастер уже один в комнате — ждём второго
        // Если это второй игрок — значит нас двое, грузим уровень
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel(firstLevelName);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (statusText != null) statusText.text = "Игрок зашёл (" +
            PhotonNetwork.CurrentRoom.PlayerCount + "/2)";

        // Когда второй игрок зашёл — мастер грузит уровень (все последуют за ним)
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel(firstLevelName);
    }

    void SetInteractable(bool state)
    {
        if (createRoomButton != null) createRoomButton.interactable = state;
        if (joinRoomButton != null) joinRoomButton.interactable = state;
    }
}
