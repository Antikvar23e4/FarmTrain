using UnityEngine;
using System.Collections;

public class ScreenFaderManager : MonoBehaviour
{
    public static ScreenFaderManager Instance { get; private set; }

    [SerializeField] private GameObject faderObject;
    [SerializeField] private float fadeDuration = 1.0f;
    // ���� � ��� ���� ��������, ����� ������������ ���
    // [SerializeField] private Animator faderAnimator; 

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (faderObject != null)
        {
            faderObject.SetActive(false);
        }
    }

    public void FadeOutAndIn(System.Action onMiddleOfFade = null)
    {
        StartCoroutine(FadeCoroutine(onMiddleOfFade));
    }

    private IEnumerator FadeCoroutine(System.Action onMiddleOfFade)
    {
        // Fade Out (����������)
        faderObject.SetActive(true);
        // ����� ����� �������� ������� �������� ����� CanvasGroup.alpha
        yield return new WaitForSeconds(fadeDuration);

        // �������� � �������� (��������, ����� ����)
        onMiddleOfFade?.Invoke();

        // Fade In (����������)
        // ����� ����� �������� ������� ��������
        yield return new WaitForSeconds(fadeDuration);
        faderObject.SetActive(false);
    }
}