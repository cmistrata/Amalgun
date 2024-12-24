using UnityEngine;

public class ShieldModule : CellModule {
    private ParticleSystem.MainModule _shieldParticleSystemMainModule;
    public AudioSource ShieldActivateAudioSource;
    public AudioSource ShieldHitAudioSource;
    public AudioSource ShieldDeactivateAudioSource;
    public ShieldCollider ShieldCollider;
    public float ShieldRechargeTime;
    public int MaxShieldHealth;
    private int _currentShieldHealth;

    protected override void ExtraAwake() {
        ShieldCollider.SignalShieldHit += HandleShieldHit;
        _currentShieldHealth = MaxShieldHealth;
        _shieldParticleSystemMainModule = ShieldCollider.gameObject.GetComponent<ParticleSystem>().main;
    }

    private void OnDestroy() {
        ShieldCollider.SignalShieldHit -= HandleShieldHit;
    }

    void HandleShieldHit() {
        _currentShieldHealth -= 1;
        if (_currentShieldHealth <= 0) {
            BreakShield();
        }
        else {
            ShieldHitAudioSource.Play();
        }
    }

    void BreakShield() {
        _currentShieldHealth = 0;
        ShieldCollider.gameObject.SetActive(false);
        if (InActiveState()) {
            ShieldDeactivateAudioSource.Play();
            Invoke(nameof(RechargeShield), ShieldRechargeTime);
        }
    }

    void RechargeShield() {
        if (!InActiveState()) {
            return;
        }
        if (_currentShieldHealth <= 0) {
            _currentShieldHealth = MaxShieldHealth;
            ShieldActivateAudioSource.Play();
        }
        ShieldCollider.gameObject.SetActive(true);
    }

    protected override void HandleStateChange(CellState newState) {
        if (InActiveState()) {
            if (_state == CellState.Enemy) {
                ShieldCollider.gameObject.layer = Layers.EnemyForceField;
                _shieldParticleSystemMainModule.startColor = Color.red;
            }
            else {
                ShieldCollider.gameObject.layer = Layers.PlayerForceField;
                if (_state == CellState.Melded) {
                    _shieldParticleSystemMainModule.startColor = Color.green;
                }
                else {
                    _shieldParticleSystemMainModule.startColor = Color.blue;
                }
            }
            RechargeShield();
        }
        else {
            BreakShield();
            ShieldCollider.gameObject.layer = Layers.NoCollision;
        }
    }
}
