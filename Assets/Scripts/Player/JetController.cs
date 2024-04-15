using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CellMover))]
public class JetController : MonoBehaviour
{
    public float Scale = .03f;

    public ParticleSystem LeftBackJet;
    private ParticleSystem.EmissionModule _leftBackJetEmission;
    public ParticleSystem LeftSideJet;
    private ParticleSystem.EmissionModule _leftSideJetEmission;
    public ParticleSystem LeftFrontJet;
    private ParticleSystem.EmissionModule _leftFrontJetEmission;
    public ParticleSystem RightBackJet;
    private ParticleSystem.EmissionModule _rightBackJetEmission;
    public ParticleSystem RightSideJet;
    private ParticleSystem.EmissionModule _rightSideJetEmission;
    public ParticleSystem RightFrontJet;
    private ParticleSystem.EmissionModule _rightFrontJetEmission;

    private List<ParticleSystem> _particleSystems;

    private const float _rotationPower = 15f;
    private const float _translationPower = 15f;

    private void Awake() {
        _leftBackJetEmission = LeftBackJet.emission;
        _leftSideJetEmission = LeftSideJet.emission;
        _leftFrontJetEmission = LeftFrontJet.emission;
        _rightBackJetEmission = RightBackJet.emission;
        _rightSideJetEmission = RightSideJet.emission;
        _rightFrontJetEmission = RightFrontJet.emission;

        _particleSystems = new List<ParticleSystem> { LeftBackJet, LeftSideJet, LeftFrontJet, RightBackJet, RightSideJet, RightFrontJet };
        
    }

    public void UpdateScale() {
        foreach (var particleSystem in _particleSystems) {
            var particleSystemMain = particleSystem.main;
            particleSystemMain.startSize = Scale;

            var particleSystemShape = particleSystem.shape;
            particleSystemShape.scale = new(Scale, Scale, Scale);
        }
    }

    // Update is called once per frame
    void Update() 
    {
        Vector3 globalTargetDirection = Vector3.zero;
        Vector3 localTargetDirection = transform.InverseTransformVector(globalTargetDirection).normalized;
        float lateralMovement = localTargetDirection.x;
        float forwardMovement = localTargetDirection.z;
        float clockwiseRotationInput = Input.GetAxis("Rotate Clockwise");

        float leftBackJetPower = 0;
        float leftSideJetPower = 0;
        float leftFrontJetPower = 0;
        float rightBackJetPower = 0;
        float rightSideJetPower = 0;
        float rightFrontJetPower = 0;

        if (clockwiseRotationInput > 0) {
            leftBackJetPower += _rotationPower * clockwiseRotationInput;
            rightFrontJetPower += _rotationPower * clockwiseRotationInput;
        } else if (clockwiseRotationInput < 0) {
            leftFrontJetPower += _rotationPower * Mathf.Abs(clockwiseRotationInput);
            rightBackJetPower += _rotationPower * Mathf.Abs(clockwiseRotationInput);
        }

        if (lateralMovement > 0) {
            leftSideJetPower += _translationPower * lateralMovement;
        } else if (lateralMovement < 0) {
            rightSideJetPower += _translationPower * Mathf.Abs(lateralMovement);
        }

        if (forwardMovement > 0) {
            leftBackJetPower += (_translationPower/2) * forwardMovement;
            rightBackJetPower += (_translationPower/2) * forwardMovement;
        } else if (forwardMovement < 0) {
            leftFrontJetPower += (_translationPower/2) * Mathf.Abs(forwardMovement);
            rightFrontJetPower += (_translationPower/2) * Mathf.Abs(forwardMovement);
        }

        _leftBackJetEmission.rateOverTime = leftBackJetPower;
        _leftSideJetEmission.rateOverTime = leftSideJetPower;
        _leftFrontJetEmission.rateOverTime = leftFrontJetPower;
        _rightBackJetEmission.rateOverTime = rightBackJetPower;
        _rightSideJetEmission.rateOverTime = rightSideJetPower;
        _rightFrontJetEmission.rateOverTime = rightFrontJetPower;
    }
}
