using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public enum Effect {
    RedSmoke,
    ToonExplosion,
    Confetti,
    KnockoutHit,
}

public class EffectsManager : MonoBehaviour {
    public static EffectsManager Instance;
    public VisualEffect RedSmokeEffect;
    public VisualEffect ToonExplosionEffect;
    public GameObject DirectionalConfetti;
    public GameObject KnockoutHit;

    static HashSet<Effect> visualEffects = new HashSet<Effect>() { Effect.RedSmoke, Effect.ToonExplosion };

    void Awake() {
        Instance = this;
    }

    public static void InstantiateEffect(Effect effect, Vector3 position) {
        if (visualEffects.Contains(effect)) {
            VisualEffect visualEffect =
                effect == Effect.RedSmoke ? Instance.RedSmokeEffect
                : effect == Effect.ToonExplosion ? Instance.ToonExplosionEffect
                : null;
            var instantiatedEffect = Instantiate(visualEffect, position: position, rotation: Quaternion.identity, Containers.Effects);
            Destroy(instantiatedEffect, 2);
        }
        else {
            GameObject gameObjectEffect =
                effect == Effect.Confetti ? Instance.DirectionalConfetti
                : effect == Effect.KnockoutHit ? Instance.KnockoutHit
                : null;
            var instantiatedEffect = Instantiate(gameObjectEffect, position: position, rotation: Quaternion.identity, Containers.Effects);
            Destroy(instantiatedEffect, 2);
        }

    }
}
