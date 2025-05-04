using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Bed", menuName = "Farming/Bed")]

public class BedData : ScriptableObject
{
    public string speciesName = "New Bed";
    public GameObject bedlPrefab; // ������ ������


    [Header("Growth")]
    public Sprite bedSprite; //  ������ ������ 
    public bool isPlanted; // �������� �� ������
    
 
}
