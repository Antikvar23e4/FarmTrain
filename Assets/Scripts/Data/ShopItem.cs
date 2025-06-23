using UnityEngine;

[System.Serializable]
public class ShopItem
{
    public ItemData itemData; // ������ �� �������� ScriptableObject ��������

    [Header("Trading Properties")]
    public int buyPrice = 10;
    public int sellPrice = 5;

    // true - ���� ����� ����� ������ � ��������
    public bool forSale = true;
    // true - ���� ����� ����� ������� ��������
    public bool willBuy = true;

    [Header("Stock")]
    // true - � �������� ����������� ����� ����� ������
    public bool isInfiniteStock = false;
    // ��������� ���������� ������, ���� �� �� �����������
    [Min(0)] public int initialStock = 10;
}