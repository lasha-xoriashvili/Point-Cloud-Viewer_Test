using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public static class ClientManager
{
    public static User MyUser { get; private set; }
    public static List<User> OtherUsers { get; private set; } = new();
    public static event Action OnServerConnected;
    public static event Action OnCreatedRoom;
    public static event Action OnJoinedRoom;
    public static event Action OnJoinedUser;
    public static event Action OnStartGame;
    public static event Action<string> OnGetMessage;

    private static UdpClient client;

    private static string ServerIp = "interface-cloudy.gl.at.ply.gg";
    private static int port = 24273;
    public static async void ConnectServer(int roomId,string userName)
    {
        MyUser = new User(userName,roomId);
        client = new UdpClient();
        client.Connect(ServerIp,port);

        await SendMessage($"[NewUser]{JsonConvert.SerializeObject(MyUser)}");

        _=ReceiveMessage();
    }

    private static async Task ReceiveMessage()
    {
        if (client == null) return;
        while (true) 
         {
             try
             {
                UdpReceiveResult result = await client.ReceiveAsync();
                string receiveMessage = Encoding.UTF8.GetString(result.Buffer);
                if (receiveMessage.StartsWith("[ConnectToServer]"))
                {
                     ConnectToServer();
                }
                else if (receiveMessage.StartsWith("[CreateRoom]"))
                {
                    CreatedRoom();
                }
                else if (receiveMessage.StartsWith("[JoinedRoom]"))
                {
                    JoinedRoom(receiveMessage.Substring("[JoinedRoom]".Length));
                }
                else if (receiveMessage.StartsWith("[JoinPlayer]"))
                {
                    JoinPlayer(receiveMessage.Substring("[JoinPlayer]".Length));
                }
                else if (receiveMessage.StartsWith("[StartGame]"))
                {
                    
                    string otherUserData = receiveMessage.Substring("[StartGame]".Length);
                    StartGame(otherUserData);
                }
                else if (receiveMessage.StartsWith($"[MessageFromUser]"))
                {
                    string messageData = receiveMessage.Substring($"[MessageFromUser]".Length);
                    GetMessageData(messageData);
                }
             }
             catch (Exception e) 
             {
                 Debug.LogError(e.Message);
                 Disconnect();
             }
         }
    }

    private static void JoinPlayer(string messageData)
    {
        Debug.Log(messageData);
        var user = JsonConvert.DeserializeObject<User>(messageData);
        OtherUsers.Add(user);
        OnJoinedUser.Invoke();
    }

    private static void GetMessageData(string messageData)
    {
        string senderUserId = messageData.Split('[', ']')[1];
        string message = messageData.Substring($"[{senderUserId}]".Length);
        OnGetMessage?.Invoke(message);
    }

    private static void StartGame(string OtherUserData)
    {
        Debug.Log($"დაიწყო თამაში {OtherUserData}");
        List<User> otherUser = JsonConvert.DeserializeObject<List<User>>(OtherUserData);
        OtherUsers = otherUser;
        OnStartGame?.Invoke();
    }

    private static void JoinedRoom(string messageData)
    {
        Debug.Log("შეუერთდი ოთახს ოთახს");

        User user = JsonConvert.DeserializeObject<User>(messageData);

        OtherUsers.Add(user);

        OnJoinedRoom?.Invoke();
    }

    private static void CreatedRoom()
    {
        Debug.Log("შეიქმნა ოთახი");
        OnCreatedRoom?.Invoke();
    }

    private static void ConnectToServer()
    {
        Debug.Log("Connected To Server");
        OnServerConnected?.Invoke();
    }
    
    public static async Task SendMessage(string message)
    {
        byte[] data =Encoding.UTF8.GetBytes(message);
        if (client.Client.Connected)
        {
            await client.SendAsync(data, data.Length);
        }
        else
        {
            Debug.LogError("კლიენტი არ არის შექმნილი");
        }
    }

    public static void Disconnect()
    {
        try
        {
            //if(client != null && client.Connected)
            //{
            //    await SendMessage("OnClientDisconnected:" + MyUser.ID);
            //    writer.Flush();

            //    stream.Close();
            //    reader.Close();
            //    writer.Close();

            //    client.Close();
            //}
            //else
            //{
            //    OnServerFailed.Invoke("failed");
            //}
        }
        catch (Exception e)
        {
            
        }
    }
}

[Serializable]
public class User
{
    public string ID { get; set; }
    public string Name { get; set; }
    public int RoomId { get; set; }
    public DateTime ConnectedAt { get; set; } = DateTime.Now;

    public User(string name,int roomId,string Id = "")
    {
        ID = Id != "" ? Id : Guid.NewGuid().ToString();
        ConnectedAt = DateTime.Now;
        Name = name;
        RoomId = roomId;
    }
}