using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Station Data", menuName = "Game/Station Data")]
public class StationData : ScriptableObject
{
    public int stationId;
    public string stationName;
    // ���� �� ����� ��������� ������ ��� ������� ������ �� ���� �������.
    // ���������� ������, ��� ��� ������� ����� ���� ���������.
    public List<ShopInventoryData> stallInventories;
}