using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public enum Effect {
    RedSmoke,
    ToonExplosion,
    Confetti,
    KnockoutHit,
    EnemySpawnCircle
}

public class EffectsManager : MonoBehaviour {
    public static EffectsManager Instance;
    public GameObject RedSmokeEffect;
    public GameObject ToonExplosionEffect;
    public GameObject ConfettiEffect;
    public GameObject KnockoutHit;
    public GameObject EnemySpawnCircleEffect;
    [HideInInspector]
    public Dictionary<Effect, GameObject> Effects;

    void Awake() {
        Instance = this;
        Effects = new(){
            {Effect.RedSmoke, RedSmokeEffect},
            {Effect.ToonExplosion, ToonExplosionEffect},
            {Effect.Confetti, ConfettiEffect},
            {Effect.KnockoutHit, KnockoutHit},
            {Effect.EnemySpawnCircle, EnemySpawnCircleEffect}
        };
    }

    public static void InstantiateEffect(Effect effect, Vector3 position, float duration = 2f) {
        if (Instance == null) return;
        GameObject gameObjectEffect = Instance.Effects[effect];

        var instantiatedEffect = Instantiate(gameObjectEffect, position: position + gameObjectEffect.transform.position, rotation: gameObjectEffect.transform.rotation, Containers.Effects);
        Destroy(instantiatedEffect, duration);
    }
}
