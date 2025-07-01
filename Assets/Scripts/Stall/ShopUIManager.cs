using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;

public class ShopUIManager : MonoBehaviour
{
    public static ShopUIManager Instance { get; private set; }

    [Header("Main Panel")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private TextMeshProUGUI shopNameText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button buyTabButton;
    [SerializeField] private Button sellTabButton;

    [Header("Item Display")]
    [SerializeField] private RectTransform itemsContentArea; 
    [SerializeField] private GameObject shopItemRowPrefab; 

    [Header("Confirmation Panel")]
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private TextMeshProUGUI confirmationText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private TextMeshProUGUI totalPriceText;
    [SerializeField] private Button plusButton;
    [SerializeField] private Button minusButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private ShopInventoryData currentShopData;
    private ShopItem currentItemForTransaction;
    private bool isBuyMode = true;
    private int transactionQuantity = 1;

    private List<GameObject> spawnedRows = new List<GameObject>();
    public event Action<ItemData, int> OnItemPurchased;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        shopPanel.SetActive(false);
        confirmationPanel.SetActive(false);

        closeButton.onClick.AddListener(CloseShop);
        buyTabButton.onClick.AddListener(() => SetMode(true));
        sellTabButton.onClick.AddListener(() => SetMode(false));

        plusButton.onClick.AddListener(IncreaseQuantity);
        minusButton.onClick.AddListener(DecreaseQuantity);
        confirmButton.onClick.AddListener(ConfirmTransaction);
        cancelButton.onClick.AddListener(() => confirmationPanel.SetActive(false));
    }

    public void OpenShop(ShopInventoryData shopData)
    {
        if (StallCameraController.Instance == null)
        {
            Debug.LogWarning("StallCameraController �� ������. ������� ��������� ��� ���������� �������.");
        }

        currentShopData = shopData;
        shopNameText.text = shopData.shopName;
        SetMode(true);
        shopPanel.SetActive(true);
    }


    public void CloseShop()
    {
        if (!shopPanel.activeSelf) return;

        Debug.Log("�������� ������ ��������.");
        shopPanel.SetActive(false);
        currentShopData = null;

        if (StallCameraController.Instance != null)
        {
            StallCameraController.Instance.EnterOverviewMode();
        }
    }


    private void SetMode(bool buy)
    {
        isBuyMode = buy;
        buyTabButton.interactable = !isBuyMode;
        sellTabButton.interactable = isBuyMode;

        PopulateShopList();
    }

    private void PopulateShopList()
    {
        foreach (var row in spawnedRows)
        {
            Destroy(row);
        }
        spawnedRows.Clear();

        foreach (var shopItem in currentShopData.shopItems)
        {
            if (isBuyMode && !shopItem.forSale) continue;
            if (!isBuyMode && !shopItem.willBuy) continue;

            GameObject rowGO = Instantiate(shopItemRowPrefab, itemsContentArea);
            ShopItemRow rowScript = rowGO.GetComponent<ShopItemRow>();

            int shopStock = ShopDataManager.Instance.GetCurrentStock(currentShopData, shopItem.itemData);

            int playerItemCount = 0;
            if (shopItem.itemData.itemType == ItemType.Animal)
            {
                playerItemCount = AnimalPenManager.Instance.GetAnimalCount(shopItem.itemData.associatedAnimalData);
            }
            else
            {
                playerItemCount = InventoryManager.Instance.GetTotalItemQuantity(shopItem.itemData);
            }

            rowScript.Setup(shopItem, shopStock, playerItemCount, isBuyMode, OnItemActionClicked);
            spawnedRows.Add(rowGO);
        }

    }

    private void OnItemActionClicked(ShopItem shopItem)
    {
        Debug.Log($"������ ������ ��� {shopItem.itemData.itemName}");
        currentItemForTransaction = shopItem;
        transactionQuantity = 1; 
        OpenConfirmationPanel();
    }

    private void OpenConfirmationPanel()
    {
        UpdateConfirmationPanel();
        confirmationPanel.SetActive(true);
    }

    private void UpdateConfirmationPanel()
    {
        // --- �����-����� ������ ---
        Debug.Log("[!!! DEBUG !!!] ������� UpdateConfirmationPanel.");

        if (currentItemForTransaction == null)
        {
            Debug.LogError("[!!! DEBUG !!!] ������: currentItemForTransaction is NULL. �����.");
            return;
        }

        var itemData = currentItemForTransaction.itemData;
        Debug.Log($"[!!! DEBUG !!!] �������� � ���������: {itemData.itemName}");

        int price = isBuyMode ? currentItemForTransaction.buyPrice : currentItemForTransaction.sellPrice;
        int maxQuantity = int.MaxValue;

        if (itemData.itemType == ItemType.Animal)
        {
            Debug.Log($"[!!! DEBUG !!!] ��� �������� - Animal. ����� �������: {isBuyMode}");
            if (isBuyMode)
            {
                // �������� ������������ �����������
                int stock = ShopDataManager.Instance.GetCurrentStock(currentShopData, itemData);
                int affordable = (price > 0) ? PlayerWallet.Instance.GetCurrentMoney() / price : int.MaxValue;
                maxQuantity = Mathf.Min(stock, affordable);
                Debug.Log($"[!!! DEBUG !!!] ��������� �����������: �����={stock}, �� �������={affordable}. ��������� maxQuantity={maxQuantity}");

                if (TrainPenController.Instance != null)
                {
                    Debug.Log("[!!! DEBUG !!!] TrainPenController ������. ������� �������� ������.");
                    var animalData = itemData.associatedAnimalData;

                    if (animalData != null)
                    {
                        Debug.Log($"[!!! DEBUG !!!] ������� ������ �� AnimalData: {animalData.name}. ���� ��� ���� �����...");
                        PenInfo penInfo = TrainPenController.Instance.GetPenInfoForAnimal(animalData);

                        if (penInfo != null)
                        {
                            Debug.Log($"[!!! DEBUG !!!] ����� ������! ����������� (MaxCapacity) = {penInfo.maxCapacity}");
                            int currentAnimalCount = AnimalPenManager.Instance.GetAnimalCount(animalData);
                            Debug.Log($"[!!! DEBUG !!!] �������� ����� ���� ������: {currentAnimalCount}");

                            int availableSpace = penInfo.maxCapacity - currentAnimalCount;
                            Debug.Log($"[!!! DEBUG !!!] ��������� ����: {availableSpace}");

                            maxQuantity = Mathf.Min(maxQuantity, availableSpace);
                            Debug.Log($"[!!! DEBUG !!!] �������� maxQuantity ����� �������� ������ = {maxQuantity}");
                        }
                        else
                        {
                            Debug.LogError($"[!!! DEBUG !!!] ����� �� ������ ��� {animalData.name}! ���������, �������� �� �� � Pen Configurations � TrainPenController.");
                            maxQuantity = 0;
                        }
                    }
                    else
                    {
                        Debug.LogError($"[!!! DEBUG !!!] � �������� {itemData.name} ���� associatedAnimalData ������ (None)!");
                        maxQuantity = 0;
                    }
                }
                else
                {
                    Debug.LogError("[!!! DEBUG !!!] TrainPenController.Instance is NULL!");
                }
            }
            else // ����� �������
            {
                maxQuantity = AnimalPenManager.Instance.GetAnimalCount(itemData.associatedAnimalData);
                Debug.Log($"[!!! DEBUG !!!] ����� �������. �������� ��� �������: {maxQuantity}");
            }
        }
        else
        {
            Debug.Log($"[!!! DEBUG !!!] ��� �� ��������. ����������� ������.");
            // ������ ��� ������� ���������
            if (isBuyMode)
            {
                int stock = ShopDataManager.Instance.GetCurrentStock(currentShopData, itemData);
                int affordable = (price > 0) ? PlayerWallet.Instance.GetCurrentMoney() / price : int.MaxValue;
                maxQuantity = Mathf.Min(stock, affordable);
            }
            else
            {
                maxQuantity = InventoryManager.Instance.GetTotalItemQuantity(itemData);
            }
        }

        transactionQuantity = Mathf.Clamp(transactionQuantity, 1, maxQuantity);
        if (maxQuantity <= 0) transactionQuantity = 0;

        Debug.Log($"[!!! DEBUG !!!] ��������� ������: transactionQuantity={transactionQuantity}, maxQuantity={maxQuantity}");
        quantityText.text = transactionQuantity.ToString();
        totalPriceText.text = $"{transactionQuantity * price} BYN";
        confirmButton.interactable = transactionQuantity > 0;

        Debug.Log("[!!! DEBUG !!!] ����� UpdateConfirmationPanel.");
    }



    private void IncreaseQuantity()
    {
        transactionQuantity++;
        UpdateConfirmationPanel();
    }

    private void DecreaseQuantity()
    {
        transactionQuantity--;
        if (transactionQuantity < 1) transactionQuantity = 1;
        UpdateConfirmationPanel();
    }

    private void ConfirmTransaction()
    {
        if (transactionQuantity <= 0) return;

        var itemData = currentItemForTransaction.itemData;
        int totalPrice = (isBuyMode ? currentItemForTransaction.buyPrice : currentItemForTransaction.sellPrice) * transactionQuantity;

        if (itemData.itemType == ItemType.Animal)
        {
            if (isBuyMode)
            {
                if (TrainPenController.Instance != null)
                {
                    // 1. ����� ��� �� �������� AnimalData �� ItemData
                    var animalData = itemData.associatedAnimalData;

                    if (animalData != null)
                    {
                        PenInfo penInfo = TrainPenController.Instance.GetPenInfoForAnimal(animalData);
                        int currentAnimalCount = AnimalPenManager.Instance.GetAnimalCount(animalData);

                        if (penInfo == null || currentAnimalCount + transactionQuantity > penInfo.maxCapacity)
                        {
                            Debug.LogError($"������! ������� ������ {transactionQuantity} ��������, �� � ������ ��� �����. " +
                                           $"({currentAnimalCount} + {transactionQuantity} > {penInfo?.maxCapacity}). ���������� ��������.");
                            confirmationPanel.SetActive(false);
                            return;
                        }
                    }
                    else
                    {
                        Debug.LogError($"� �������� {itemData.itemName} �� ������� ������ �� AnimalData! ���������� ��������.");
                        confirmationPanel.SetActive(false);
                        return;
                    }
                }
                if (!PlayerWallet.Instance.HasEnoughMoney(totalPrice)) return;

                PlayerWallet.Instance.SpendMoney(totalPrice);
                ShopDataManager.Instance.DecreaseStock(currentShopData, itemData, transactionQuantity);
                for (int i = 0; i < transactionQuantity; i++)
                {
                    AnimalPenManager.Instance.AddAnimal(itemData.associatedAnimalData);
                }
                
            }
            else 
            {
                if (AnimalPenManager.Instance.GetAnimalCount(itemData.associatedAnimalData) < transactionQuantity) return;
                if (TrainPenController.Instance != null)
                {
                    for (int i = 0; i < transactionQuantity; i++)
                    {
                        bool despawned = TrainPenController.Instance.DespawnAnimal(itemData.associatedAnimalData);
                        if (!despawned)
                        {
                            Debug.LogError("������ �������������! �� ������� ������� ���������� ������������� ���������.");
                            return;
                        }
                    }
                }

                PlayerWallet.Instance.AddMoney(totalPrice);
                ShopDataManager.Instance.IncreaseStock(currentShopData, itemData, transactionQuantity);
                for (int i = 0; i < transactionQuantity; i++)
                {
                    AnimalPenManager.Instance.SellAnimal(itemData.associatedAnimalData);
                }
            }
        }
        else
        {
            if (isBuyMode)
            {
                if (!PlayerWallet.Instance.HasEnoughMoney(totalPrice)) return;
                if (!InventoryManager.Instance.CheckForSpace(itemData, transactionQuantity)) return;

                PlayerWallet.Instance.SpendMoney(totalPrice);
                InventoryManager.Instance.AddItem(itemData, transactionQuantity);
                ShopDataManager.Instance.DecreaseStock(currentShopData, itemData, transactionQuantity);
                if (itemData.itemType == ItemType.Tool)
                {
                    TrainUpgradeManager.Instance.PurchaseUpgrade(itemData);
                }
            }
            else
            {
                if (InventoryManager.Instance.GetTotalItemQuantity(itemData) < transactionQuantity) return;

                PlayerWallet.Instance.AddMoney(totalPrice);
                InventoryManager.Instance.RemoveItemByType(itemData, transactionQuantity);
                OnItemPurchased?.Invoke(itemData, transactionQuantity);
                ShopDataManager.Instance.IncreaseStock(currentShopData, itemData, transactionQuantity);
            }
        }

        Debug.Log("���������� �������!");
        confirmationPanel.SetActive(false);
        PopulateShopList();
    }


}