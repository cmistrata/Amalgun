using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetweenPlayerAndMouse : MonoBehaviour
{
    public float Factor = .2f;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentPlayer != null) {
            var playerPosition = GameManager.Instance.CurrentPlayer.transform.position;
            var mousePosition = Utils.GetMousePosition();
            var fromPlayerToMouse = mousePosition - playerPosition;
            transform.position = playerPosition + Factor * fromPlayerToMouse;
        }
    }
}
