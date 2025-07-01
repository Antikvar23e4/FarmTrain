using UnityEngine;
using System;

public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance { get; private set; }

    [SerializeField] private int[] xpPerStation = { 100, 200, 300 }; // XP ��� �������� �� 2, 3, 4 �������

    public int CurrentXP { get; private set; }
    public int CurrentStation { get; private set; }
    public int XpForNextStation { get; private set; }

    public event Action<int, int> OnXPChanged; // currentXP, xpForNext
    public event Action<int> OnStationChanged; // newStationID

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
    }

    private void Initialize()
    {
        // ����� ����� ������ �������� ����������, � ���� ������ � ����
        CurrentXP = 0;
        CurrentStation = 1;
        XpForNextStation = GetXPForStation(CurrentStation);
    }

    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        CurrentXP += amount;
        Debug.Log($"�������� {amount} XP. �����: {CurrentXP}/{XpForNextStation}");

        OnXPChanged?.Invoke(CurrentXP, XpForNextStation);

        if (CurrentXP >= XpForNextStation)
        {
            LevelUpStation();
        }
    }

    private void LevelUpStation()
    {
        if (CurrentStation >= xpPerStation.Length + 1)
        {
            Debug.Log("���������� ��������� �������!");
            return;
        }

        CurrentXP -= XpForNextStation;
        CurrentStation++;
        XpForNextStation = GetXPForStation(CurrentStation);

        Debug.Log($"<color=cyan>������� �� ����� �������: {CurrentStation}!</color>");

        OnStationChanged?.Invoke(CurrentStation);
        OnXPChanged?.Invoke(CurrentXP, XpForNextStation);
    }

    private int GetXPForStation(int station)
    {
        int index = station - 1;
        if (index >= 0 && index < xpPerStation.Length)
        {
            return xpPerStation[index];
        }
        return int.MaxValue; // ���� ������� ������, ��� ������
    }
}