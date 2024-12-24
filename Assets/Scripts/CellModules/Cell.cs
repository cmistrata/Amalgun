using System;
using System.Collections.Generic;
using UnityEngine;

public enum CellState {
    Friendly,
    Enemy,
    Neutral,
    Absorbing,
    BeingAbsorbed,
    Attaching,
    Melded,
}

public enum CellType {
    None = 0,
    Basic = 1, Basic2 = 2, Basic3 = 3,
    Rocket = 4, Rocket2 = 5, Rocket3 = 6,
    Mine = 7, Mine2 = 8, Mine3 = 9,
    Tri = 10, Tri2 = 11, Tri3 = 12,
    Shield = 13, Shield2 = 14, Shield3 = 15,
    Gatling = 16, Gatling2 = 17, Gatling3 = 18
}

public class CellStats {
    public int Difficulty;
    public int Rarity;

    public CellStats(int difficulty, int rarity) {
        Difficulty = difficulty;
        Rarity = rarity;
    }
}

public class Cell : MonoBehaviour {

    public CellState State = CellState.Neutral;
    public CellType Type;
    private CellState _previousState;
    public event Action<CellState> ChangeStateEvent;
    [HideInInspector]
    public Rigidbody rb;
    private GameObject _knockoutStars;
    private GameObject _shell;
    private GameObject _armature;
    private List<GameObject> _renderedObjects = new();

    private const float _knockoutTimeout = 40f;

    // Start is called before the first frame update
    void Awake() {
        _previousState = State;
        rb = GetComponent<Rigidbody>();
    }

    void Start() {
        ChangeState(State);
        _knockoutStars = transform.Find("KnockoutStars")?.gameObject;
        _shell = transform.Find("Shell")?.gameObject;
        _armature = transform.Find("Armature")?.gameObject;
        _renderedObjects.Add(_knockoutStars);
        _renderedObjects.Add(_shell);
        _renderedObjects.Add(_armature);
    }

    private void Update() {
        // For when State gets updated through the inspector or by direct attribute access.
        // TODO: Add an editor widget to call ChangeState in the inspector, and remove
        // this logic and public access to State.
        if (State != _previousState) {
            ChangeState(State);
        }
    }

    public void ChangeState(CellState newState) {
        _previousState = newState;
        State = newState;
        gameObject.layer =
            State == CellState.Friendly || State == CellState.Absorbing || State == CellState.Melded ? Layers.PlayerCell
            : State == CellState.Enemy ? Layers.EnemyCell
            : State == CellState.Neutral || State == CellState.Attaching ? Layers.NeutralCell
            : State == CellState.Absorbing || State == CellState.BeingAbsorbed ? Layers.NoCollision
            : Layers.NoCollision;
        bool cellInCollidableState = State == CellState.Neutral || State == CellState.Enemy || State == CellState.Attaching;
        if (rb != null && !cellInCollidableState) {
            DisableRigidbody();
        }
        else if (rb == null && cellInCollidableState) {
            EnableRigidbody();
        }
        if (State == CellState.Neutral) {
            Knockout();
        }
        else if (_knockoutStars != null) {
            _knockoutStars.SetActive(false);
        }
        ChangeStateEvent?.Invoke(newState);
    }

    void Knockout() {
        if (_knockoutStars != null) {
            _knockoutStars.SetActive(true);
        }
        StartCoroutine(BlinkOutAfterDuration());
    }

    IEnumerator<WaitForSeconds> BlinkOutAfterDuration() {
        float updateInterval = .1f;
        int iterations = 0;
        int totalIterations = (int)(_knockoutTimeout / updateInterval);
        while (iterations < totalIterations) {
            if (State != CellState.Neutral) {
                SetEnabledOfRenderers(true);
                _knockoutStars.SetActive(false);
                yield break;
            }
            // Blink the cell in intervals of increasing frequency.
            if (iterations >= 300) {
                if (iterations < 360) {
                    SetEnabledOfRenderers(iterations % 10 <= 4);
                }
                else {
                    SetEnabledOfRenderers(iterations % 2 == 0);
                }
            }
            iterations += 1;
            yield return new WaitForSeconds(updateInterval);
        }
        if (State != CellState.Neutral) {
            SetEnabledOfRenderers(true);
            _knockoutStars.SetActive(false);
            yield break;
        }
        Destroy(gameObject);
        yield break;
    }

    void SetEnabledOfRenderers(bool enable) {
        foreach (var renderedObject in _renderedObjects) {
            if (renderedObject != null) {
                renderedObject.SetActive(enable);
            }
        }
    }

    void DisableRigidbody() {
        Destroy(GetComponent<Rigidbody>());
    }

    void EnableRigidbody() {
        if (rb != null) return;
        Rigidbody newRigidbody = gameObject.AddComponent<Rigidbody>();
        newRigidbody.mass = 15;
        newRigidbody.linearDamping = 2;
        newRigidbody.angularDamping = 5;
        newRigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        newRigidbody.useGravity = false;
        rb = newRigidbody;
    }
}
