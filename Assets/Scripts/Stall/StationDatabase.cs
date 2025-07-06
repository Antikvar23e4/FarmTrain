using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class StationDatabase : MonoBehaviour
{
    public static StationDatabase Instance { get; private set; }

    [SerializeField] private List<StationData> allStations;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else { Instance = this; DontDestroyOnLoad(gameObject); }
    }

    public StationData GetStationDataById(int id)
    {
        // ���� �������, � ������� stationId ��������� � ������� ������� ����
        return allStations.FirstOrDefault(s => s.stationId == id);
    }
}