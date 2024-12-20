using UnityEngine;


public class Rocket : MonoBehaviour {
    private Rigidbody _rb;
    private AudioSource _audioSource;
    public ParticleSystem ParticleSystem;

    private const float _startupLength = .5f;
    private bool _inStartupPhase = true;
    private readonly float _creationTime;

    private const float _thrustStrength = 30f;
    private const float _startupRotationSpeed = 20f;
    private const float _thrustingRotationSpeed = 1.4f;
    private bool _passedTarget = false;

    private void Awake() {
        _rb = GetComponent<Rigidbody>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start() {
        Invoke(nameof(ExitStartupPhase), _startupLength);
    }

    void ExitStartupPhase() {
        _inStartupPhase = false;
        _audioSource.Play();
        ParticleSystem.gameObject.SetActive(true);
    }

    void FixedUpdate() {
        Vector3 targetDirection = GetTargetPosition() - transform.position;
        _passedTarget = _passedTarget || Vector3.Dot(targetDirection, _rb.linearVelocity) < 0;
        if (!_passedTarget) {
            // Rotate the rocket towards the target
            var angle = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;
            var targetRotation = Quaternion.AngleAxis(angle, Vector3.up);
            var angleBetweenTarget = Quaternion.Angle(transform.rotation, targetRotation);
            var rotationSpeed = _inStartupPhase ? _startupRotationSpeed : _thrustingRotationSpeed;
            if (angleBetweenTarget < rotationSpeed) {
                transform.rotation = targetRotation;
            }
            else {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);
            }
        }


        if (!_inStartupPhase) {
            // Apply force in the direction the rocket is facing
            _rb.AddForce(transform.forward * _thrustStrength);
        }
    }

    Vector3 GetTargetPosition() {
        if (gameObject.layer == Layers.EnemyBullet && GameManager.Instance.Player != null) {
            return GameManager.Instance.Player.transform.position;
        }
        else if (gameObject.layer == Layers.PlayerBullet) {
            return Utils.GetMousePosition();
        }
        return Vector3.zero;
    }
}
