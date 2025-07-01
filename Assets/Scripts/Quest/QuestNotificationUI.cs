using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class QuestNotificationUI : MonoBehaviour
{
    [SerializeField] private GameObject notificationIcon; // ���� ��������� ���� ������ "!"

    private void Start()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogError("QuestManager �� ������! ����������� � ������� �� ����� ��������.");
            gameObject.SetActive(false);
            return;
        }

        // ������������� �� ����� ���������� ����
        QuestManager.Instance.OnQuestLogUpdated += CheckForNewQuests;

        // �������� ������ ��� ������
        notificationIcon.SetActive(false);

        // ������ �������� �� ������, ���� ���� ���������� � ��������������� �������
        CheckForNewQuests();
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestLogUpdated -= CheckForNewQuests;
        }
    }

    private void CheckForNewQuests()
    {
        // ���������, ���� �� ���� �� ���� �������� �����, ������� ��� �� ��� ����������
        bool hasUnreadQuests = QuestManager.Instance.ActiveQuests.Any(quest => !quest.hasBeenViewed);

        notificationIcon.SetActive(hasUnreadQuests);
    }
}