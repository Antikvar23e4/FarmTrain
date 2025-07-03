using UnityEngine;

public class AutoScrollParallax : MonoBehaviour
{
    [Tooltip("��������, � ������� ����� ��������� ���� ����. ������������� �������� - �������� �����.")] public float scrollSpeed = -2f; // ��������� ������� �������� ��� ����� �������� ��������

    private float spriteWidth;
    private Vector3 startPosition;

    void Start()
    {
        // ��������� ��������� ������� �������
        startPosition = transform.position;

        // ��������� ������ ������� � ������� �����������
        spriteWidth = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        // ������� ������ ����� � ���������� ���������
        // Time.deltaTime ������ �������� ������� � �� ��������� �� FPS
        transform.Translate(Vector3.right * scrollSpeed * Time.deltaTime); // ��������� ��������� ��������

        // ���������, ����� �� "���������������" ��� ��� �������� �������������
        // Mathf.Repeat ����������� ������� � ��������� �� 0 �� ������ �������
        float newPosition = Mathf.Repeat(Time.time * scrollSpeed * 2f, spriteWidth) - 2; // ��������� �������� � ����������
        transform.position = startPosition + Vector3.right * newPosition;
    }

}