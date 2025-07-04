using UnityEngine;

// 1. ���������������� ������ (�������� � AnimalPenManager)
// �� �������� ������ �� ������� �����.
[System.Serializable]
public class PenConfigData
{
    public AnimalData animalData;
    public int maxCapacity;

    // ����� ��������, ������� �� ����� ������ �� ����� ������.
    // ��� ��������, ��� ������ ������.
    public string placementAreaName;
    public string animalParentName;
}

// 2. "�����" ������ (������������ TrainPenController �� ����� ����)
// �������� ������ �� �������� ������� �� �����.
public class PenRuntimeInfo
{
    public PenConfigData config; // ������ �� �������� ������
    public Collider2D placementArea;
    public Transform animalParent;
}