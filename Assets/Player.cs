using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UI;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Mover))]
public class Player : MonoBehaviour {
    public int MaxHealth = 4;
    public int CurrentHealth = 4;
    private Rigidbody _rb;
    private Mover _mover;

    [SerializeField]
    private Dictionary<GameObject, List<GameObject>> partGraph = new();
    public static Player Instance;

    private float _rotationSpeed = 100f;
    private float _newPartMassIncrease = .1f;

    public static event Action SignalPlayerDeath;

    private void Awake() {
        _rb = GetComponent<Rigidbody>();
        _mover = GetComponent<Mover>();
        partGraph.Add(gameObject, new List<GameObject>());
        Instance = this;
    }

    // Update is called once per frame
    void Update() {
        Vector3 newTargetDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        _mover.TargetDirection = newTargetDirection.normalized;

        float clockwiseRotation = Input.GetAxis("Rotate Clockwise");
        transform.Rotate(new Vector3(0, clockwiseRotation * _rotationSpeed * Time.deltaTime, 0));

        DetectAndHandleClick();
    }

    void DetectAndHandleClick() {
        if (Input.GetMouseButtonDown(0) && Utils.MouseRaycast(out var hit)) {
            if (GameManager.Instance.State == GameState.Shop) {
                if (GameManager.Instance.Money < 0) return;
                if (hit.collider.gameObject == this.gameObject) {
                    GameManager.Instance.SpendMoney();
                    Heal(1);
                } else {
                    var clickedPart = hit.collider.gameObject.GetComponent<Part>();
                    GameManager.Instance.SpendMoney();
                    clickedPart.Meld();
                }
            }
        }
    }

    public void Heal(int amount) {
        CurrentHealth = Math.Min(CurrentHealth + amount, MaxHealth);
    }

    public void TakeDamage(int damage) {
        CurrentHealth = Math.Max(CurrentHealth - damage, 0);
        if (CurrentHealth <= 0) {
            Die();
            return;
        }

        AudioManager.Instance.PlayPlayerDamagedSound(1 + (4f - CurrentHealth) / 8f);
        CameraManager.Instance.FlashDamageFilter();
    }


    /// <summary>
    /// Adds a part to the player and updates the part graph
    /// </summary>
    /// <param name="neutralPart">the new part</param>
    /// <param name="adjacentParts">existing parts adjacent to new part</param>
    public void ConnectPart(GameObject neutralPart) {
        List<GameObject> touchingPlayerParts = FindTouchingPlayerPartsAndDestroyNearbyBullets(neutralPart);

        // Add the new part to the graph, and simultaneously create edges
        // from it to all the parts near it during the collision.
        partGraph.Add(neutralPart, touchingPlayerParts);
        _rb.mass += _newPartMassIncrease;
        neutralPart.transform.parent = transform;
        // Also create edges from its neighbors to it.
        foreach (GameObject touchingPlayerPart in touchingPlayerParts) {
            try {
                partGraph[touchingPlayerPart].Add(neutralPart);
            } catch (KeyNotFoundException) {
                Debug.LogError($"Tried to update adjacencies for part ${touchingPlayerPart} not in the Player's partGraph.");
            }
        }

        // Remove the part's RigidBody, but not its collider.
        // This will effectively combine the part's collider into the player's.
        neutralPart.GetComponent<Mover>().enabled = false;
        Destroy(neutralPart.GetComponent<Rigidbody>());

        // Make the part's Team "Team.Player".
        neutralPart.GetComponent<TeamTracker>().ChangeTeam(Team.Player);
    }

    private List<GameObject> FindTouchingPlayerPartsAndDestroyNearbyBullets(GameObject neutralPart) {
        Collider[] nearbyColliders = Physics.OverlapSphere(neutralPart.transform.position, 1.05f);
        List<GameObject> adjacentParts = new();

        foreach (Collider nearbyCollider in nearbyColliders) {
            if (nearbyCollider.transform == neutralPart.transform) continue;

            var nearbyObject = nearbyCollider.gameObject;
            var nearbyObjectIsPlayerPart = partGraph.ContainsKey(nearbyObject);
            if (nearbyObjectIsPlayerPart) {
                adjacentParts.Add(nearbyObject);
            }

            // Destroy nearby bullets on attach to prevent instafrag
            if (nearbyObject.TryGetComponent<Bullet>(out var _nearbyBullet)) {
                Destroy(nearbyObject);
            }
        }

        return adjacentParts;
    }

    /// <summary>
    /// Removes a part from the player and updates the part graph
    /// Then checks the connection and removes other detached parts
    /// </summary>
    /// <param name="part">the part to detatch</param>
    public void DamagePart(GameObject part) {
        var partComponent = part.GetComponent<Part>();
        partComponent.TakeDamage(1);
        if (partComponent.beingDestroyed) {
            DisconnectPart(part);
        }
    }

    public void DisconnectPart(GameObject part) {
        // Decrease mass by the part's mass
        if (_rb.mass > 1) {
            _rb.mass -= _newPartMassIncrease;
        }

        // Remove any edges containing the part, and then remove the node representing the part
        foreach (GameObject p in partGraph.Keys) {
            partGraph[p].Remove(part);
        }
        partGraph.Remove(part);
        part.transform.parent = null;
        DestroyUnconnectedParts();
    }

    private void DestroyUnconnectedParts() {
        var partsConnectedToPlayer = GetPartsConnectedToPlayer();
        var newlyDisconnectedParts = partGraph.Keys.Where(part => !partsConnectedToPlayer.Contains(part)).ToList();

        foreach (GameObject newlyDisconnectedPart in newlyDisconnectedParts) {
            partGraph.Remove(newlyDisconnectedPart);
            newlyDisconnectedPart.GetComponent<Part>().Die();
        }
    }

    private List<GameObject> GetPartsConnectedToPlayer() {
        List<GameObject> connectedParts = new() { gameObject };
        Stack<GameObject> partsToSearch = new();
        partsToSearch.Push(gameObject);

        while (partsToSearch.Count > 0) {
            var currentPart = partsToSearch.Pop();
            if (currentPart == null || !partGraph.ContainsKey(currentPart)) {
                Debug.LogError($"Couldn't find {currentPart} in part graph.");
                continue;
            }
            var neighbors = partGraph[currentPart];
            foreach (GameObject p in neighbors) {
                if (!connectedParts.Contains(p)) {
                    partsToSearch.Push(p);
                    connectedParts.Add(p);
                }
            }
        }

        return connectedParts;
    }

    public void DestroyAllParts() {
        foreach (GameObject part in partGraph.Keys) {
            part.GetComponent<Part>().PlayDeathFX();
        }
        partGraph = new();
    }

    private string GetPartGraphAsString() {
        var printString = "";
        foreach (GameObject part in partGraph.Keys) {
            printString += $"{part.name}: {String.Join(", ", partGraph[part])}\n";
        }
        return printString;
    }

    private void Die() {
        PlayDeathFX();
        Destroy(gameObject);
        SignalPlayerDeath?.Invoke();
    }

    public void HandleBulletCollision(Collision collision) {
        var nonBulletObject = collision.GetContact(0).thisCollider.gameObject;
        var collisionIsWithMainPart = nonBulletObject == gameObject;
        if (collisionIsWithMainPart) {
            TakeDamage(1);
        } else {
            DamagePart(nonBulletObject);
        }
    }

    void OnCollisionEnter(Collision other) {
        Utils.LogOncePerSecond($"Collided with {other}");
        int collisionLayer = other.gameObject.layer;
        switch (collisionLayer) {
            case Layers.NeutralPart:
                ConnectPart(other.gameObject);
                break;
            case Layers.EnemyBullet:
                HandleBulletCollision(other);
                break;
        }
    }

    public void PlayDeathFX() {
        Instantiate(PrefabsManager.Instance.PlayerDeathEffect, transform.position, Quaternion.identity, transform.parent);
        AudioManager.Instance.PlayPartDestroy();
        CameraManager.Instance.ShakeCamera(.3f, .6f);
    }
}
