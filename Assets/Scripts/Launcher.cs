using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;


public class Launcher : Photon.PunBehaviour
{
    #region Public Variables
    /// The PUN loglevel. 
    public PhotonLogLevel Loglevel = PhotonLogLevel.Informational;
    public Text RoomInfoText;
    /// The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.
    [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
    public byte MaxPlayersPerRoom = 8;
    public string _gameVersion = "1.0";
    #endregion


    #region Private Variables
    /// <summary>
            /// This client's version number. Users are separated from each other by gameversion (which allows you to make breaking changes).
            /// </summary>
//    bool isInLobby = false;
    UnityEngine.Object room;
    Text roomName;
    Text roomNum;
    InputField nameField;
    GameObject panel;
    GameObject[] rooms;
    GameObject peopleInfo;
    GameObject roomNumInfo;
    GameObject pagesInfo;

    string appKey = "52ce8009-c83d-4bc5-90ea-61f5bb39d10a";
    InputField addressField;
    InputField portField;
    int index=1;
    RoomInfo[] Page;
    int pageNum;
    #endregion


    #region PunBehaviour CallBacks
    /// <summary>
            /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
            /// </summary>
    void Awake()
    {
        // #Critical
        // we don't join the lobby. There is no need to join a lobby to get the list of rooms.
        PhotonNetwork.autoJoinLobby = true;
        // #Critical
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.automaticallySyncScene = true;
        // #NotImportant
        // Force LogLevel
        PhotonNetwork.logLevel = Loglevel;
    }
    /// <summary>
            /// MonoBehaviour method called on GameObject by Unity during initialization phase.
            /// </summary>
    void Start()
    {
        index = 1;
        PhotonNetwork.automaticallySyncScene = true;
        // Connect when the game is start
        rooms = GameObject.FindGameObjectsWithTag("Room");
        peopleInfo = GameObject.Find("/Canvas/Info/People");
        roomNumInfo = GameObject.Find("/Canvas/Info/Rooms");
        pagesInfo = GameObject.Find("/Canvas/Info/Pages");
        nameField = GameObject.Find("/Canvas/Panel_SetName/InputField").GetComponent<InputField>();
        addressField = GameObject.Find("/Canvas/Panel_Connect/InputField_Address").GetComponent<InputField>();
        portField = GameObject.Find("/Canvas/Panel_Connect/InputField_Port").GetComponent<InputField>();
        // 房间卡片序号置0
        int i = 0;
        // 5张房间卡片隐藏
        for (i = 0; i < 5; i++)
        {
            rooms[i].SetActive(false);
        }
    }
    void OnGUI()
    {
        if (PhotonNetwork.insideLobby)
        {
            // 房间卡片序号置0
            int i = 0;
            // 5张房间卡片激活
            for (i = 0; i < 5; i++)
            {
                rooms[i].SetActive(true);
            }
            // 获得本页的房间列表
            Page = RoomSlice(index);
            pageNum = RoomPages();
            // 房间卡片序号置0
            i = 0;
            // 房间列表非空
            if (Page != null)
            {
                //打印房间信息
                int Pagenum = Page.GetLength(0);
                foreach (RoomInfo roomInfo in Page)
                {
                    Text roomName = rooms[i].GetComponentsInChildren<Text>()[0];
                    Text roomPeople = rooms[i].GetComponentsInChildren<Text>()[1];
                    roomName.text = roomInfo.Name;
                    roomPeople.text = roomInfo.PlayerCount.ToString() + '/' + roomInfo.MaxPlayers.ToString();
                    i++;
                }
            }
            // 将未使用的房间卡片隐藏
            for (; i < 5; i++)
            {
                rooms[i].SetActive(false);
            }
            peopleInfo.GetComponent<Text>().text = "在线人数：" + PhotonNetwork.countOfPlayers.ToString();
            roomNumInfo.GetComponent<Text>().text = "房间数：" + PhotonNetwork.countOfRooms.ToString();
            pagesInfo.GetComponent<Text>().text = index.ToString() + "/" + pageNum.ToString();
        }
        else
        {
            peopleInfo.GetComponent<Text>().text = "未连接！";
        }
    }


    #endregion
    #region Public Methods
    /// <summary>
            /// Start the connection process. 
            /// - If already connected, we attempt joining a random room
            /// - if not yet connected, Connect this application instance to Photon Cloud Network
            /// </summary>
      

    public void Connect()
    {
        if (!PhotonNetwork.connected)
        {
            PhotonNetwork.ConnectToMaster(addressField.text,Int32.Parse(portField.text),appKey, "1");
        }
    }
    // 按钮关联，创建房间
    public void Create()
    {
        if (PhotonNetwork.insideLobby)
        {
            PhotonNetwork.LoadLevel("CreateRoom");
        }
        else
        {
            PhotonNetwork.JoinLobby();
            Debug.LogWarning("Connect is reset.Retry...");
        }
    }
    // 按钮关联，加入指定房间
    public void Join(GameObject roomName)
    {
        if (PhotonNetwork.insideLobby)
        {
            PhotonNetwork.JoinRoom(roomName.GetComponent<Text>().text);
        }
        else
        {
            PhotonNetwork.JoinLobby();
            Debug.LogWarning("Connect is reset.Retry...");
        }
    }
    //按钮关联，随机加入房间
    public void Auto()
    {
        if (PhotonNetwork.insideLobby)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            PhotonNetwork.JoinLobby();
            Debug.LogWarning("Connect is reset.Retry...");
        }
    }
    //按钮关联，加入任意房间
    public void JoinAno()
    {
        if (PhotonNetwork.insideLobby)
        {
            PhotonNetwork.LoadLevel("InputRoomName");
        }
        else
        {
            PhotonNetwork.JoinLobby();
            Debug.LogWarning("Connect is reset.Retry...");
        }
    }
    //按钮关联，退出
    public void Quit()
    {
        Application.Quit();
    }
    //翻页按钮操作
    //回到第一页
    public void Qback()
    {
        index = 1;
    }
    //回一页
    public void Back()
    {
        index = Math.Max(index - 1, 1);
    }
    // 前进一页
    public void Forward()
    {
        index = Math.Min(index + 1, pageNum);
    }
    // 前进到头
    public void Qforward()
    {
        index = pageNum;
    }

    //按钮关联，添加昵称
    public void SetPlayerName()
    {
        PhotonNetwork.player.NickName = nameField.text;
    }
    #endregion
    #region Photon.PunBehaviour CallBacks
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        Debug.Log("DemoAnimator/Launcher: OnConnectedToMaster() was called by PUN");
    }

    public override void OnDisconnectedFromPhoton()
    {
        Debug.LogWarning("DemoAnimator/Launcher: OnDisconnectedFromPhoton() was called by PUN");
    }
    public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
    {
        Debug.Log("DemoAnimator/Launcher:OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 4}, null);");
        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        // PhotonNetwork.CreateRoom("First Room", new RoomOptions() { MaxPlayers = MaxPlayersPerRoom }, null);
        Debug.Log("Please create a room");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("DemoAnimator/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
        // #Critical: We only load if we are the first player, else we rely on  PhotonNetwork.automaticallySyncScene to sync our instance scene.
        if (PhotonNetwork.room.PlayerCount == 1)
        {
            Debug.Log("We load the main scene");
            // #Critical
            // Load the Room Level. 
            PhotonNetwork.LoadLevel("Waiting");
        }
    }

    #endregion
    #region Private Methods
    private string RoomInfoToString(RoomInfo roomInfo)
    {
        string roomInfoString = "Name:" + roomInfo.Name + "  PlayerNumber:" + roomInfo.PlayerCount.ToString() + "/" + roomInfo.MaxPlayers.ToString();
        return roomInfoString;
    }
    // 封装复用方法，获取房间列表并切片分页，序号从1开始
    private RoomInfo[] RoomSlice(int index)
    {
        int i = 0;
        int length, tail, rLength;
        RoomInfo[] roomList = PhotonNetwork.GetRoomList();
        
        length = roomList.GetLength(0);
        tail = Math.Min(index * 5, length);
        rLength = tail - 5 * index + 5;
        if (rLength < 1)
        {
            return null;
        }
        RoomInfo[] returnList = new RoomInfo[rLength];
        //5*index-5 -- tail
        for (i = 0; i < rLength; i++)
        {
            returnList[i] = roomList[index * 5 - 5 + i];
        }
        return returnList;
    }
    // 封装复用方法，获取房间列表页数
    private int RoomPages()
    {
        RoomInfo[] roomList = PhotonNetwork.GetRoomList();
        int num = roomList.GetLength(0);
        if (num == 0)
        {
            return 1;
        }
        return (int)Math.Ceiling(roomList.GetLength(0)/5.0);
    }
    #endregion

}
