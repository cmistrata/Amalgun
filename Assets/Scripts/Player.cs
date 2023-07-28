using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MovingBody))]
public class Player : Part {
    private Rigidbody2D _rb;
    private MovingBody _movingBody;
    public bool Shooting = false;
    public Part CenterPart;
    [SerializeField]
    private Dictionary<Part, List<Part>> partGraph = new Dictionary<Part, List<Part>>();
    public static Player Instance;
    
    public readonly float ATTACHED_PART_MASS = .5f;
    public float rotationSpeed = 100f;

    private void Awake()
    {
        Instance = this;
        _rb = GetComponent<Rigidbody2D>();
        _movingBody = GetComponent<MovingBody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Acceleration = 50f;
        CenterPart.gameObject.GetComponent<TeamTracker>().ChangeTeam(Team.Player);
        partGraph.Add(CenterPart, new List<Part>());
    }

    // Update is called once per frame
    void Update()
    {
        CheckInputs();   
    }

    void CheckInputs()
    {
        // Movement
        Vector2 newTargetDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        _movingBody.TargetDirection = newTargetDirection.normalized;
        float clockwiseRotation = Input.GetAxis("Rotate Clockwise");
        transform.Rotate(new Vector3(0, 0, -clockwiseRotation * rotationSpeed * Time.deltaTime));
    }

    public void TakeDamage(float damage) {
        currentHealth -= damage;
        if (currentHealth <= 0) {
            Die();
        } else if (Player.Instance.CenterPart == this) {
            AudioManager.Instance.PlayCenterPartHitSound(1 + (4f - currentHealth) / 8f);
            CameraEffectsManager.Instance.FlashDamageFilter();
        }
    }


    /// <summary>
    /// Adds a part to the player and updates the part graph
    /// </summary>
    /// <param name="part">the new part</param>
    /// <param name="adjacentParts">existing parts adjacent to new part</param>
    public void AddPart(Part part)
    {
        List<Collider2D> hits = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();
        Physics2D.OverlapCircle(
            point: new Vector2(part.transform.position.x, part.transform.position.y),
            radius: part.GetComponent<CircleCollider2D>().radius * 1.1f,
            contactFilter: filter.NoFilter(),
            results: hits
        );
        List<Part> adjacentParts = new List<Part>();

        foreach (Collider2D hit in hits)
        {

            if (hit.transform == part.transform) continue;

            var nearbyPart = hit.transform.gameObject.GetComponent<Part>();
            if (nearbyPart != null && nearbyPart.Team == Team.Player)
            {
                adjacentParts.Add(nearbyPart);
            }

            // Destroy nearby bullets on attach to prevent instafrag
            var nearbyBullet = hit.transform.gameObject.GetComponent<Bullet>();
            if (nearbyBullet != null)
            {
                Destroy(nearbyBullet.gameObject);
            }
        }

        partGraph.Add(part, adjacentParts);
        _rb.mass += ATTACHED_PART_MASS;
        part.transform.parent = transform;
        // Remove the part's RigidBody, but not its collider.
        // This will effectively combine the part's collider into the player's.
        part.GetComponent<MovingBody>().enabled = false;
        Destroy(part.GetComponent<Rigidbody2D>());

        part.GetComponent<TeamTracker>().ChangeTeam(Team.Player);

        foreach (Part p in adjacentParts)
        {
            try
            {
                partGraph[p].Add(part);
            }
            catch (KeyNotFoundException)
            {
                Debug.LogError($"Tried to update adjacencies for part ${p.gameObject.name} not in the Player's partGraph.");
            }
        }

    }

    /// <summary>
    /// Removes a part to the player and updates the part graph
    /// Then checks the connection and removes other detached parts
    /// </summary>
    /// <param name="part">the part to detatch</param>
    public void DetatchPart(Part part)
    {
        // The only time Detatch should be called is when a Part loses all its health.
        // If this happens on the CenterPart, we should destroy the player's parts and
        // trigger a GameOver();
        if (part == CenterPart)
        {
            DestroyAllParts();
            GameManager.Instance.GameOver();
            return;
        }
        if (_rb.mass > 1)
        {
            _rb.mass -= ATTACHED_PART_MASS;
        }

        foreach (Part p in partGraph.Keys)
        {
            partGraph[p].Remove(part);
        }
        partGraph.Remove(part);
        part.transform.parent = null;
        DestroyUnconnectedParts();
    }

    private void DestroyUnconnectedParts()
    {
        List<Part> connectedParts = new List<Part>();
        connectedParts.Add(CenterPart);
        Stack<Part> partsToSearch = new Stack<Part>();
        partsToSearch.Push(CenterPart);

        while (partsToSearch.Count > 0)
        {
            Part currentPart = partsToSearch.Pop();
            if (currentPart == null || !partGraph.ContainsKey(currentPart))
            {
                Debug.LogError($"Couldn't find {currentPart} in part graph.");
                continue;
            }
            List<Part> neighbors = partGraph[currentPart];
            foreach (Part p in neighbors)
            {
                if (!connectedParts.Contains(p))
                {
                    partsToSearch.Push(p);
                    connectedParts.Add(p);
                }
            }
        }

        List<Part> unconnectedParts = new List<Part>();
        // Remove the disconnected parts
        foreach (Part p in partGraph.Keys)
        {
            if (!connectedParts.Contains(p))
            {
                unconnectedParts.Add(p);
            }
        }

        foreach (Part unconnectedPart in unconnectedParts)
        {
            foreach (Part p in partGraph.Keys)
            {
                partGraph[p].Remove(unconnectedPart);
            }
            partGraph.Remove(unconnectedPart);
            unconnectedPart.PlayDeathEffect();
        }
    }

    public void DestroyAllParts()
    {
        foreach (Part part in partGraph.Keys)
        {
            part.PlayDeathEffect();
        }
        partGraph = new Dictionary<Part, List<Part>>();
        CenterPart = null;
    }

    private string GetPartGraphAsString()
    {
        var printString = "";
        foreach (Part part in partGraph.Keys)
        {
            printString += $"{part.name}: {String.Join(", ", partGraph[part])}\n";
        }
        return printString;
    }
}
