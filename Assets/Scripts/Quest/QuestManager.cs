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
        ActivateQuestsForCurrentPhase();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    public void ActivateQuestsForCurrentPhase()
    {
        int level = ExperienceManager.Instance.CurrentLevel;
        GamePhase phase = ExperienceManager.Instance.CurrentPhase;

        Debug.Log($"<color=yellow>[QuestManager]</color> ��������� ������� ��� ������ {level}, ����: {phase}");

        var questsForPhase = allQuests.Where(q => q.gameLevel == level && q.phase == phase && q.status == QuestStatus.NotAccepted).ToList();

        // ����������� ������ ��� ����� ������ ���� (����� 1)
        if (level == 1 && phase == GamePhase.Train)
        {
            Debug.Log("��������� ������� � ������ '�� �������'.");
            var firstQuestInChain = questsForPhase.FirstOrDefault(q => !allQuests.Any(pq => pq.nextQuest == q));
            if (firstQuestInChain != null)
            {
                StartQuest(firstQuestInChain);
            }
        }
        else
        {
            Debug.Log("��������� ������� � ������ '��� �����'.");
            foreach (var quest in questsForPhase)
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


    public void AddQuestProgress(GoalType goalType, string targetID, int amount)
    {
        foreach (var quest in new List<Quest>(ActiveQuests))
        {
            bool questProgressed = false;
            foreach (var goal in quest.goals.Where(g => g.goalType == goalType && !g.IsReached()))
            {
                // <<< ���������� ������ ��� ���� ������������� �����
                if (goalType == GoalType.Gather ||
                    goalType == GoalType.Buy ||
                    goalType == GoalType.FeedAnimal ||
                    goalType == GoalType.Earn) // <<< ��������� EARN � ���� ������
                {
                    // ��� Earn ��� �� ����� ��������� ID, �������� ���� ������.
                    // ��� ��������� ����� ID ������ ���������.
                    if (goalType == GoalType.Earn || goal.targetID == targetID)
                    {
                        goal.UpdateProgress(amount); // ���� ����� ���������: currentAmount += amount
                        questProgressed = true;
                    }
                }
                // <<< ������ ������ ��� else if (goalType == GoalType.Earn) ������ �� ����� � �������
            }

            if (questProgressed)
            {
                Debug.Log($"<color=lightblue>[QuestManager]</color> �������� ��� ������ '{quest.title}' �������� �� ���� '{goalType}'.");
                OnQuestLogUpdated?.Invoke();
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
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnItemAdded -= HandleItemAdded;
        //if (ShopUIManager.Instance != null)
          //  ShopUIManager.Instance.OnItemPurchased -= HandleItemPurchased;
        if (PlayerWallet.Instance != null)
            PlayerWallet.Instance.OnMoneyAdded -= HandleMoneyAdded;
    }
    private void SubscribeToEvents()
    {
        InventoryManager.Instance.OnItemAdded += HandleItemAdded;
        //ShopUIManagerShopUIManager.Instance.OnItemPurchased += HandleItemPurchased;
        PlayerWallet.Instance.OnMoneyAdded += HandleMoneyAdded;

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

    private void HandleMoneyAdded(int amountAdded)
    {
        // ������ �� ���������� �������� ��� ���� Earn, �� �������� ������ ����� ��������
        AddQuestProgress(GoalType.Earn, "", amountAdded);
    }
    public void TriggerQuestLogUpdate()
    {
        OnQuestLogUpdated?.Invoke();
    }

}