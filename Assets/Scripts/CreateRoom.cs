using System;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoom : Photon.PunBehaviour
{
    #region Public Variables
    public string roomName;
    public bool anonymous;
    public InputField inputtext;
    public Toggle toggle;
    public byte MaxPlayersPerRoom = 8;
    #endregion
    #region Private Variables
    #endregion
    #region MonoBehaviour CallBacks
    // Use this for initialization
    void Start ()
    {
        

	}
	
	// Update is called once per frame
	void Update ()
    {
        
    }
    #endregion
    #region Public Methods
    public void Create()
    {
        // 获取两个对象
        inputtext = FindObjectOfType<InputField>();
        toggle = FindObjectOfType<Toggle>();
        // 房间名为空时返回
        if (string.IsNullOrEmpty(inputtext.text) == true)
        {
            Debug.LogWarning("Room name should not be null");
            return;
        }
        // 获取房间名和是否匿名
        roomName = inputtext.text;
        anonymous = toggle.isOn;
        if (PhotonNetwork.connected)
        {
            PhotonNetwork.CreateRoom(roomName, new RoomOptions() { MaxPlayers = MaxPlayersPerRoom, IsVisible=!anonymous}, null);
        }
        else
        {
            PhotonNetwork.LoadLevel("Launcher");
            Debug.LogWarning("Connect is reset.Return to Launcher");
        }
    }

    public void Return()
    {
        PhotonNetwork.LoadLevel("Launcher");
    }
    #endregion
    #region Photon.PunBehaviour CallBacks
    public override void OnJoinedRoom()
    {
        Debug.Log("DemoAnimator/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
    }
    public override void OnCreatedRoom()
    {
        PhotonPlayer me = PhotonNetwork.player;        
        PhotonNetwork.LoadLevel("Waiting");
    }

    #endregion
}
