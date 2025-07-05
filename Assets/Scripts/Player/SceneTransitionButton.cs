using UnityEngine;

public class SceneTransitionButton : MonoBehaviour
{
    // ���� ��������� ����� ����� ���������� �� ������� OnClick() �� ������.
    // �� �� ������� ������� ���������� � ������ � ����������.
    public void GoToTrainScene()
    {
        // ���������, ���������� �� ��� �������� � ����
        if (TransitionManager.Instance != null)
        {
            // ������� �������� ����� ��� � �������� ��� �����
            TransitionManager.Instance.GoToTrainScene();
        }
        else
        {
            // ��� ��������� �������, ���� �� �������� �������� TransitionManager �� ����� Initializer
            Debug.LogError("TransitionManager �� ������ � ����! ���������� ������� �� ����� ������.");
        }
    }
}