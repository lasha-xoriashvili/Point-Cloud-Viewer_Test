using Newtonsoft.Json;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject registrationPanel, loadingPanel, chatPanel;

    [Header("Registration")]
    [SerializeField] private TMP_InputField serverIPText,serverPortText,userNameText;
    [SerializeField] private Button connectBTN;

    [Header("Loading")]
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("Chat")]
    [SerializeField] private TMP_InputField sendText;
    [SerializeField] private TextMeshProUGUI getText, myUserName, otherUserName;

    private void Start()
    {
        registrationPanel.SetActive(true);

        connectBTN.onClick.AddListener(ConnectingServer);

        ClientManager.OnServerConnected += OnServerConnected;
        ClientManager.OnJoinedUser += OnJoinedUser;
        ClientManager.OnJoinedRoom += OnJoinedRoom;
        ClientManager.OnStartGame += OnStartChating;
        ClientManager.OnGetMessage += OnGetMessage;

        loadingPanel.SetActive(false);
        chatPanel.SetActive(false);
    }

    private void OnJoinedRoom()
    {
        Debug.Log($"Joined Server User: {ClientManager.OtherUsers[0].Name}");
        otherUserName.text = ClientManager.OtherUsers[0].Name;
    }

    private void OnJoinedUser()
    {
        Debug.Log($"Joined Server User: {ClientManager.OtherUsers[0].Name}");
        otherUserName.text = ClientManager.OtherUsers[0].Name;
    }

    private void OnDisconnectClientFromServer(User user)
    {
        otherUserName.text = "loadingUser...";
        getText.text = "until user do not connected";
    }

    private void OnGetMessage(string message)
    {
        getText.text = message;
    }

    private void OnStartChating()
    {
        otherUserName.text = ClientManager.OtherUsers[0].Name;
        sendText.interactable = true;
        sendText.textComponent.textWrappingMode = TextWrappingModes.Normal;
        sendText.onValueChanged.AddListener(SendMessageToOther);
        if (!string.IsNullOrEmpty(sendText.text)) SendMessageToOther(sendText.text);
    }

    private void SendMessageToOther(string message)
    {
        _= ClientManager.SendMessage($"[MessageFromUser][{ClientManager.MyUser.RoomId}][{ClientManager.OtherUsers[0].ID}]{message}");
    }

    private void OnReciveMessage(string message)
    {
        Debug.Log(message);
    }

    private void OnServerFailed(string message)
    {
        loadingPanel.SetActive(false);
        chatPanel.SetActive(false);
        registrationPanel.SetActive(true);
        serverIPText.text = "";
        serverPortText.text = "";
        userNameText.text = "";
    }

    private void OnServerDisconnected(string message)
    {
        loadingPanel.SetActive(false);
        chatPanel.SetActive(false);
        registrationPanel.SetActive(true);
        serverIPText.text = "";
        serverPortText.text = "";
        userNameText.text = "";
    }

    private void OnServerConnected()
    {
        registrationPanel.SetActive(false);
        loadingPanel.SetActive(false);
        chatPanel.SetActive(true);
        myUserName.text = ClientManager.MyUser.Name;
        sendText.interactable = false;
        otherUserName.text = "loadingUser...";
        getText.text = "until user do not connected";
    }

    private void ConnectingServer()
    {
        ClientManager.ConnectServer(int.Parse(serverPortText.text),userNameText.text);
        registrationPanel.SetActive(false);
        loadingPanel.SetActive(true);
        loadingText.text = "Connecting...";
    }

    private void OnApplicationQuit()
    {
        ClientManager.Disconnect();
    }
}
