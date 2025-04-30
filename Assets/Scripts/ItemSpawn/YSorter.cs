using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))] // ��������, ��� SpriteRenderer ����
public class YSorter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    // ���� ����������� ������� � (�������� ������ ��� ������� �����):
    // �������������� � �������� ������ "FeetPosition" ���� � ���������� �������
    // [SerializeField] private Transform sortPointTransform;

    // ��������� ��� �������������� Y � sortingOrder.
    // �������������, �.�. ��� ���� Y, ��� ������ ������ ���� sortingOrder.
    // �������� 100 ������ ����������, ����� �������� ���������� � ������ ������� Y.
    private const int sortingOrderMultiplier = -100;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate() // ���������� LateUpdate, ����� ������� ������� ��� ����������
    {
        if (spriteRenderer == null) return;

        float sortY;

        // ���������� Y-���������� ��� ����������
        // ---- ������� � (���������� Pivot ������� / ������� �������) ----
        sortY = transform.position.y;

        // ---- ������� � (���������� �������� ������ sortPointTransform) ----
        /*
        if (sortPointTransform != null)
        {
            sortY = sortPointTransform.position.y;
        }
        else
        {
            // �������� �������, ���� sortPointTransform �� ��������
            sortY = transform.position.y;
            // ����� �������� �������������� � Awake, ���� �� �� ��������
            // Debug.LogWarning($"SortPointTransform �� �������� ��� {gameObject.name}, ������������ ������� �������.", gameObject);
        }
        */ // �������������/�������������� ������ �������

        // ������������ � ������������� sortingOrder
        // �������� �� -100 � �������� � int.
        // ��� ������ Y, ��� ������ ����� �������� sortingOrder.
        spriteRenderer.sortingOrder = Mathf.RoundToInt(sortY * sortingOrderMultiplier);

        // �����������: ����������� ��� �������
        // Debug.Log($"{gameObject.name} - Y: {sortY}, SortingOrder: {spriteRenderer.sortingOrder}");
    }
}