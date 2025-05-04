using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName = "New Item";
    public Sprite itemIcon = null;
    public ItemType itemType = ItemType.General;
    public bool isStackable = true;
    public int maxStackSize = 16;

    [TextArea(3, 5)]
    public string description = "";

    [Header("Farming Links (Optional)")]
    [Tooltip("������ � ��������, ���� ���� ������� �������� �������.")]
    public PlantData associatedPlantData; // ������ �� ������ ��������

    [Tooltip("������ � ��������, ���� ���� ������� ������������ ��� ��� ����������/��������.")]
    public AnimalData associatedAnimalData; // ������ �� ������ ���������

    [Tooltip("������ � ������")]
    public BedData associatedBedData; // ������ �� ������ ������

}

public enum ItemType
{
    General,
    Tool,
    Seed,
    Pot,
    AnimalProduct,
    PlantProduct,
    Fertilizer,
    Animal
}