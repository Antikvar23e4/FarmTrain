// LocomotiveController.cs

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
// using UnityEngine.EventSystems; // <<< ����� �������

public class LocomotiveController : MonoBehaviour
{
    // ... (���� ���������, ���� ����)

    [Header("������ �� �������")]
    // <<< �������� ��� ���� ���������, ����� ������ ������� ����� ��� ������
    public GameObject hornObject;
    //[SerializeField] private Animator hornAnimator;
    [SerializeField] private Button goToStationButton;
    [SerializeField] private GameObject screenFader;
    [SerializeField] private AutoScrollParallax[] parallaxLayers;

    private bool travelUnlocked = false;

    // <<< ������� ���� ����� Update() � Awake() �� ����� �������. ��� ������ �� �����.
    // void Awake() { ... }
    // void Update() { ... }

    void Start()
    {
        // �������� �� ������� ��������
        ExperienceManager.Instance.OnPhaseUnlocked += OnPhaseUnlocked;
        goToStationButton.onClick.AddListener(GoToStation);

        screenFader.SetActive(false);
        goToStationButton.gameObject.SetActive(false);

        // �������� ��� ������ ���� ��������
        if (ExperienceManager.Instance.CurrentPhase == GamePhase.Train &&
            ExperienceManager.Instance.CurrentXP >= ExperienceManager.Instance.XpForNextPhase)
        {
            OnPhaseUnlocked(ExperienceManager.Instance.CurrentLevel, GamePhase.Train);
        }
    }

    void OnDisable()
    {
        if (ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.OnPhaseUnlocked -= OnPhaseUnlocked;
        }
    }

    // <<< �������� ���� ����� ���������
    public void OnHornClicked()
    {
        Debug.Log("<color=orange>���� �� ����� ��������������� ����� LocomotiveController!</color>");

        if (travelUnlocked)
        {
            Debug.Log("������� travelUnlocked ���������. ������������� �����.");
            StopTrain();
        }
        else
        {
            Debug.Log("���� �� ����� ���, �� travelUnlocked == false. ������ �� ������.");
        }
    }

    private void OnPhaseUnlocked(int level, GamePhase phase)
    {
        if (phase == GamePhase.Train)
        {
            Debug.Log("<color=cyan>LocomotiveController: ������� ������ OnPhaseUnlocked. travelUnlocked = true</color>");
            travelUnlocked = true;
            //hornAnimator.SetBool("IsHighlighted", true);
        }
    }

    // ��������� ������ (StopTrain, GoToStation, DepartToNextTrainLevel) �������� ��� ���������.
    private void StopTrain()
    {
        travelUnlocked = false;
        //hornAnimator.SetBool("IsHighlighted", false);
        foreach (var layer in parallaxLayers) layer.enabled = false;
        goToStationButton.gameObject.SetActive(true);
    }

    private void GoToStation()
    {
        ExperienceManager.Instance.AdvanceToNextPhase();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Station_1");
    }

    public IEnumerator DepartToNextTrainLevel()
    {
        screenFader.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        ExperienceManager.Instance.AdvanceToNextPhase();
        foreach (var layer in parallaxLayers) layer.enabled = true;
        yield return new WaitForSeconds(1.0f);
        screenFader.SetActive(false);
    }
}