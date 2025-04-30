using UnityEngine;

// ���� ������ �������� �� �������� ������ ������� �������
public class BubbleYSorter : MonoBehaviour
{
    // ������ �� SpriteRenderer ���� �������
    [SerializeField] private SpriteRenderer backgroundRenderer;
    // ������ �� SpriteRenderer ������ ������ �������
    [SerializeField] private SpriteRenderer iconRenderer;

    // Transform ���������-���������. ����� ���������� �� AnimalController.
    private Transform ownerTransform;

    // ���������, ��� � YSorter ��� ��������
    private const int sortingOrderMultiplier = -100;

    // ������� ������� ��� ���� ������ ������� (����� �� ��� "������")
    private const int backgroundOrderOffset = 0;
    // �������� ��� ������, ����� ��� ���� ������ ����
    private const int iconOrderOffset = 1;

    void Awake()
    {
        // ��������, ��� ��������� ��������� � ����������
        if (backgroundRenderer == null)
        {
            Debug.LogError("Background Renderer �� �������� � BubbleYSorter!", gameObject);
        }
        if (iconRenderer == null)
        {
            Debug.LogError("Icon Renderer �� �������� � BubbleYSorter!", gameObject);
        }
    }

    // ����� ��� ��������� ��������� ����� (�� AnimalController)
    public void SetOwner(Transform owner)
    {
        ownerTransform = owner;
        // ����� ����� �������� ������� ��� ��������� ���������
        UpdateSortOrder();
    }

    // ���������� LateUpdate, ����� ������� ��������� ���� ����������
    void LateUpdate()
    {
        UpdateSortOrder();
    }

    void UpdateSortOrder()
    {
        if (ownerTransform == null || backgroundRenderer == null || iconRenderer == null)
        {
            // ���� �������� ��� �� ���������� ��� ��������� �������, ������ �� ������
            // ����� ������ ��������� ��� ���������� �� ��������� ������ �������
            // backgroundRenderer.sortingOrder = -10000;
            // iconRenderer.sortingOrder = -10000 + 1;
            return;
        }

        // �������� Y-���������� ��������� (���������� ��� ����/���������, ���� ���� YSorter)
        // ������� �������: ������ Y ������� ���������
        float ownerY = ownerTransform.position.y;

        // ������������ ������� sortingOrder ��� ����� �������
        int baseSortingOrder = Mathf.RoundToInt(ownerY * sortingOrderMultiplier);

        // ������������� sortingOrder ��� ���� � ������ ������������ ��������
        backgroundRenderer.sortingOrder = baseSortingOrder + backgroundOrderOffset; // ���
        iconRenderer.sortingOrder = baseSortingOrder + iconOrderOffset;       // ������ (����� +1 ������������ ����)

        // �����������: �����������
        // Debug.Log($"Bubble for {ownerTransform.name} - OwnerY: {ownerY}, BaseOrder: {baseSortingOrder}, BgOrder: {backgroundRenderer.sortingOrder}, IconOrder: {iconRenderer.sortingOrder}");
    }
}