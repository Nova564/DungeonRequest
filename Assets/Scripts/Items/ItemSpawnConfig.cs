using UnityEngine;

[System.Serializable]
public class ItemSpawnConfig
{
    public GameObject prefab;
    [Min(1)]
    public int maxTotal = 1; 
    [Range(0f, 1f)]
    public float dropRate = 1f; 
    [Min(1)]
    public int maxPerRoom = 1; 
}