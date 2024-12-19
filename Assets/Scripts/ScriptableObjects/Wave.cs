using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

[CreateAssetMenu(menuName = "Wave")]
public class Wave : ScriptableObject {
    [SerializeField] private List<CellType> _enemies;
    public ReadOnlyCollection<CellType> Enemies { get => _enemies.AsReadOnly(); }
}
