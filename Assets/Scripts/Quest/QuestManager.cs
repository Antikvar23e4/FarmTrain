using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [SerializeField] private List<Quest> allQuests; // ���� � ���������� ���������� ��� ���� ������

    public List<Quest> ActiveQuests { get; private set; } = new List<Quest>();
    public List<Quest> CompletedQuests { get; private set; } = new List<Quest>();

    public event Action OnQuestLogUpdated; // ������ ��� UI, ��� ������ ������� ����������
    public event Action<Quest> OnQuestCompleted; // ������ ��� UI, ����� �������� ���� ����������
    public event Action<Quest> OnNewQuestAvailable; // ������ ��� ������-�����������

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else { Instance = this; DontDestroyOnLoad(gameObject); }
    }

    private void Start()
    {
        // ���������� ������� ���� ������� ��� ������ (��� �����)
        foreach (var quest in allQuests)
        {
            quest.status = QuestStatus.NotAccepted;
            quest.isPinned = false;
            quest.hasBeenViewed = false;
            foreach (var goal in quest.goals)
            {
                goal.currentAmount = 0;
            }
        }

        SubscribeToEvents();

        // ��������� ������ ��� ������ �������
        ActivateQuestsForStation(1, true);
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void HandleStationChanged(int newStationID)
    {
        ActivateQuestsForStation(newStationID, false);
    }

    // ���������� ������ ��� �������
    private void ActivateQuestsForStation(int stationId, bool isFirstStation)
    {
        var questsForStation = allQuests.Where(q => q.stationId == stationId && q.status == QuestStatus.NotAccepted).ToList();

        if (isFirstStation)
        {
            // ��� ������ ������� �������� ������ ������ ����� (� �������� ��� "������")
            var firstQuest = questsForStation.FirstOrDefault(q => !allQuests.Any(pq => pq.nextQuest == q));
            if (firstQuest != null)
            {
                StartQuest(firstQuest);
            }
        }
        else
        {
            // ��� ������� 2+ ���������� ��� ������ �����
            foreach (var quest in questsForStation)
            {
                StartQuest(quest);
            }
        }
    }

    public void StartQuest(Quest quest)
    {
        if (quest.status != QuestStatus.NotAccepted) return;

        quest.status = QuestStatus.Accepted;
        ActiveQuests.Add(quest);
        OnNewQuestAvailable?.Invoke(quest);
        OnQuestLogUpdated?.Invoke();
        Debug.Log($"����� '{quest.title}' �����!");
    }

    public void CompleteQuest(Quest quest)
    {
        if (quest.status != QuestStatus.Accepted) return;

        quest.status = QuestStatus.Completed;
        ActiveQuests.Remove(quest);
        CompletedQuests.Add(quest);

        // ������ �������
        ExperienceManager.Instance.AddXP(quest.rewardXP);

        // �������� ��������� ��������� ����� � �������
        if (quest.nextQuest != null)
        {
            StartQuest(quest.nextQuest);
        }

        OnQuestCompleted?.Invoke(quest);
        OnQuestLogUpdated?.Invoke();
        Debug.Log($"����� '{quest.title}' ��������! �������: {quest.rewardXP} XP.");
    }

    // ���� ����� ����� �������� ������ �������
    public void AddQuestProgress(GoalType goalType, string targetID, int amount)
    {
        foreach (var quest in new List<Quest>(ActiveQuests)) // ������� ��������� ����� ������
        {
            bool questProgressed = false;
            foreach (var goal in quest.goals.Where(g => g.goalType == goalType && !g.IsReached()))
            {
                // ��� ����� � ������� ��������� ID, ��� ��������� - ���.
                if (goalType == GoalType.Gather || goalType == GoalType.Buy)
                {
                    if (goal.targetID == targetID)
                    {
                        goal.UpdateProgress(amount);
                        questProgressed = true;
                    }
                }
                else if (goalType == GoalType.Earn)
                {
                    // ��� ��������� ����� �� �� ���������, � ������������� ������� ��������
                    goal.currentAmount = PlayerWallet.Instance.GetCurrentMoney();
                    questProgressed = true;
                }
            }

            if (questProgressed)
            {
                Debug.Log($"�������� ��� ������ '{quest.title}' ��������.");
                OnQuestLogUpdated?.Invoke(); // ��������� UI
                CheckQuestCompletion(quest);
            }
        }
    }

    private void CheckQuestCompletion(Quest quest)
    {
        if (quest.goals.All(g => g.IsReached()))
        {
            CompleteQuest(quest);
        }
    }

    public void PinQuest(Quest questToPin)
    {
        // ������� ��� �� ���� ���������
        foreach (var q in allQuests)
        {
            if (q != questToPin) q.isPinned = false;
        }
        // ����������� ��� ��� ����������
        questToPin.isPinned = !questToPin.isPinned;
        OnQuestLogUpdated?.Invoke();
    }

    private void UnsubscribeFromEvents()
    {
        if (ExperienceManager.Instance != null)
            ExperienceManager.Instance.OnStationChanged -= HandleStationChanged;
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnItemAdded -= HandleItemAdded;
        //if (ShopUIManager.Instance != null)
          //  ShopUIManager.Instance.OnItemPurchased -= HandleItemPurchased;
        if (PlayerWallet.Instance != null)
            PlayerWallet.Instance.OnMoneyChanged -= HandleMoneyChanged;
    }
    private void SubscribeToEvents()
    {
        ExperienceManager.Instance.OnStationChanged += HandleStationChanged;
        InventoryManager.Instance.OnItemAdded += HandleItemAdded;
        //ShopUIManagerShopUIManager.Instance.OnItemPurchased += HandleItemPurchased;
        // PlayerWallet.OnMoneyChanged ��� ����������, ���������� ���
        PlayerWallet.Instance.OnMoneyChanged += HandleMoneyChanged;
    }
    private void HandleItemAdded(ItemData item, int quantity)
    {
        // ���������� �������� ��� ����� ���� Gather
        AddQuestProgress(GoalType.Gather, item.name, quantity);
    }

    private void HandleItemPurchased(ItemData item, int quantity)
    {
        // ���������� �������� ��� ����� ���� Buy
        AddQuestProgress(GoalType.Buy, item.name, quantity);
    }

    private void HandleMoneyChanged(int newTotalMoney)
    {
        // ���������� �������� ��� ����� ���� Earn
        // amount �� ������������, �.�. �� ������ ������������� ������� ��������
        AddQuestProgress(GoalType.Earn, "", newTotalMoney);
    }
    public void TriggerQuestLogUpdate()
    {
        OnQuestLogUpdated?.Invoke();
    }

}