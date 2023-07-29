using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class Player : MonoBehaviour {
    public int MaxHealth = 4;
    public int CurrentHealth = 4;
    private Rigidbody2D _rb;
    private MovingBody _movingBody;
    private Cannon _cannon;

    [SerializeField]
    private Dictionary<GameObject, List<GameObject>> partGraph = new();
    public static Player Instance;

    public float RotationSpeed = 100f;
    public float NewPartMassIncrease = .1f;

    private void Awake() {
        _rb = GetComponent<Rigidbody2D>();
        _movingBody = GetComponent<MovingBody>();
        _cannon = GetComponent<Cannon>();
        partGraph.Add(gameObject, new List<GameObject>());
        Instance = this;
    }

    // Update is called once per frame
    void Update() {
        // Movement
        Vector2 newTargetDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        _movingBody.TargetDirection = newTargetDirection.normalized;
        float clockwiseRotation = Input.GetAxis("Rotate Clockwise");
        transform.Rotate(new Vector3(0, 0, -clockwiseRotation * RotationSpeed * Time.deltaTime));
    }

    public void TakeDamage(int damage) {
        CurrentHealth -= damage;
        if (CurrentHealth <= 0) {
            Die();
            return;
        }

        AudioManager.Instance.PlayPlayerDamagedSound(1 + (4f - CurrentHealth) / 8f);
        CameraEffectsManager.Instance.FlashDamageFilter();
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
        _rb.mass += NewPartMassIncrease;
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
        neutralPart.GetComponent<MovingBody>().enabled = false;
        Destroy(neutralPart.GetComponent<Rigidbody2D>());

        // Make the part's Team "Team.Player".
        neutralPart.GetComponent<TeamTracker>().ChangeTeam(Team.Player);
    }

    private List<GameObject> FindTouchingPlayerPartsAndDestroyNearbyBullets(GameObject neutralPart) {
        List<Collider2D> hits = new();
        ContactFilter2D filter = new();
        Physics2D.OverlapCircle(
            point: new Vector2(neutralPart.transform.position.x, neutralPart.transform.position.y),
            radius: neutralPart.GetComponent<CircleCollider2D>().radius * 1.1f,
            contactFilter: filter.NoFilter(),
            results: hits
        );
        List<GameObject> adjacentParts = new();

        foreach (Collider2D hit in hits) {
            if (hit.transform == neutralPart.transform) continue;

            var nearbyPart = hit.transform.gameObject;
            var nearbyPartIsPlayerPart = partGraph.ContainsKey(nearbyPart);
            if (nearbyPartIsPlayerPart) {
                adjacentParts.Add(nearbyPart);
            }

            // Destroy nearby bullets on attach to prevent instafrag
            if (hit.transform.gameObject.TryGetComponent<Bullet>(out var nearbyBullet)) {
                Destroy(nearbyBullet.gameObject);
            }
        }

        return adjacentParts;
    }

    /// <summary>
    /// Removes a part from the player and updates the part graph
    /// Then checks the connection and removes other detached parts
    /// </summary>
    /// <param name="part">the part to detatch</param>
    public void DisconnectAndRemovePart(GameObject part) {
        // Decrease mass by the part's mass
        if (_rb.mass > 1) {
            _rb.mass -= NewPartMassIncrease;
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
        var newlyDisconnectedParts = partGraph.Keys.Where(part => !partsConnectedToPlayer.Contains(part));

        foreach (GameObject newlyDisconnectedPart in newlyDisconnectedParts) {
            partGraph.Remove(newlyDisconnectedPart);
            newlyDisconnectedPart.GetComponent<Part>().PlayDeathEffect();
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
            part.GetComponent<Part>().PlayDeathEffect();
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
        print("I died :(");
    }

    public void HandleBulletCollision(Collision2D other) {
        print("A bullet hit the player :(");
    }

    void OnCollisionEnter2D(Collision2D other) {
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
}
