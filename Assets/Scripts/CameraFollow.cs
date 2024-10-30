using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;
    void Start()
    {
        offset = new(0, 0, -1);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = player.position + offset;
    }
}
