using UnityEngine;

[CreateAssetMenu(fileName = "New Animal Type", menuName = "Farming/Animal Type")]
public class AnimalData : ScriptableObject
{
    public string speciesName = "New Animal";
    public Sprite icon;
    // public GameObject prefab; // ������ ��������� � ��� �������/�������� � ������� ��������

    [Header("Needs")]
    public ItemData requiredFood; // ����� ItemSO ����� ��� ���������
    public float feedInterval = 60.0f; // ��� ����� ����� �������

    [Header("Production")]
    public ItemData productProduced; // ����� ItemSO ����������
    public float productionInterval = 120.0f; // ��� ����� ���������� �������
    public int productAmount = 1; // ������� �������� �� ���
}