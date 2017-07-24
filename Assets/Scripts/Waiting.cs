using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using System;
using System.Collections;

public class Waiting : Photon.PunBehaviour
{
    #region Private Variables
    // 当前房间
    Room thisRoom;
    // 玩家卡片和提示文字
    Text userName;
    Text isReady;
    RawImage panel;
    GameObject roomInfo;
    Text roomName;
    Text roomAnonymous;
    // 特殊成员：自己/房主
    PhotonPlayer me;
    PhotonPlayer master;
    // 房主操作
    GameObject begin;
    GameObject[] pops;
    GameObject quit;

    //阵营
    Flag flag;
    int maxPlayerOneSide = 4;
    int playerId;

    public PlayerCard[] playercards;
    // 玩家属性
    ExitGames.Client.Photon.Hashtable playerProperties;
    #endregion
    #region PunBehaviour CallBacks
    // Use this for initialization
    void Start () {
        // 获取对象
        begin = GameObject.Find("/Canvas/buttons/Begin");
        pops = GameObject.FindGameObjectsWithTag("Pop");
        roomInfo = GameObject.Find("/Canvas/Info");
        thisRoom = PhotonNetwork.room;
        me = PhotonNetwork.player;
        master = PhotonNetwork.masterClient;
        playerProperties = PhotonNetwork.player.CustomProperties;
        for (int id = 0; id < 8; id++)
        {
            playercards[id].id = id;
        }

        GetFlag();
        SetFlag();
        SetReady(me, false);
    }
	
	// Update is called once per frame
	void Update () {
        
    }
    void OnGUI()
    {
        foreach (GameObject pop in pops)
        {
            if (PhotonNetwork.isMasterClient)
            {
                pop.SetActive(true);
            }
            else
            {
                pop.SetActive(false);
            }
        }
        // 显示房间信息
        roomName = roomInfo.GetComponentsInChildren<Text>()[0];
        roomAnonymous = roomInfo.GetComponentsInChildren<Text>()[1];
        if (PhotonNetwork.room != null)
        {
            roomName.text = (PhotonNetwork.room == null ? "" : PhotonNetwork.room.Name);
            roomAnonymous.text = (PhotonNetwork.room.IsVisible ? "公开" : "隐藏");
        }
        // 初始化玩家序列
        // 将玩家信息展示在玩家卡片上
        int id;
        for (id = 0; id < 8; id++)
        {
            playercards[id].used = 0;
        }
        foreach (PhotonPlayer player in PhotonNetwork.playerList)
        {
            // 防止玩家离开但还未被删除时报错
            if (!player.AllProperties.ContainsKey("index"))
            {
                continue;
            }
            id = (int)player.AllProperties["index"];
            playercards[id].used = 1;

            userName =playercards[id].card.GetComponentsInChildren<Text>()[0];
            isReady = playercards[id].card.GetComponentsInChildren<Text>()[1];
            // 是否准备好
            isReady.text = (bool)player.AllProperties["isReady"] ? "准备" : "未准备";
            // 显示自己/房主/玩家昵称
            if (player.Equals(master) && !player.Equals(me))
            {
                userName.text = player.NickName + "/房主";
            }
            else if (!player.Equals(master) && player.Equals(me))
            {
                userName.text = player.NickName + "/自己";
            }
            else if (player.Equals(master)&& player.Equals(me))
            {
                userName.text = player.NickName + "/自己/房主";
                // 不能踢自己
                Transform transform = playercards[id].card.transform.Find("Pop");
                transform.gameObject.SetActive(false);
            }
            else
            {
                userName.text = player.NickName + "/玩家";
            }
        }
        // 没有玩家的卡片置空
        for (id=0; id < 8; id++)
        {
            if (playercards[id].used == 0)
            {
                Transform transform = playercards[id].card.transform.Find("Pop");
                transform.gameObject.SetActive(false);
                userName = playercards[id].card.GetComponentsInChildren<Text>()[0];
                isReady = playercards[id].card.GetComponentsInChildren<Text>()[1];
                userName.text = "空";
                isReady.text = "";
            }
        }
        // 只有房主显示踢人和开始按钮
        if (!PhotonNetwork.isMasterClient)
        {
            begin.SetActive(false);
        }
    }
    #endregion
    #region Public Methods
    // 设置准备
    public void Ready(GameObject readyInfo)
    {
        if ((bool)me.AllProperties["isReady"])
        {
            SetReady(me, false);
            readyInfo.GetComponent<Text>().text = "准备";
        }
        else
        {
            SetReady(me, true);
            readyInfo.GetComponent<Text>().text = "取消准备";
        }
    }
    // 离开房间
    public void Back()
    {
        ReturnFlag(me);
        PhotonNetwork.LeaveRoom();
    }
    // 开始游戏
    public void Begin()
    {
        // 开始游戏逻辑写在这里
        PhotonNetwork.LoadLevel("tankField");
    }
    // 踢人
    public void Pop(int i)
    {
        PhotonPlayer person = null;
        // 查找按钮的父对象玩家卡片对应的序号
                //根据玩家卡片中关联的ID查找玩家
        foreach(PhotonPlayer p in PhotonNetwork.playerList)
        {
            if((int)p.CustomProperties["index"] == i)
            {
                person = p;
                break;
            }
        }
        // 断开他的连接
        ReturnFlag(person);
        playercards[i].used = 0;
        playercards[i].id = -1;
        PhotonNetwork.CloseConnection(person);
    }
    #endregion
    #region Private Methods
    // 封装复用方法，给自己分配一个序号
    // RoomInfo 里的公共变量string "1234567" 来保存序号
    public void GetFlag()
    {
        ExitGames.Client.Photon.Hashtable roomProperties = thisRoom.CustomProperties;
        if (!roomProperties.ContainsKey("RedCount"))
        {
            roomProperties.Add("RedCount", 1);
            flag = 0;
            roomProperties.Add("RedIds", "123");
            playerId = 0;
        }
        else if ((int)roomProperties["RedCount"] == maxPlayerOneSide)
        {
            if(!roomProperties.ContainsKey("BlueCount"))
            {
                roomProperties.Add("BlueCount", 1);
                flag = Flag.Blue;
                roomProperties.Add("BlueIds", "567");
                playerId = 4;
            }
            else
            {
                roomProperties["BlueCount"] = (int)roomProperties["BlueCount"]+1;
                flag = Flag.Red;
                playerId = ((string)roomProperties["BlueIds"])[0] - '0';
                roomProperties["BlueIds"] = ((string)roomProperties["BlueIds"]).Substring(1);
            }
        }
        else
        {
            roomProperties["RedCount"] = (int)roomProperties["RedCount"] + 1;
            playerId = ((string)roomProperties["RedIds"])[0] - '0';
            roomProperties["RedIds"] = ((string)roomProperties["RedIds"]).Substring(1);
        }
        thisRoom.SetCustomProperties(roomProperties);
    }
    public int GetFlag(Flag preferredFlag)
    {
        ExitGames.Client.Photon.Hashtable roomProperties = thisRoom.CustomProperties;
        string flagCountKey = "RedCount";
        string flagIdKey = "RedIds";
        string idStr = "123";
        int returnId = -1;
        switch(preferredFlag)
        {
            case Flag.Red: flagCountKey = "RedCount"; flagIdKey = "RedIds"; idStr = "123"; break;
            case Flag.Blue:flagCountKey = "BlueCount"; flagIdKey = "BlueIds"; idStr = "567"; break;
            default:;break;
        }
        if (!roomProperties.ContainsKey(flagCountKey))
        {
            roomProperties.Add(flagCountKey, 1);
            roomProperties.Add(flagIdKey, idStr);
            returnId = (preferredFlag == Flag.Red) ? 0 : 4;
        }
        else if ((int)roomProperties["RedCount"] == maxPlayerOneSide)
        {
            return -1;
        }
        else
        {
            roomProperties[flagCountKey] = (int)roomProperties[flagCountKey] + 1;
            returnId = ((string)roomProperties[flagIdKey])[0] - '0';
            roomProperties[flagIdKey] = ((string)roomProperties[flagIdKey]).Substring(1);
        }
        return returnId;
    }
    //设置自己的阵营和编号
    public void SetFlag()
    {
        playerProperties = PhotonNetwork.player.CustomProperties;
        if (playerProperties.ContainsKey("index"))
        {
            playerProperties["index"] = playerId;
        }
        else
        {
            playerProperties.Add("index", playerId);
        }
        if (playerProperties.ContainsKey("flag"))
        {
            playerProperties["flag"] = flag;
        }
        else
        {
            playerProperties.Add("flag", flag);
        }
        PhotonNetwork.player.SetCustomProperties(playerProperties);
    }
    public void ReturnFlag(PhotonPlayer player)
    {
        ExitGames.Client.Photon.Hashtable roomProperties = thisRoom.CustomProperties;
        Flag playerFlag = (Flag)player.CustomProperties["flag"];
        char id = (char)('0' + (char)((int)player.CustomProperties["index"]));
        if(playerFlag == Flag.Red)
        {
            roomProperties["RedCount"] = (int)roomProperties["RedCount"] - 1;
            roomProperties["RedIds"] = (string)roomProperties["RedIds"]+ id;
        }
        else
        {
            roomProperties["BlueCount"] = (int)roomProperties["BlueCount"] - 1;
            roomProperties["BlueIds"] = (string)roomProperties["BlueIds"] + id;
        }
        thisRoom.SetCustomProperties(roomProperties);
    }
    public void ChangeFlag()
    {
        Flag preferredFlag = (flag == Flag.Red) ? Flag.Blue : Flag.Red;
        int newId = GetFlag(preferredFlag);
        if (newId == -1) 
        {
            return;
        }
        flag = preferredFlag;
        playerId = newId;
        ReturnFlag(PhotonNetwork.player);
        SetFlag();
    }
    public void SetReady(PhotonPlayer player, bool isReady)
    {
        playerProperties = PhotonNetwork.player.CustomProperties;
        
        if (playerProperties.ContainsKey("isReady"))
        {
            playerProperties["isReady"] = isReady;
        }
        else
        {
            playerProperties.Add("isReady", isReady);
        }
        player.SetCustomProperties(playerProperties);
    }
    #endregion
    #region Photon.PunBehaviour CallBacks
    // 当玩家进入房间时
    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        base.OnPhotonPlayerConnected(newPlayer);
    }
    // 复写方法，当有玩家离开房间时
    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        // 刷新房主
        master = PhotonNetwork.masterClient;
        // 更新所有UI
        begin.SetActive(true);
        if (me.Equals(master))
        {
            //ReIndex(otherPlayer);
            ReturnFlag(otherPlayer);
        }

    }
    // 复写方法，当自己离开房间时
    public override void OnLeftRoom()
    {
        //销毁自己的序号
        PhotonNetwork.LoadLevel("Launcher");
    }
    #endregion
}
