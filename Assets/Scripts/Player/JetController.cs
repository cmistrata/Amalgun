using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Mover))]
public class JetController : MonoBehaviour {
    public float Scale = .03f;

    private Mover _mover;

    public GameObject LeftJet;
    private List<ParticleSystem.EmissionModule> _leftParticleSystems;
    private List<ParticleSystem.EmissionModule> _leftEmitters;
    public GameObject LeftBackJet;
    private List<ParticleSystem.EmissionModule> _leftBackEmitters;
    public GameObject LeftFrontJet;
    private List<ParticleSystem.EmissionModule> _leftFrontEmitters;
    public GameObject RightJet;
    private List<ParticleSystem.EmissionModule> _rightEmitters;
    public GameObject RightBackJet;
    private List<ParticleSystem.EmissionModule> _rightBackEmitters;
    public GameObject RightFrontJet;
    private List<ParticleSystem.EmissionModule> _rightFrontEmitters;

    private List<ParticleSystem> _particleSystems;

    public AudioSource RocketSound;

    private const float _rotationPower = 7f;
    private const float _translationPower = 35f;

    private void Awake() {
        _mover = GetComponent<Mover>();
        _leftEmitters = LeftJet.GetComponentsInChildren<ParticleSystem>().Select(ps => ps.emission).ToList();
        _leftEmitters.Add(LeftJet.GetComponent<ParticleSystem>().emission);

        _leftBackEmitters = LeftBackJet.GetComponentsInChildren<ParticleSystem>().Select(ps => ps.emission).ToList();
        _leftBackEmitters.Add(LeftBackJet.GetComponent<ParticleSystem>().emission);

        _leftFrontEmitters = LeftFrontJet.GetComponentsInChildren<ParticleSystem>().Select(ps => ps.emission).ToList();
        _leftFrontEmitters.Add(LeftFrontJet.GetComponent<ParticleSystem>().emission);

        _rightEmitters = RightJet.GetComponentsInChildren<ParticleSystem>().Select(ps => ps.emission).ToList();
        _rightEmitters.Add(RightJet.GetComponent<ParticleSystem>().emission);

        _rightBackEmitters = RightBackJet.GetComponentsInChildren<ParticleSystem>().Select(ps => ps.emission).ToList();
        _rightBackEmitters.Add(RightBackJet.GetComponent<ParticleSystem>().emission);

        _rightFrontEmitters = RightFrontJet.GetComponentsInChildren<ParticleSystem>().Select(ps => ps.emission).ToList();
        _rightFrontEmitters.Add(RightFrontJet.GetComponent<ParticleSystem>().emission);


        // _particleSystems = new List<ParticleSystem> { LeftBackJet, LeftSideJet, LeftFrontJet, RightBackJet, RightSideJet, RightFrontJet };
    }

    // public void UpdateScale() {
    //     foreach (var particleSystem in _particleSystems) {
    //         var particleSystemMain = particleSystem.main;
    //         particleSystemMain.startSize = Scale;

    //         var particleSystemShape = particleSystem.shape;
    //         particleSystemShape.scale = new(Scale, Scale, Scale);
    //     }
    // }

    // Update is called once per frame
    void Update() {
        Vector3 globalTargetDirection = _mover.TargetDirection;
        Vector3 localTargetDirection = transform.InverseTransformVector(globalTargetDirection).normalized;
        float lateralMovement = localTargetDirection.x;
        float forwardMovement = localTargetDirection.z;
        float clockwiseRotationInput = Input.GetAxis("Rotate Clockwise");

        bool propelling = lateralMovement != 0 || forwardMovement != 0 || clockwiseRotationInput != 0;
        if (propelling && !RocketSound.isPlaying) {
            RocketSound.Play();
        }
        else if (!propelling && RocketSound.isPlaying) {
            RocketSound.Stop();
        }

        float leftBackJetPower = 0;
        float leftSideJetPower = 0;
        float leftFrontJetPower = 0;
        float rightBackJetPower = 0;
        float rightSideJetPower = 0;
        float rightFrontJetPower = 0;

        float effectiveTranslationPower = _translationPower;
        if (_mover.Dashing) effectiveTranslationPower *= 3;

        if (clockwiseRotationInput > 0) {
            leftBackJetPower += _rotationPower * clockwiseRotationInput;
            rightFrontJetPower += _rotationPower * clockwiseRotationInput;
        }
        else if (clockwiseRotationInput < 0) {
            leftFrontJetPower += _rotationPower * Mathf.Abs(clockwiseRotationInput);
            rightBackJetPower += _rotationPower * Mathf.Abs(clockwiseRotationInput);
        }

        if (lateralMovement > 0) {
            leftSideJetPower += effectiveTranslationPower * lateralMovement;
        }
        else if (lateralMovement < 0) {
            rightSideJetPower += effectiveTranslationPower * Mathf.Abs(lateralMovement);
        }

        if (forwardMovement > 0) {
            leftBackJetPower += (effectiveTranslationPower / 2) * forwardMovement;
            rightBackJetPower += (effectiveTranslationPower / 2) * forwardMovement;
        }
        else if (forwardMovement < 0) {
            leftFrontJetPower += (effectiveTranslationPower / 2) * Mathf.Abs(forwardMovement);
            rightFrontJetPower += (effectiveTranslationPower / 2) * Mathf.Abs(forwardMovement);
        }

        for (int i = 0; i < _leftEmitters.Count(); i++) {
            var emitter = _leftEmitters[i];
            emitter.rateOverTime = leftSideJetPower;
        }
        for (int i = 0; i < _leftBackEmitters.Count(); i++) {
            var emitter = _leftBackEmitters[i];
            emitter.rateOverTime = leftBackJetPower;
        }
        for (int i = 0; i < _leftFrontEmitters.Count(); i++) {
            var emitter = _leftFrontEmitters[i];
            emitter.rateOverTime = leftFrontJetPower;
        }
        for (int i = 0; i < _rightEmitters.Count(); i++) {
            var emitter = _rightEmitters[i];
            emitter.rateOverTime = rightSideJetPower;
        }
        for (int i = 0; i < _rightBackEmitters.Count(); i++) {
            var emitter = _rightBackEmitters[i];
            emitter.rateOverTime = rightBackJetPower;
        }
        for (int i = 0; i < _rightFrontEmitters.Count(); i++) {
            var emitter = _rightFrontEmitters[i];
            emitter.rateOverTime = rightFrontJetPower;
        }
    }
}
