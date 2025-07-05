using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExperienceBarUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI xpText;

    private void Start()
    {
        // ���������, ��� ExperienceManager ����������
        if (ExperienceManager.Instance == null)
        {
            Debug.LogError("ExperienceManager �� ������! UI ����� �� ����� ��������.");
            gameObject.SetActive(false);
            return;
        }

        // ������������� �� ������� ��������� �����
        ExperienceManager.Instance.OnXPChanged += UpdateXPBar;

        // ��������� ������ ��� ������, ����� �������� ��������� ��������
        UpdateXPBar(ExperienceManager.Instance.CurrentXP, ExperienceManager.Instance.XpForNextPhase);
    }

    private void OnDestroy()
    {
        // ����������� ������������, ����� �������� ������
        if (ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.OnXPChanged -= UpdateXPBar;
        }
    }

    private void UpdateXPBar(int currentXP, int xpForNextPhase)
    {
        if (xpForNextPhase > 0)
            xpText.text = $"{currentXP} / {xpForNextPhase}";
        else // ���� ����� ��� �������� �� ����� (��������, �� ������� 2)
        {
            xpText.text = "���������!";

        }

    }
}