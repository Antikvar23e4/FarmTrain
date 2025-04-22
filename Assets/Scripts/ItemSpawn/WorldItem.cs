using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]

public class WorldItem : MonoBehaviour
{
    public ItemData itemData;
    private SpriteRenderer spriteRenderer;
    private Collider2D itemCollider;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        itemCollider = GetComponent<Collider2D>();
        if (itemCollider == null)
        {
            Debug.LogError($"�� ������� {gameObject.name} ����������� Collider2D, ���� �� ���������!", this);
        }
    }

    public void InitializeVisuals()
    {
        if (itemData != null)
        {
            spriteRenderer.sprite = itemData.itemIcon;
            gameObject.name = $"{itemData.itemName}"; 

            UpdateCollider();
        }
        else
        {
            Debug.LogWarning($"ItemData �� �������� ��� WorldItem �� ������� {gameObject.name}");
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            if (itemCollider != null) itemCollider.enabled = false;
        }
    }

    // ����� ��� ���������� ���������� ��� ������ �������
    private void UpdateCollider()
    {
        if (itemCollider == null || spriteRenderer == null || spriteRenderer.sprite == null)
        {
            Debug.LogWarning($"�� ���� �������� ��������� ��� {gameObject.name}: ��������� ��� ������ �����������.");
            return;
        }

        BoxCollider2D boxCollider = itemCollider as BoxCollider2D;
        if (boxCollider != null)
        {
            // ������������� ������ � �������� ���������� ������� �������� �������
            boxCollider.size = spriteRenderer.sprite.bounds.size;
            boxCollider.offset = spriteRenderer.sprite.bounds.center;
        }
        else
        {
            // ���� ��� �� BoxCollider2D, �������� ������ ��� ������ ����� (�� �����)
            Debug.LogWarning($"��������� �� {gameObject.name} �� �������� BoxCollider2D. �������������� ��������� ������� �� �������������� ��� ���� {itemCollider.GetType()}.");
        }
    }

    public ItemData GetItemData()
    {
        return itemData;
    }
}