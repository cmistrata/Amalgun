using System;
using UnityEngine;
using UnityEngine.VFX;

public enum Effect {
    RedSmoke,
    ToonExplosion
}

public class EffectsManager : MonoBehaviour {
    public static EffectsManager Instance;
    public VisualEffect RedSmokeEffect;
    public VisualEffect ToonExplosionEffect;

    void Awake() {
        Instance = this;
    }

    public static void InstantiateEffect(Effect effect, Vector3 position) {
        VisualEffect visualEffect =
            effect == Effect.RedSmoke ? Instance.RedSmokeEffect
            : effect == Effect.ToonExplosion ? Instance.ToonExplosionEffect
            : null;
        var instantiatedEffect = Instantiate(visualEffect, position: position, rotation: Quaternion.identity, Containers.Effects);
        Destroy(instantiatedEffect, 2);
    }
}
