using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Test : NetworkBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if(Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log(Time.time);
            RpcCallClient();
        }
		
	}

    [ClientRpc]
    public void RpcCallClient()
    {
        CmdReceiveFromClient(Time.time);
    }
    [Command]
    public void CmdReceiveFromClient(float time)
    {
        Debug.Log(Time.time);
        Debug.Log("Client time:" + time.ToString());
    }
}
