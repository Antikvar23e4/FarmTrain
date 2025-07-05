using UnityEngine;
using System;

public enum GamePhase
{
    Train,
    Station
}

public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance { get; private set; }

    [System.Serializable]
    public struct XpThreshold
    {
        public int trainPhaseXP; // XP ��� �������� � ������ �� �������
        public int stationPhaseXP; // XP ��� �������� �� ������� �� ��������� �����
    }

    [SerializeField] private XpThreshold[] xpLevels; // ������ ������� XP ��� ������� "������"

    public int CurrentLevel { get; private set; } // ������� ���� (1, 2, 3...)
    public GamePhase CurrentPhase { get; private set; } // ������� ���� (����� ��� �������)

    public int CurrentXP { get; private set; }
    public int XpForNextPhase { get; private set; }

    public event Action<int, int> OnXPChanged; // currentXP, xpForNext
    public event Action<int, GamePhase> OnPhaseUnlocked; // ������, ��� ����� ���������� �� ����. ����

    private void Awake()
    {
        // ... (��������)
        if (Instance != null && Instance != this) Destroy(gameObject);
        else { Instance = this; DontDestroyOnLoad(gameObject); Initialize(); }
    }

    private void Initialize()
    {
        // � ������� ����� ����� �������� ����������
        CurrentLevel = 1;
        CurrentPhase = GamePhase.Train;
        CurrentXP = 0;
        UpdateXpThreshold();
    }

    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        CurrentXP += amount;
        Debug.Log($"�������� {amount} XP. �����: {CurrentXP}/{XpForNextPhase}");

        OnXPChanged?.Invoke(CurrentXP, XpForNextPhase);

        if (CurrentXP >= XpForNextPhase)
        {
            // �� �� ��������� �� ����� ������� �������������.
            // �� ������ ��������, ��� ������� �������������.
            OnPhaseUnlocked?.Invoke(CurrentLevel, CurrentPhase);
            Debug.Log($"<color=cyan>������� �� ��������� ���� �������������!</color>");
        }
    }

    // ���� ����� ����� ���������� �� LocomotiveController ��� StationController
    public void AdvanceToNextPhase()
    {
        CurrentXP = 0; // ���������� XP ��� ������ �����

        if (CurrentPhase == GamePhase.Train)
        {
            // ��������� �� ������ �� �������
            CurrentPhase = GamePhase.Station;
        }
        else // CurrentPhase == GamePhase.Station
        {
            // ��������� �� ������� �� ��������� �����
            CurrentPhase = GamePhase.Train;
            CurrentLevel++;
        }

        UpdateXpThreshold();
        OnXPChanged?.Invoke(CurrentXP, XpForNextPhase); // ��������� UI XP ����
        QuestManager.Instance.ActivateQuestsForCurrentPhase(); // <<< �������� ������
    }

    private void UpdateXpThreshold()
    {
        int levelIndex = CurrentLevel - 1;
        if (levelIndex < 0 || levelIndex >= xpLevels.Length)
        {
            XpForNextPhase = int.MaxValue; // ����� ����
            return;
        }

        if (CurrentPhase == GamePhase.Train)
        {
            XpForNextPhase = xpLevels[levelIndex].trainPhaseXP;
        }
        else
        {
            XpForNextPhase = xpLevels[levelIndex].stationPhaseXP;
        }
    }
}
