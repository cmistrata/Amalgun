using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabsManager : MonoBehaviour
{
    public static PrefabsManager Instance;
    public GameObject EnemyDeathEffect;
    public GameObject PlayerDeathEffect;
    public Sprite[] Bases;
    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }

    public (Sprite, Sprite) GetRandomEnemyAndPlayerBase()
    {
        int baseIndex = Random.Range(0, Bases.Length / 2);
        var enemyBase = Bases[Bases.Length / 2 + baseIndex];
        var playerBase = Bases[baseIndex];
        return (enemyBase, playerBase);
    }
}
