using UnityEngine;
using UnityEngine.VFX;

public class EffectsManager : MonoBehaviour {
    public static EffectsManager Instance;
    public VisualEffect EnemySpawnEffect;

    void Awake() {
        Instance = this;
    }

    public void InstantiateEnemySpawnEffect(Vector3 position) {
        Instantiate(EnemySpawnEffect, position: position, rotation: Quaternion.identity);
    }
}
