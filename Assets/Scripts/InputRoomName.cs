using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputRoomName : MonoBehaviour {

    #region Public Variables
    public string roomName;
    public InputField inputtext;
    #endregion
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    #region Public Methods
    public void Join()
    {
        // 获取两个对象
        inputtext = FindObjectOfType<InputField>();
        // 房间名为空时返回
        if (string.IsNullOrEmpty(inputtext.text) == true)
        {
            Debug.LogWarning("Room name should not be null");
            return;
        }
        // 获取房间名和是否匿名
        roomName = inputtext.text;
        if (PhotonNetwork.connected)
        {
            PhotonNetwork.JoinRoom(roomName);
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
}
