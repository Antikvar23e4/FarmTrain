using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlotScripts : MonoBehaviour
{
    public bool isPlanted = false; // ���� �� ��������
    public bool ishavebed = false;  // ���� �� ������
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


        if(selectedItem == null)
        {
            Debug.Log("������ ������� �� ���������");
        }

        else
        {
            // ���������, ���� �� ��������� ������� � �������� �� �� ������� ��� �������
            if (!selectedItem.IsEmpty && selectedItem.itemData.itemType == ItemType.Pot && !isPlanted)
            {

               

                if (ishavebed)
                {

                    Debug.Log("��� ��� ������, ����??");
                }
                else
                {
                    _itemSpawner.SpawnItem(selectedItem.itemData, transform.position);
                    ishavebed = true;
                    InventoryManager.Instance.RemoveItem(selectedIndex);

                }

            }

            if (!selectedItem.IsEmpty && selectedItem.itemData.itemType == ItemType.Seed && ishavebed && !isPlanted)
            {

                Transform parentSlot = transform.parent;

                BedSlotController bedSlotController = parentSlot.GetComponent<BedSlotController>();
                if (parentSlot != null)
                {
                    float weightSeed = selectedItem.itemData.associatedPlantData.Weight;

                    // �������� ���� �������� 
                 
                    switch (weightSeed)
                    {
                        case 1:
                            _itemSpawner.SpawnItem(selectedItem.itemData, transform.position);
                            isPlanted = true;
                            InventoryManager.Instance.RemoveItem(selectedIndex);
                            break;

                        case 2:
                           
                            if (bedSlotController != null)
                            {

                                bool isFreeSlot = bedSlotController.CheckFreeSlot(2);

                                if (isFreeSlot)
                                {
                                    _itemSpawner.SpawnItem(selectedItem.itemData, bedSlotController.Plant2Slot(gameObject.name));
                                    InventoryManager.Instance.RemoveItem(selectedIndex);
                                }
                                else
                                {
                                    Debug.Log("�� ������� ������, ���� ������ ���");

                                }

                            }
                            else
                            {
                                Debug.Log("������ ����������, ����������� ������ BedSlotController � ������������� Slot");
                            }
                            break;
                        case 4:
                          
                            if (bedSlotController != null)
                            {

                                bool isFreeSlot = bedSlotController.CheckFreeSlot(4);

                                if (isFreeSlot)
                                {
                                    _itemSpawner.SpawnItem(selectedItem.itemData, bedSlotController.Plant4Slot());
                                    InventoryManager.Instance.RemoveItem(selectedIndex);
                                }
                                else
                                {
                                    Debug.Log("�� ������� ������, ���� ������ ���");

                                }

                            }
                            else
                            {
                                Debug.Log("������ ����������, ����������� ������ BedSlotController � ������������� Slot");
                            }
                                break;

                    }

                }
                else
                {
                    Debug.LogError("�� ������ ������������ ����! ������");
                }
               
            }
            else
            {
                    if (isPlanted)
                    {

                        Debug.Log("��� ��� ������, ����??");
                    }
                    if (selectedItem.itemData.itemType != ItemType.Seed)
                    {
                        Debug.Log("��� ������ ������ ;)");
                    }
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
