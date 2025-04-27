using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BedsScripts : MonoBehaviour
{
    public bool isPlanted = false;
    Color currentColor;
    SpriteRenderer spriteRenderer;
    Transform slot;

    Transform seed;


    private InventoryManager inventoryManager; // ������ �� �������� ���������


    

    void Start()
    {

        inventoryManager = InventoryManager.Instance; // � ����� ��������� ����


        slot = transform.Find("Square");
        seed = transform.Find("Seed");
        spriteRenderer = slot.GetComponent<SpriteRenderer>();
        currentColor = spriteRenderer.color;
        if (slot == null)
        {
            Debug.Log("not find");
        }
        else
        {
            Debug.Log("<< find");
        }
      
    }

    public void PlantSeeds()
    {
        
        // �������� ��������� ������� � ������ ���������� �����
        InventoryItem selectedItem = inventoryManager.GetSelectedItem();
        int selectedIndex = inventoryManager.SelectedSlotIndex; // ���������� ����� ��������

        // ���������, ���� �� ��������� ������� � �������� �� �� ��������
        if (selectedItem != null && !selectedItem.IsEmpty && selectedItem.itemData.itemType == ItemType.Seed && !isPlanted)
        {
          
                seed.gameObject.SetActive(true);
                isPlanted = true;
                InventoryManager.Instance.RemoveItem(selectedIndex);
        }
        else
        {
            if (isPlanted) {

                Debug.Log("��� ��� ������, ����??");
            }
            if(selectedItem.itemData.itemType != ItemType.Seed)
            {
                Debug.Log("������ ����� ������ ������ ;)");
            }

            Debug.Log($"�� ������� �������� :(");
            //interactionSuccessful = false; // ��������� �� �������
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
