using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPart : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.GetComponent<Player>() != null || other.gameObject.GetComponent<Part>() != null) {
            GameManager.Instance.StartGame();
            Destroy(this);
        }
    }
    
}
