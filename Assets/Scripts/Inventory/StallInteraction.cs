using UnityEngine;

public class StallInteraction : MonoBehaviour
{
    [Header("Shop Data")]
    public ShopInventoryData shopInventoryData;

    private void Start()
    {
        if (shopInventoryData != null && ShopDataManager.Instance != null)
        {
            ShopDataManager.Instance.InitializeShop(shopInventoryData);
        }
    }

    public void OpenShopUI()
    {
        if (shopInventoryData == null)
        {
            Debug.LogError($"� ������ {gameObject.name} �� ��������� ������ �������� (ShopInventoryData)!");
            return;
        }

        if (ShopUIManager.Instance == null)
        {
            Debug.LogError("ShopUIManager �� ������ �� �����! ���������� ������� �������.");
            return;
        }

        Debug.Log($"��������� UI ��� ��������: {shopInventoryData.name}");
        ShopUIManager.Instance.OpenShop(shopInventoryData);
    }
}