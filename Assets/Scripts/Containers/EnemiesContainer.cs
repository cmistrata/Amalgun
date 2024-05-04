using UnityEngine;

public class EnemiesContainer : MonoBehaviour {
    public static EnemiesContainer Instance;

    private void Awake() {
        Instance = this;
    }
}
