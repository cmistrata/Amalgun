using UnityEngine;

public class Layers {
    public static readonly int EnemyPart = LayerMask.NameToLayer("EnemyPart");
    public static readonly int PlayerPart = LayerMask.NameToLayer("PlayerPart");
    public static readonly int NeutralPart = LayerMask.NameToLayer("NeutralPart");

    public static readonly int EnemyBullet = LayerMask.NameToLayer("EnemyBullet");
    public static readonly int PlayerBullet = LayerMask.NameToLayer("PlayerBullet");

    public static readonly int EnemyForceField = LayerMask.NameToLayer("EnemyForceField");
    public static readonly int PlayerForceField = LayerMask.NameToLayer("PlayerForceField");
}