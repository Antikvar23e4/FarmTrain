using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(WorldItem))] 
public class ItemPickup : MonoBehaviour
{
    public int quantity = 1;

    [Header("��������� ������� �� �����")]
    [Tooltip("���, ������� ������ ���� � ����� �������, ����� ��� ����� ���� ��������� ������.")]
    public string requiredTagForPickup = "CanBePickedUp";

    private WorldItem worldItem; 

    void Awake()
    {
        worldItem = GetComponent<WorldItem>();
        if (worldItem == null)
        {
            Debug.LogError($"�� ������� {gameObject.name} ����������� ��������� WorldItem, ����������� ��� ItemPickup!", this);
        }
    }


    private void OnMouseDown()
    {
        if (GameStateManager.Instance != null && GameStateManager.Instance.IsGamePaused)
        {
            return;
        }

        if (gameObject.CompareTag(requiredTagForPickup))
        {
            AttemptPickup();
        }
    }

    public void AttemptPickup()
    {
        if (worldItem == null)
        {
            Debug.LogError("����������� ��������� WorldItem, �� ���� �������� ������ ��������!", this);
            return;
        }

        ItemData dataToPickup = worldItem.GetItemData();

        if (dataToPickup == null)
        {
            Debug.LogError("��������� WorldItem �� �������� ������ (itemData is null)!", this);
            return;
        }

        bool added = InventoryManager.Instance.AddItem(dataToPickup, quantity);

        if (added)
        {
            Debug.Log($"�������� �������: {dataToPickup.itemName} (��� �������: {gameObject.tag})");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log($"�� ������� ��������� {dataToPickup.itemName} - ��������� �����?");
        }
    }
}