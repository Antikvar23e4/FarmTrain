using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class QuestLogEntryUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image checkmarkImage;
    [SerializeField] private Button pinButton; // ���� ������-�������
    [SerializeField] private Image pinIcon; // ������ �� ���� ������
    [SerializeField] private Button mainButton; // �������� ������, �� ������� �������

    [Header("Pin Sprites")]
    [SerializeField] private Sprite pinnedSprite; // ������ ������������ �������
    [SerializeField] private Sprite unpinnedSprite; // ������ ������� �������

    private Quest assignedQuest;
    private Action<Quest> onSelectCallback;

    public void Setup(Quest quest, Action<Quest> selectCallback)
    {
        assignedQuest = quest;
        onSelectCallback = selectCallback;

        titleText.text = quest.title;

        // ����������� ��������� ��������� � ����������� �� ������� ������
        bool isCompleted = quest.status == QuestStatus.Completed;
        checkmarkImage.gameObject.SetActive(isCompleted);
        pinButton.gameObject.SetActive(!isCompleted); // ������� ���������� ������ ��� ��������

        if (!isCompleted)
        {
            pinIcon.sprite = quest.isPinned ? pinnedSprite : unpinnedSprite;
        }

        // ����������� ���������� �������
        mainButton.onClick.RemoveAllListeners();
        mainButton.onClick.AddListener(HandleSelection);

        pinButton.onClick.RemoveAllListeners();
        pinButton.onClick.AddListener(HandlePinning);
    }

    // ���� �� �������� ����� ������ - ������� ����� ��� ������ �������
    private void HandleSelection()
    {
        onSelectCallback?.Invoke(assignedQuest);
    }

    // ���� �� ������� - ���������/���������
    private void HandlePinning()
    {
        QuestManager.Instance.PinQuest(assignedQuest);
    }
}