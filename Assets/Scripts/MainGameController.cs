using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGameController : MonoBehaviour {
    public TankControl localPlayer;
    public List<TankControl> AllPlayers = new List<TankControl>();
    public RadarControl radarControl;
    private int frameCount = 0;
	// Use this for initialization
	void Start ()
    {
        radarControl = GetComponent<RadarControl>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(frameCount%256 == 0)
        {
            foreach(TankControl tank in AllPlayers)
            {
                if (tank != null)
                {
                    if (tank.flag == localPlayer.flag && !radarControl.FriendObjects.Contains(tank.transform))
                    {
                        radarControl.FriendObjects.Add(tank.transform);
                    }
                    else if(!radarControl.TrackedObjects.Contains(tank.transform))
                    {
                        radarControl.TrackedObjects.Add(tank.transform);
                    }
                }
            }
        }
        frameCount++;
    }
}
