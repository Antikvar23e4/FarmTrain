using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlotScripts : MonoBehaviour
{
    public bool isPlanted = false; // ���� �� ��������
    public bool ishavebed = false;  // ���� �� ������
    public bool isRaked = false; // ���������� �� ������
    Color currentColor;
    SpriteRenderer spriteRenderer;
    Transform slot;



    private InventoryManager inventoryManager; // ������ �� �������� ���������

   

    [SerializeField] ItemSpawner _itemSpawner;
    

    void Start()
    {

        inventoryManager = InventoryManager.Instance; // � ����� ��������� ����
       
        if(_itemSpawner == null)
        {
            Debug.Log("itemSpaner not found!");
        }

        //slot = transform.Find("Square");
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentColor = spriteRenderer.color;
        //if (slot == null)
        //{
        //    Debug.Log("not find");
        //}
        //else
        //{
        //    // Debug.Log("<< find"); ����������� ��� ��� ������� ��� ������� 
        //    spriteRenderer = slot.GetComponent<SpriteRenderer>();
        //}
      
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
            if (!selectedItem.IsEmpty && selectedItem.itemData.itemType == ItemType.Pot)
            {

               

                if (ishavebed)
                {

                    Debug.Log("��� ��� ������, ����??");
                }
                else
                {
                    _itemSpawner.TestSpawnBed(selectedItem.itemData, transform.position, new Vector3(0.25f,0.25f,0.25f), gameObject.transform);
                    ishavebed = true;
                    InventoryManager.Instance.RemoveItem(selectedIndex);

                }

            }

            if (!selectedItem.IsEmpty && selectedItem.itemData.itemType == ItemType.Seed && !isPlanted)
            {
                if (ishavebed) {
                    if (isRaked) {
                        if (!isPlanted) {
                            Transform parentSlot = transform.parent;

                            BedSlotController bedSlotController = parentSlot.GetComponent<BedSlotController>();
                            GridGenerator gridGenerator = parentSlot.GetComponent<GridGenerator>();



                            if (parentSlot != null)
                            {
                                float weightSeed = selectedItem.itemData.associatedPlantData.Weight;

                                // �������� ���� �������� 

                                switch (weightSeed)
                                {
                                    case 1:
                                        _itemSpawner.TestSpawnPlant(selectedItem.itemData, transform.position, new Vector3(0.5f, 0.5f, 0.5f),gameObject.transform);
                                        isPlanted = true;
                                        InventoryManager.Instance.RemoveItem(selectedIndex);
                                        break;

                                    case 2:

                                        if (gridGenerator != null)
                                        {

                                            bool isFreeSlot = gridGenerator.CheckFree2Slot(name);

                                            if (isFreeSlot)
                                            {
                                                _itemSpawner.TestSpawnPlant(selectedItem.itemData, transform.position, new Vector3(0.5f, 0.5f, 0.5f),gameObject.transform.parent);
                                                InventoryManager.Instance.RemoveItem(selectedIndex);
                                            }
                                            else
                                            {
                                                Debug.Log("�� ������� ������, ���� ������ ���");

                                            }

                                        }
                                        else
                                        {
                                            Debug.Log("������ ����������, ����������� ������ gridGenerator � ������������� Slot");
                                        }
                                        break;
                                    case 4:

                                        if (gridGenerator != null)
                                        {

                                            bool isFreeSlot = gridGenerator.CheckSquareCells(name).Item1;
                                            Vector3 Plantposition = gridGenerator.CheckSquareCells(name).Item2;

                                            if (isFreeSlot)
                                            {
                                                _itemSpawner.TestSpawnPlant(selectedItem.itemData, Plantposition, new Vector3(0.5f, 0.5f, 0.5f),gameObject.transform.parent);
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
                            Debug.Log("��� ��� ������, ����??");
                        }
                    }
                    else
                    {
                        Debug.Log("������� ���� ���������� ������, � ����� ��� ������ ��������");

                    }

                }
                else
                {
                    Debug.Log("������� ���� ��������� ������!");
                }

                
               
            }
         

            if(!selectedItem.IsEmpty && selectedItem.itemData.itemType == ItemType.Tool)
            {
                if (ishavebed)
                {
                    if (selectedItem.itemData.itemName == "Rake")
                    {
                        GameObject childBed = FindChildWithTag("Bed");
                        if (childBed != null)
                        {
                            BedController bedController = childBed.GetComponent<BedController>();
                            if (bedController != null) {

                                bedController.ChangeStage(BedData.StageGrowthPlant.Raked, 1);
                                isRaked = true;
                            }
                            else
                            {
                                Debug.LogError("bedController �� ������");
                            }
                        }
                        else
                        {
                            Debug.LogError("������ �� �������� �������� ��� �����, ������");
                        }
                    }
                    if(selectedItem.itemData.itemName == "Shovel")
                    {
                        if (isPlanted)
                        {
                            GameObject plant = FindChildWithTag("Plant");
                            Destroy(plant);
                            isPlanted = false;
                        }
                        else
                        {
                            Debug.Log("����� ��� ��������, ������ ���������� ");
                        }
                    }
                }
                else
                {
                    Debug.Log("������������ ����� ������ ���������� ������!");
                }
            }

        }
        
    }

    private GameObject FindChildWithTag(string tag)
    {
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child.CompareTag(tag))
            {
                return child.gameObject;
            }
        }
        Debug.LogWarning($"No child with tag {tag} found.");
        return null;
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
