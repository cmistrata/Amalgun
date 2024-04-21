using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

[CreateAssetMenu(menuName = "Level")]
public class Level : ScriptableObject
{
    [SerializeField] private List<Wave> _waves;
    public ReadOnlyCollection<Wave> Waves { get => _waves.AsReadOnly(); }
}
