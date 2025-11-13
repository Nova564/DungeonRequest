using UnityEngine;

[System.Serializable]
public class ItemSpawnConfig
{
    public GameObject prefab;
    [Min(1)]
    //max d'items dans la génération
    public int maxTotal = 1; 
    [Range(0f, 1f)]
    //a chaque tentative la probabilité qu'il spawn
    public float dropRate = 1f; 
    [Min(1)]
    //maximum possible par room
    public int maxPerRoom = 1; 
}