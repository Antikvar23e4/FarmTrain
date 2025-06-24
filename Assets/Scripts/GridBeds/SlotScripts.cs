using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlotScripts : MonoBehaviour
{
    public bool isPlanted = false;
    bool ishavebed = false; // ���� �� ������ 
    Color currentColor;
    SpriteRenderer spriteRenderer;
    Transform slot;

    private InventoryManager inventoryManager; // ������ �� �������� ���������

    [SerializeField] GameObject _itemSpawnManager;

     ItemSpawner _itemSpawner;
    

    void Start()
    {

        inventoryManager = InventoryManager.Instance; // � ����� ��������� ����
        _itemSpawner = _itemSpawnManager.GetComponent<ItemSpawner>();
        if(_itemSpawner == null)
        {
            Debug.Log("itemSpaner not found!");
        }

        slot = transform.Find("Square");
        spriteRenderer = slot.GetComponent<SpriteRenderer>();
        currentColor = spriteRenderer.color;
        if (slot == null)
        {
            Debug.Log("not find");
        }
        else
        {
            // Debug.Log("<< find"); ����������� ��� ��� ������� ��� �������
        }
      
    }

    public void PlantSeeds()
    {
        
        // �������� ��������� ������� � ������ ���������� �����
        InventoryItem selectedItem = inventoryManager.GetSelectedItem();
        int selectedIndex = inventoryManager.SelectedSlotIndex; // ���������� ����� ��������

        // ���������, ���� �� ��������� ������� � �������� �� �� ��������
        if (selectedItem != null && !selectedItem.IsEmpty && selectedItem.itemData.itemType == ItemType.Pot && !isPlanted)
        {

                _itemSpawner.SpawnItem(selectedItem.itemData, transform.position);
                isPlanted = true;
                InventoryManager.Instance.RemoveItem(selectedIndex);
        }
        else
        {
            if (selectedItem != null)
            {

                if (isPlanted)
                {

                    Debug.Log("��� ��� ������, ����??");
                }
                if (selectedItem.itemData.itemType != ItemType.Pot)
                {
                    Debug.Log("������� ����� �������� ;)");
                }

                
            }
            else
            {
                Debug.Log($"������ ������� �� ���������");
            }
        }


        if(selectedItem != null && !selectedItem.IsEmpty && selectedItem.itemData.itemType == ItemType.Seed && !ishavebed && isPlanted)
        {
            _itemSpawner.SpawnItem(selectedItem.itemData, transform.position);
            ishavebed = true;
            InventoryManager.Instance.RemoveItem(selectedIndex);
        }
        else
        {
            if (selectedItem != null)
            {

                if (ishavebed)
                {

                    Debug.Log("��� ��� ������, ����??");
                }
                if (selectedItem.itemData.itemType != ItemType.Seed)
                {
                    Debug.Log("��� ������ ������ ;)");
                }
               


            }
            else
            {
                Debug.Log($"������ ������� �� ���������");
            }
        }
    }
    public void ChangeColor()
    {

        slot.GetComponent<SpriteRenderer>().color = new Color(0, 255f, 0, 0.1f);

    }
    public void UnChangeColor()
    {
        slot.GetComponent<SpriteRenderer>().color = currentColor;
    }
}
