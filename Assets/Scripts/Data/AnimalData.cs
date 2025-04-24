using UnityEngine;

[CreateAssetMenu(fileName = "New Animal Type", menuName = "Farming/Animal Type")]
public class AnimalData : ScriptableObject
{
    public string speciesName = "New Animal";
    public GameObject animalPrefab; // ������ ��������� � ��� �������/�������� � ������� ��������

    [Header("Needs")]
    public ItemData requiredFood; // ����� ItemData ����� ��� ���������
    public float feedInterval = 60.0f; // ��� ����� ����� �������

    [Header("Production")]
    public ItemData productProduced; // ����� ItemData ����������
    public float productionInterval = 120.0f; // ��� ����� ���������� �������
    public int productAmount = 1; // ������� �������� �� ���

    [Header("Fertilizer")] // ���������
    public ItemData fertilizerProduced;
    public float fertilizerInterval = 180.0f;
    public int fertilizerAmount = 1; 

}