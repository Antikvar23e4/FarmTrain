using UnityEngine;

public class InfiniteScroller : MonoBehaviour
{
    [Tooltip("�������� �������� ����. ������������� �������� - �����, ������������� - ������.")]
    public float scrollSpeed = -5f;

    private float spriteWidth;
    private Vector2 startPosition;

    void Start()
    {
        // �������� ������ �������
        spriteWidth = GetComponent<SpriteRenderer>().bounds.size.x;
        startPosition = transform.position;
    }

    void Update()
    {
        // ������� ������
        transform.Translate(Vector3.right * scrollSpeed * Time.deltaTime);

        // ���������, ���� �� ������ ��������� �� ����� �����
        // �� ���������, ����� �� ����������� �� ������ ���������� ��������� �� ���� ������ ������
        if (transform.position.x < startPosition.x - spriteWidth)
        {
            // ���������, �� ������� ����� "������������" ������
            // �� ������� ��� ������ �� ��� ������ ������� �� ������� �������, ����� �� �������� ������ �� ������ "���������"
            transform.position += new Vector3(spriteWidth * 2f, 0, 0);
        }
    }
}