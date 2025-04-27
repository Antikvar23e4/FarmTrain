using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System; 

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; } // Singleton

    [Header("Inventory Data")]
    [SerializeField] private int hotbarSize = 7;
    [SerializeField] private int maxMainInventoryRows = 3;
    [SerializeField] private int columns = 7;
    private int currentMainInventoryRows = 3; // ������� ���������� ����� ������
    private List<InventoryItem> inventoryItems; // ������ ������ ��� ���� ������

    [Header("UI References")]
    [SerializeField] private GameObject hotbarPanel; // ������ �������
    [SerializeField] private GameObject mainInventoryPanel; // ������ ������
    [SerializeField] private GameObject slotPrefab; // ������ ����� UI
    [SerializeField] private Button inventoryToggleButton; // ������ ��� ��������/�������� ������
    [SerializeField] private GameObject inventoryBackgroundPanel;
    
    [Header("Selection")]
    [SerializeField] private int selectedSlotIndex = 0;





    [Header("BedManager")]
    [SerializeField] GameObject BedManager; // ��� ���������� � �������������� ������
    public int SelectedSlotIndex => selectedSlotIndex; // ��������� �������� ��� ������

    private List<InventorySlotUI> hotbarSlotsUI = new List<InventorySlotUI>();
    private List<InventorySlotUI> mainInventorySlotsUI = new List<InventorySlotUI>();

    public event Action OnInventoryChanged;
    public event Action<int> OnSelectedSlotChanged;

    #region Initialization

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject);

        InitializeInventory();
    }

    private void Start()
    {
        CreateSlotsUI();
        SetupToggleButton();
        UpdateAllSlotsUI(); 
        SelectSlot(selectedSlotIndex);
        UpdateMainInventoryUIVisibility();
    }

    private void InitializeInventory()
    {
        int totalSlots = hotbarSize + (maxMainInventoryRows * columns); // ?
        inventoryItems = new List<InventoryItem>(totalSlots);
        for (int i = 0; i < totalSlots; i++)
        {
            inventoryItems.Add(null); // ��������� null ��� new InventoryItem(null, 0)
        }
    }

    private void CreateSlotsUI()
    {
        for (int i = 0; i < hotbarSize; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, hotbarPanel.transform);
            slotGO.name = $"HotbarSlot_{i}";
            InventorySlotUI slotUI = slotGO.GetComponent<InventorySlotUI>();
            if (slotUI != null)
            {
                slotUI.Setup(i, true, OnSlotClicked);
                hotbarSlotsUI.Add(slotUI);
            }
            else
            {
                Debug.LogError($"Slot Prefab '{slotPrefab.name}' is missing InventorySlotUI script!");
                return; 
            }
        }

        int mainInventoryStartIndex = hotbarSize;
        for (int i = 0; i < maxMainInventoryRows * columns; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, mainInventoryPanel.transform);
            slotGO.name = $"MainInventorySlot_{i}";
            InventorySlotUI slotUI = slotGO.GetComponent<InventorySlotUI>();
            if (slotUI != null)
            {
                slotUI.Setup(mainInventoryStartIndex + i, false, OnSlotClicked);
                mainInventorySlotsUI.Add(slotUI);
            }
            else
            {
                Debug.LogError($"Slot Prefab '{slotPrefab.name}' is missing InventorySlotUI script!");
                return;
            }
        }
    }

    private void SetupToggleButton()
    {
        if (inventoryToggleButton != null)
        {
            inventoryToggleButton.onClick.AddListener(ToggleMainInventory);
        }
        else
        {
            Debug.LogWarning("Inventory Toggle Button is not assigned in the Inspector.");
        }
    }

    #endregion

    #region Input Handling

    private void Update()
    {
        HandleHotbarInput();
    }

    private void HandleHotbarInput()
    {
        // ����� ����� ������� ��������� 1-7
        for (int i = 0; i < hotbarSize; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) 
            {
                SelectSlot(i);
                break;
            }
        }
    }

    private void OnSlotClicked(int index)
    {
        Debug.Log($"Clicked on slot index: {index}");
        if (index < hotbarSize)
        {
            SelectSlot(index);
        }
        else
        {
            Debug.Log($"Clicked on main inventory slot. Item: {GetItemInSlot(index)?.itemData?.itemName ?? "Empty"}");
        }
    }

    #endregion

    #region Inventory Management (Add, Remove, Find)

    public bool AddItem(ItemData itemToAdd, int quantity = 1)
    {
        if (itemToAdd == null || quantity <= 0) return false;

        // 1. ����� ������������� �����
        if (itemToAdd.isStackable)
        {
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                // ���������� ���������� ����� ��������� ���������
                if (!IsSlotActive(i)) continue;

                InventoryItem currentItem = inventoryItems[i];
                if (currentItem != null && !currentItem.IsEmpty && currentItem.itemData == itemToAdd)
                {
                    int spaceAvailable = itemToAdd.maxStackSize - currentItem.quantity;
                    if (spaceAvailable >= quantity)
                    {
                        currentItem.AddQuantity(quantity);
                        UpdateSlotUI(i);
                        OnInventoryChanged?.Invoke(); // ���������� �� ���������
                        return true; 
                    }
                    else if (spaceAvailable > 0)
                    {
                        currentItem.AddQuantity(spaceAvailable);
                        UpdateSlotUI(i);
                        quantity -= spaceAvailable; 
                        // ���������� ����� ��� �������
                    }
                }
                if (quantity <= 0)
                {
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
        }

        // 2. ����� ������� �����
        int firstEmptySlot = FindFirstEmptySlot();
        if (firstEmptySlot != -1)
        {
            inventoryItems[firstEmptySlot] = new InventoryItem(itemToAdd, quantity);
            UpdateSlotUI(firstEmptySlot);
            OnInventoryChanged?.Invoke();
            return true; 
        }

        Debug.Log("Inventory is full!");
        return false;
    }

    private int FindFirstEmptySlot()
    {
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (IsSlotActive(i) && (inventoryItems[i] == null || inventoryItems[i].IsEmpty))
            {
                return i;
            }
        }
        return -1;
    }

    public void RemoveItem(int index, int quantity = 1)
    {
        if (index < 0 || index >= inventoryItems.Count || inventoryItems[index] == null || inventoryItems[index].IsEmpty)
        {
            return; 
        }

        inventoryItems[index].RemoveQuantity(quantity);

        if (inventoryItems[index].quantity <= 0)
        {
            inventoryItems[index] = null;
        }

        UpdateSlotUI(index);
        OnInventoryChanged?.Invoke();
    }

    public InventoryItem GetItemInSlot(int index)
    {
        if (index < 0 || index >= inventoryItems.Count) return null;
        return inventoryItems[index];
    }

    public InventoryItem GetSelectedItem()
    {
        return GetItemInSlot(selectedSlotIndex);
    }


    #endregion

    #region UI Update & Selection

    public void UpdateAllSlotsUI()
    {
        // ������
        for (int i = 0; i < hotbarSize; i++)
        {
            UpdateSlotUI(i);
        }
        for (int i = 0; i < currentMainInventoryRows * columns; i++)
        {
            int actualIndex = hotbarSize + i;
            UpdateSlotUI(actualIndex);
        }
    }

    private void UpdateSlotUI(int index)
    {
        InventorySlotUI slotUI = GetSlotUIByIndex(index);
        if (slotUI != null)
        {
            slotUI.UpdateSlot(inventoryItems[index]);
        }
    }

    private InventorySlotUI GetSlotUIByIndex(int index)
    {
        if (index >= 0 && index < hotbarSize)
        {
            return hotbarSlotsUI[index];
        }
        else if (index >= hotbarSize && index < hotbarSize + mainInventorySlotsUI.Count)
        {
            return mainInventorySlotsUI[index - hotbarSize];
        }
        return null;
    }


    public void SelectSlot(int index)
    {
        if (index < 0 || index >= hotbarSize) return;

        // ����� ��������� � ����������� �����
        InventorySlotUI previousSlotUI = GetSlotUIByIndex(selectedSlotIndex);
        if (previousSlotUI != null)
        {
            previousSlotUI.SetHighlight(false);
        }

        // �������� ����� ����
        selectedSlotIndex = index;
        InventorySlotUI currentSlotUI = GetSlotUIByIndex(selectedSlotIndex);
        if (currentSlotUI != null)
        {
            currentSlotUI.SetHighlight(true);
        }

        Debug.Log($"Selected hotbar slot: {selectedSlotIndex}");
        OnSelectedSlotChanged?.Invoke(selectedSlotIndex); 


        // ��������� ��������� ������ ��� ������� (�������� �� ������ ��� ����)

        //InventoryItem selected = GetSelectedItem();
        //if(selected.itemData.itemType == ItemType.Seed)
        //{
        //    BedsManagerScript bedsManagerScript = BedManager.GetComponent<BedsManagerScript>();
        //    if (bedsManagerScript != null) { 
        //        bedsManagerScript.CheckFreeSlots();
        //    }
        //}
        //else
        //{
        //    BedsManagerScript bedsManagerScript = BedManager.GetComponent<BedsManagerScript>();
        //    if (bedsManagerScript != null)
        //    {
        //        bedsManagerScript.UnCheckFreeSlots();
        //    }
        //}

    }

    #endregion

    #region Main Inventory Expansion

    public bool IsMainInventoryPanelActive()
    {
        // ��������� �������� �� null �� ������, ���� ������ �� ���������
        return mainInventoryPanel != null && mainInventoryPanel.activeSelf;
    }

    public void ToggleMainInventory()
    {
        bool isActive = !mainInventoryPanel.activeSelf;
        mainInventoryPanel.SetActive(isActive);

        if (inventoryBackgroundPanel != null) // ���������, ��������� �� ������ �� ���
        {
            inventoryBackgroundPanel.SetActive(isActive); // ������������� �� �� ��������� ����������
        }
        else
        {
            Debug.LogWarning("Inventory Background Panel is not assigned in the Inspector. Background visibility will not be toggled.");
        }

        if (isActive)
        {
            UpdateMainInventoryUIVisibility();
        }
    }

    // ������������� ���������� ������� ����� ��������� ���������
    public void SetMainInventorySize(int numberOfRows)
    {
        currentMainInventoryRows = Mathf.Clamp(numberOfRows, 1, maxMainInventoryRows);
        Debug.Log($"Main inventory size set to {currentMainInventoryRows} rows.");
        UpdateMainInventoryUIVisibility();
        ClearInactiveSlots(); // ������� �������� � ���������� ������
        OnInventoryChanged?.Invoke(); 
    }

    private void UpdateMainInventoryUIVisibility()
    {
        if (!mainInventoryPanel.activeSelf)
        {
            return;
        }

        int totalVisibleMainSlots = currentMainInventoryRows * columns;
        for (int i = 0; i < mainInventorySlotsUI.Count; i++)
        {
            bool shouldBeActive = i < totalVisibleMainSlots;
            if (mainInventorySlotsUI[i].gameObject.activeSelf != shouldBeActive)
            {
                mainInventorySlotsUI[i].gameObject.SetActive(shouldBeActive);
            }
            if (shouldBeActive)
            {
                UpdateSlotUI(hotbarSize + i);
            }
        }

    }

    // ���������, ������� �� ���� (� ������� ��� � ������� ����� ������)
    private bool IsSlotActive(int index)
    {
        if (index >= 0 && index < hotbarSize)
        {
            return true; // ����� ������� ������ �������
        }
        else if (index >= hotbarSize && index < hotbarSize + (currentMainInventoryRows * columns))
        {
            return true; // ���� � ������� ����� ��������� ���������
        }
        return false; // ���� �� ��������� ������� �����
    }

    // ������� ������ � ������, ������� ����� �����������
    private void ClearInactiveSlots()
    {
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (!IsSlotActive(i) && inventoryItems[i] != null && !inventoryItems[i].IsEmpty)
            {
                Debug.LogWarning($"Clearing item '{inventoryItems[i].itemData.itemName}' from inactive slot {i}");
                inventoryItems[i] = null;
            }
        }
    }


    #endregion

    #region Drag & Drop Logic

    // ����� ��� �����������/������ ��������� ����� �������
    public void MoveItem(int fromIndex, int toIndex)
    {
        // �������� �� ������������ ��������
        if (fromIndex < 0 || fromIndex >= inventoryItems.Count ||
            toIndex < 0 || toIndex >= inventoryItems.Count ||
            fromIndex == toIndex) // ������ ����������� � ��� �� ����
        {
            Debug.LogWarning($"Invalid move operation: from {fromIndex} to {toIndex}");
            // ����� �������� UI ��������� �����, ��� ��� OnDrop ��� �� ������� ������
            UpdateSlotUI(fromIndex);
            return;
        }

        // ���������, ������� �� ��� �����
        if (!IsSlotActive(fromIndex) || !IsSlotActive(toIndex))
        {
            Debug.LogWarning($"Cannot move item: one or both slots are inactive (from: {IsSlotActive(fromIndex)}, to: {IsSlotActive(toIndex)})");
            UpdateSlotUI(fromIndex); // ������������ ��� ��������� �����
            return;
        }

        // �������� �������� �� ������
        InventoryItem itemFrom = inventoryItems[fromIndex];
        InventoryItem itemTo = inventoryItems[toIndex];

        Debug.Log($"Attempting to move item from {fromIndex} ({itemFrom?.itemData?.itemName ?? "Empty"}) to {toIndex} ({itemTo?.itemData?.itemName ?? "Empty"})");

        // --- ������ ������/������� ---

        // ������ 1: ��� ����� �������� ���������� ����������� ��������
        if (itemFrom != null && !itemFrom.IsEmpty && itemTo != null && !itemTo.IsEmpty &&
            itemFrom.itemData == itemTo.itemData && itemFrom.itemData.isStackable)
        {
            // �������� ����� �����
            int spaceInTarget = itemTo.itemData.maxStackSize - itemTo.quantity;
            if (spaceInTarget > 0)
            {
                int amountToMove = Mathf.Min(itemFrom.quantity, spaceInTarget);
                itemTo.AddQuantity(amountToMove);
                itemFrom.RemoveQuantity(amountToMove);

                // ���� �������� ���� �������, ������� ���
                if (itemFrom.quantity <= 0)
                {
                    inventoryItems[fromIndex] = null;
                }
                Debug.Log($"Merged stacks: Moved {amountToMove} of {itemFrom.itemData.itemName}. From Qty: {itemFrom.quantity}, To Qty: {itemTo.quantity}");
            }
            else
            {
                Debug.Log("Merge failed: Target stack is full. Swapping instead.");
                // ���� ����� ���, ������ ������ �������
                inventoryItems[toIndex] = itemFrom;
                inventoryItems[fromIndex] = itemTo; // itemTo �� ����� ���� null �����
            }
        }
        else // ������ 2: ������� ����� (��� ����������� � ������ ����)
        {
            inventoryItems[toIndex] = itemFrom;
            inventoryItems[fromIndex] = itemTo; // itemTo ����� ���� null (���� toIndex ��� ����)
            Debug.Log("Swapped items between slots.");
        }

        // ��������� UI ����� ���������� ������
        UpdateSlotUI(fromIndex);
        UpdateSlotUI(toIndex);

        // �������� ������� ��������� ���������
        OnInventoryChanged?.Invoke();
    }

    #endregion // Drag & Drop Logic

}