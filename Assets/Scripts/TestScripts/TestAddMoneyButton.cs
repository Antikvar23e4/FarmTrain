using UnityEngine;
using UnityEngine.UI;

// ���� ������ ����� �������� �� ����� GameObject � ����������� Button � UI.
[RequireComponent(typeof(Button))]
public class TestAddMoneyButton : MonoBehaviour
{
    [Tooltip("����� ����� ����� ��������� �� ���� �������.")]
    [SerializeField] private int amountToAdd = 50;

    private Button testButton;

    void Start()
    {
        // �������� ������ �� ��������� ������
        testButton = GetComponent<Button>();

        // ���������, ��� PlayerWallet ���������� �� �����
        if (PlayerWallet.Instance == null)
        {
            Debug.LogError("TestAddMoneyButton: PlayerWallet �� ������ �� �����! ������ ����� ���������.");
            testButton.interactable = false; // ��������� ������, ����� �������� ������
            return;
        }

        // ����������� ��� ����� � ������� onClick ������
        testButton.onClick.AddListener(AddMoneyForTest);
    }

    /// <summary>
    /// ���� ����� ����� ���������� ��� ������� �� ������.
    /// </summary>
    private void AddMoneyForTest()
    {
        // ��������� ��� ��� �� ������ ������
        if (PlayerWallet.Instance != null)
        {
            Debug.Log($"<color=lime>[TEST BUTTON]</color> ������ ������ ���������� �����. ��������� {amountToAdd}.");

            // �������� ��������� ����� �� PlayerWallet
            PlayerWallet.Instance.AddMoney(amountToAdd);

            // PlayerWallet.AddMoney() ��� ������� ��� ������ ������� (OnMoneyAdded � OnMoneyChanged),
            // ������� QuestManager � ������ ���������� ������� ����������� �������������.
        }
        else
        {
            Debug.LogError("TestAddMoneyButton: ������� �������� ������, �� PlayerWallet.Instance ����� null!");
        }
    }

    // ������� �������� - ������������ �� �������, ����� ������ ������������
    void OnDestroy()
    {
        if (testButton != null)
        {
            testButton.onClick.RemoveListener(AddMoneyForTest);
        }
    }
}