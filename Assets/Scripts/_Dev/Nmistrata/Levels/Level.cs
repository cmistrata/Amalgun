using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Level")]
public class Level : ScriptableObject
{
    public List<Wave> Waves;
}
