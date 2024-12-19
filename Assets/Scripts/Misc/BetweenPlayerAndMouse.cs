using UnityEngine;

public class BetweenPlayerAndMouse : MonoBehaviour {
    public float Factor = .2f;

    // Must be fixed update to match the player position updates.
    void FixedUpdate() {
        if (GameManager.Instance != null && GameManager.Instance.Player != null) {
            var playerPosition = GameManager.Instance.Player.transform.position;
            var mousePosition = Utils.GetMousePosition();
            var fromPlayerToMouse = mousePosition - playerPosition;
            transform.position = playerPosition + Factor * fromPlayerToMouse;
        }
    }
}
