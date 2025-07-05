using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StationPhaseController : MonoBehaviour
{
    [SerializeField] private Button departButton; // ������ "����������� ������"
    [SerializeField] private TextMeshProUGUI stationTitle;

    private bool departureUnlocked = false;

    void Start()
    {
        ExperienceManager.Instance.OnPhaseUnlocked += OnPhaseUnlocked;
        departButton.onClick.AddListener(DepartToNextTrain);

        // ����������� ��� ��� �����
        int currentLevel = ExperienceManager.Instance.CurrentLevel;
        stationTitle.text = $"������� {currentLevel}";

        // ���������, ����� �� ������ ��� ������ ����
        if (ExperienceManager.Instance.XpForNextPhase == 0)
        {
            OnPhaseUnlocked(currentLevel, GamePhase.Station);
        }
        else
        {
            departButton.gameObject.SetActive(false);
        }
    }

    void OnDisable()
    {
        if (ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.OnPhaseUnlocked -= OnPhaseUnlocked;
        }
    }

    private void OnPhaseUnlocked(int level, GamePhase phase)
    {
        // ��������� ������ �� ������� �� ���� �������
        if (phase == GamePhase.Station)
        {
            departureUnlocked = true;
            departButton.gameObject.SetActive(true);
            // ��� ����� �������� �������� ��������� ������
        }
    }

    private void DepartToNextTrain()
    {
        if (!departureUnlocked) return;

        // ��������� ����� ������. ��������� ��� ���������� � ��������� ��������.
        // �� �������� ���������� ���.
        StartCoroutine(LoadTrainAndDepart());
    }

    private System.Collections.IEnumerator LoadTrainAndDepart()
    {
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("TrainScene");

        // ����, ���� ����� ����������
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // ����� �������� �����, ������� LocomotiveController � ��������� ��� ��������.
        // ��� ����� �������, ��� ����������� ������.
        LocomotiveController loco = FindObjectOfType<LocomotiveController>();
        if (loco != null)
        {
            yield return loco.StartCoroutine(loco.DepartToNextTrainLevel());
        }
    }
}