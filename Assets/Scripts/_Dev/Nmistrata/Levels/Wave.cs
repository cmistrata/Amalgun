using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

[CreateAssetMenu(menuName = "Wave")]
public class Wave : ScriptableObject
{
    public float EnemySpawnDelaySeconds { get; private set; }
    public bool RandomizeOrder { get; private set; }
    [SerializeField] private List<CellType> _enemies;
    public ReadOnlyCollection<CellType> Enemies { get => _enemies.AsReadOnly(); }
}
