using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform player;

    // Update is called once per frame
    void Update()
    {
        Debug.Log($"Player position is {player.position}");
        transform.position = player.position;
    }
}
