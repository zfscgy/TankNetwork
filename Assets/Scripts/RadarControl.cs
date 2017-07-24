using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarControl : MonoBehaviour {
    public Transform radar;
    public List<Transform> TrackedObjects = new List<Transform>();
    public List<Transform> FriendObjects = new List<Transform>();
    public Texture radarPoint;
    public RectTransform minimap;
    private int viewPortSize = 256;
    private int frameCount = 0;

    public float maxSensorRange = 100f;
    // Use this for initialization
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {
        if (frameCount % 16 == 0)
        {
            foreach(Transform enemy in TrackedObjects)
            {
                if((enemy.position - radar.position).magnitude > maxSensorRange)
                {
                    transform.GetComponent<TankControl>().radarPoint.SetActive(false);
                }
                else
                {
                    transform.GetComponent<TankControl>().radarPoint.SetActive(true);
                }
            }
        }

    }

}
