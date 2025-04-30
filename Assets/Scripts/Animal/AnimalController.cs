using UnityEngine;
using System.Collections;

public class AnimalController : MonoBehaviour
{
    
    [Header("Data & Links")]
    public AnimalData animalData; 
    public GameObject thoughtBubblePrefab; // ������ ������

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.0f; // �������� ������������
    [SerializeField] private float minIdleTime = 2.0f; // ���. ����� ������� �� �����
    [SerializeField] private float maxIdleTime = 5.0f; // ����. ����� ������� �� �����
    [SerializeField] private float minWalkTime = 3.0f; // ���. ����� ������
    [SerializeField] private float maxWalkTime = 6.0f; // ����. ����� ������
    [SerializeField] private Vector3 thoughtBubbleOffset = new Vector3(1.4f, 0.9f, 0);

    // --- ��������� ��������� ---
    private enum AnimalState { Idle, Walking, NeedsAttention }
    private AnimalState currentState = AnimalState.Idle;

    // --- ������� �������� ---
    private Bounds movementBounds; // ������� ���� AnimalPlacementArea
    private bool boundsInitialized = false; // ����, ��� ������� �����������

    // --- ������� ---
    private float feedTimer;
    private float productionTimer;
    private float fertilizerTimer;
    private float stateChangeTimer; // ������ ��� ����� ��������� Idle/Walking

    // --- ����� ������������ ---
    private bool needsFeeding = false;
    private bool hasProductReady = false;
    private bool hasFertilizerReady = false;
    private ItemData currentNeedIcon = null; // ����� ������� ���������� � �������

    // --- ������ --- 
    private Transform myTransform; // �������� transform ��� ������������������
    private ThoughtBubbleController activeThoughtBubble; // ������ �� �������� �������
    private InventoryManager inventoryManager; // ������ �� �������� ���������
    private SpriteRenderer spriteRenderer; // ��� ���������� �������� ������� ��� ������

    // --- ������������ ---
    private Vector2 currentTargetPosition;
    private bool isMoving = false;

    void Awake() 
    {
        myTransform = transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        inventoryManager = InventoryManager.Instance; 
        if (inventoryManager == null)
        {
            Debug.LogError($"InventoryManager �� ������! Awake() � AnimalController");
        }
    }

    void Start()
    {
        if (animalData == null)
        {
            Debug.LogError($"AnimalData �� ��������� ��� {gameObject.name}! �������� �� ����� ��������.", gameObject);
            enabled = false; 
            return;
        }
        if (thoughtBubblePrefab == null)
        {
            Debug.LogError($"ThoughtBubblePrefab �� �������� ��� {gameObject.name}! �� ������ �������� �����������.", gameObject);
        }
        if (inventoryManager == null)
        {
            Debug.LogError($"InventoryManager �� ������ �� �����! ���� ��������� �� ����� ��������.", gameObject);
        }

        // ��������� ��������� �������
        ResetFeedTimer();
        ResetProductionTimer();
        ResetFertilizerTimer();

        currentState = AnimalState.Idle;
        SetNewStateTimer(AnimalState.Idle);
        UpdateAppearance();
    }


    // ���� ����� ������ ���������� �����
    public void InitializeMovementBounds(Bounds bounds)
    {
        if (myTransform == null)
        {
            Debug.LogError($"������: myTransform ��� ��� null � InitializeMovementBounds! ��������� Awake() � {gameObject.name}", gameObject);
            myTransform = transform;
        }

        movementBounds = bounds;
        boundsInitialized = true;
        Debug.Log($"{animalData.speciesName} ({gameObject.name}) ������� ������� ��������: {movementBounds}");

        myTransform.position = GetRandomPositionInBounds();
        PickNewWanderTarget();

        if (boundsInitialized) // ���. ��������
        {
            StartCoroutine(StateMachineCoroutine());
            Debug.Log($"StateMachineCoroutine ������� ��� {animalData.speciesName} ({gameObject.name})");
        }
        else
        {
            Debug.LogError($"�� ������� ��������� StateMachineCoroutine ��� {animalData.speciesName}, �.�. ������� �� ����������������!", gameObject);
        }
    }


    void Update()
    {
        // ���� ������� �� �����������, ������ �� ������
        if (!boundsInitialized || animalData == null) return;

        // ��������� �������, ������ ���� �������� �� ���� �������� ������
        if (currentState != AnimalState.NeedsAttention)
        {
            UpdateTimers(Time.deltaTime);
            CheckNeeds(); // ���������, �� ���� �� ���-�� ���������
        }

        // ���������� ������� �������, ���� ��� �������
        if (activeThoughtBubble != null && activeThoughtBubble.gameObject.activeSelf)
        {
            // ��������� ��� ������� ������ ����, ����� ��� ���������� ��� ������� ����������� ���������
            activeThoughtBubble.transform.position = myTransform.position + thoughtBubbleOffset;
        }

        // ������ �������� �������
        if (isMoving && spriteRenderer != null)
        {
            // ���������� ������� ������� � �������
            float horizontalDifference = currentTargetPosition.x - myTransform.position.x;

            if (Mathf.Abs(horizontalDifference) > 0.01f) 
            {
                spriteRenderer.flipX = (horizontalDifference > 0);
            }
        }
    }

    // �������� ��� ���������� ����������� Idle/Walking
    // �������� ��� ���������� ����������� Idle/Walking
    private IEnumerator StateMachineCoroutine()
    {
        Debug.Log($"StateMachineCoroutine ����� ������ ��� {gameObject.name}. ��������� ���������: {currentState}");

        while (boundsInitialized) 
        {
            if (currentState == AnimalState.NeedsAttention)
            {
                isMoving = false;
                yield return null; // ����� 1 ����
                continue; // ��������� � ������ ����� while � ��������� ��������� �����
            }

            // Idle ��� Walking

            yield return new WaitForSeconds(stateChangeTimer); // ���� ���� ������� ����� �������� ���������

            // ����� ��������, ��������� �����, �� ���������� �� ��������� �� NeedsAttention ���� �� �����
            if (currentState == AnimalState.NeedsAttention)
            {
                // ���� �� ����� �������� ��������� �����������, ��������� ����� ���������
                Debug.Log($"{gameObject.name}: ��������� ���������� �� NeedsAttention �� ����� ��������. ��������� �����.");
                isMoving = false;
                yield return null; // ����� 1 ����
                continue; // ��������� � ������ ����� while
            }

            // ���� ��������� ��� ��� Idle ��� Walking, ���������� �����
            Debug.Log($"{gameObject.name}: ����� �������� ��� {currentState} �������. ������ ���������.");
            if (currentState == AnimalState.Idle)
            {
                currentState = AnimalState.Walking;
                PickNewWanderTarget(); // �������� ����� ����� ��� ������
                SetNewStateTimer(AnimalState.Walking); // ������������� ����� ������
                isMoving = true;
                Debug.Log($"{gameObject.name}: ������� � ��������� Walking. ����� ����: {currentTargetPosition}, �����: {stateChangeTimer} ���.");
            }
            else // ���� � ��������� Walking
            {
                currentState = AnimalState.Idle;
                SetNewStateTimer(AnimalState.Idle); // ������������� ����� ������
                isMoving = false;
                Debug.Log($"{gameObject.name}: ������� � ��������� Idle. �����: {stateChangeTimer} ���.");
            }
        }
        Debug.LogWarning($"StateMachineCoroutine �������� ������ ��� {gameObject.name} (boundsInitialized ���� false?)");
    }


    void FixedUpdate()
    {
        if (currentState == AnimalState.Walking && isMoving && boundsInitialized)
        {
            MoveTowardsTarget();
        }
    }

    private void UpdateTimers(float deltaTime)
    {
        // ��������� �������, ���� ��� ������ ����
        if (feedTimer > 0) feedTimer -= deltaTime;
        if (productionTimer > 0) productionTimer -= deltaTime;
        if (fertilizerTimer > 0) fertilizerTimer -= deltaTime;
    }

    private void CheckNeeds()
    {
        bool needsAttentionNow = false;
        ItemData nextNeedIcon = null;
        bool didProductBecomeReady = false;

        if (!needsFeeding && feedTimer <= 0)
        {
            Debug.Log($"[CheckNeeds] ���������� �����������: ��� ({animalData.requiredFood.itemName})"); 
            needsFeeding = true;
            nextNeedIcon = animalData.requiredFood;
            needsAttentionNow = true;
        }

        else if (!hasProductReady && productionTimer <= 0)
        {
            Debug.Log($"[CheckNeeds] ���������� �����������: ������� ({animalData.productProduced.itemName})");
            hasProductReady = true;
            didProductBecomeReady = true;
            nextNeedIcon = animalData.productProduced;
            needsAttentionNow = true;
        }

        else if (!hasFertilizerReady && fertilizerTimer <= 0)
        {
            Debug.Log($"[CheckNeeds] ���������� �����������: ��������� ({animalData.fertilizerProduced.itemName})");
            hasFertilizerReady = true;
            nextNeedIcon = animalData.fertilizerProduced;
            needsAttentionNow = true;
        }

        if (didProductBecomeReady)
        {
            UpdateAppearance(); // <--- ��������� ��� ��� ���������� ��������
        }

        // ���� ���-�� ���������, ��������� � ��������� NeedsAttention
        if (needsAttentionNow)
        {
            if (currentState != AnimalState.NeedsAttention || currentNeedIcon != nextNeedIcon)
            {
                currentState = AnimalState.NeedsAttention;
                currentNeedIcon = nextNeedIcon;
                isMoving = false;
                ShowThoughtBubble(currentNeedIcon);
            }
        }
        else if (currentState == AnimalState.NeedsAttention)
        {
            // ���� ��� �������� ������������ (needsAttentionNow == false),
            // � �� ��� ��� � ��������� NeedsAttention, ������, ����������� ���� ������ ��� �������������
            // ��� �������� ������. ���������� � Idle.
            Debug.LogWarning($"[CheckNeeds] ������������ �� �������, �� ��������� ���� NeedsAttention. ���������� � Idle.");
            HideThoughtBubble();
            currentState = AnimalState.Idle;
            SetNewStateTimer(currentState);
        }
    }

    private void ResetFeedTimer()
    {
        feedTimer = animalData.feedInterval;
        needsFeeding = false;
    }

    private void ResetProductionTimer()
    {
        productionTimer = animalData.productionInterval;
        hasProductReady = false;
        UpdateAppearance();
    }

    private void ResetFertilizerTimer()
    {
        fertilizerTimer = animalData.fertilizerInterval;
        hasFertilizerReady = false;
    }

    private void SetNewStateTimer(AnimalState forState)
    {
        if (forState == AnimalState.Idle)
        {
            stateChangeTimer = Random.Range(minIdleTime, maxIdleTime);
        }
        else // Walking
        {
            stateChangeTimer = Random.Range(minWalkTime, maxWalkTime);
        }
    }

    private void UpdateAppearance()
    {
        if (spriteRenderer == null || animalData == null) return; // ������������

        // ���� ������� ����� � ���� ������ ��� ����� ���������
        if (hasProductReady && animalData.productReadySprite != null)
        {
            spriteRenderer.sprite = animalData.productReadySprite;
            // Debug.Log($"{animalData.speciesName} ���������� ������ 'productReadySprite'");
        }
        // �����, ���� ���� ������ �� ��������� (���������� ���)
        else if (animalData.defaultSprite != null)
        {
            spriteRenderer.sprite = animalData.defaultSprite;
            // Debug.Log($"{animalData.speciesName} ���������� ������ 'defaultSprite'");
        }
        // ���� ��� �� ����, �� ������� (��� ������� �� �����, �� ��� ������� �� ���������)
        else
        {
            // ��������� ������� ������ ��� �������� ��������������, ���� ������� �� ��������� � AnimalData
            if (hasProductReady && animalData.productReadySprite == null && animalData.defaultSprite == null)
                Debug.LogWarning($"� {animalData.speciesName} ����� �������, �� �� ��������� 'productReadySprite' � 'defaultSprite' � AnimalData!", gameObject);
            else if (!hasProductReady && animalData.defaultSprite == null)
                Debug.LogWarning($"� {animalData.speciesName} ��� ��������, �� �� �������� 'defaultSprite' � AnimalData!", gameObject);
        }
    }


    //=========================================================================
    // ������ ������������
    //=========================================================================

    private void PickNewWanderTarget()
    {
        currentTargetPosition = GetRandomPositionInBounds();
    }

    private Vector2 GetRandomPositionInBounds()
    {
        if (!boundsInitialized) return myTransform.position; // ������������

        float randomX = Random.Range(movementBounds.min.x, movementBounds.max.x);
        float randomY = Random.Range(movementBounds.min.y, movementBounds.max.y);

        return new Vector2(randomX, randomY);
    }

    private void MoveTowardsTarget()
    {
        if (!isMoving) return;

        Vector2 currentPosition = myTransform.position;
        Vector2 direction = (currentTargetPosition - currentPosition).normalized;
        Vector2 newPosition = Vector2.MoveTowards(currentPosition, currentTargetPosition, moveSpeed * Time.fixedDeltaTime);

        newPosition.x = Mathf.Clamp(newPosition.x, movementBounds.min.x, movementBounds.max.x);
        newPosition.y = Mathf.Clamp(newPosition.y, movementBounds.min.y, movementBounds.max.y);


        myTransform.position = newPosition;


        if (Vector2.Distance(currentPosition, currentTargetPosition) < 0.1f)
        {
            isMoving = false; 
        }
    }

    //=========================================================================
    // ���������� ���������� (�������)
    //=========================================================================

    private void ShowThoughtBubble(ItemData itemToShow)
    {
        Debug.Log($"[ShowThoughtBubble] ������. �������� ��������: {itemToShow?.itemName ?? "NULL"}"); // ��� 5

        if (thoughtBubblePrefab == null)
        {
            Debug.LogError("��� ������� �������!");
            return;
        }

        // ���� ������� ��� �� �������, ������� ���
        if (activeThoughtBubble == null)
        {
            GameObject bubbleInstance = Instantiate(thoughtBubblePrefab, myTransform.position + thoughtBubbleOffset, Quaternion.identity, myTransform); // ������ �������� � ���������
            activeThoughtBubble = bubbleInstance.GetComponent<ThoughtBubbleController>();
            if (activeThoughtBubble == null)
            {
                Debug.LogError("������ ������� �� �������� ������ ThoughtBubbleController!");
                Destroy(bubbleInstance);
                return;
            }
            BubbleYSorter bubbleSorter = bubbleInstance.GetComponent<BubbleYSorter>();
            if (bubbleSorter != null)
            {
                bubbleSorter.SetOwner(myTransform); // �������� transform ����� ���������
            }
            else
            {
                Debug.LogError($"�� ������� ������� {thoughtBubblePrefab.name} ����������� ������ BubbleYSorter!", bubbleInstance);
            }
        }

        // ����������� � ���������� �������
        if (itemToShow != null && itemToShow.itemIcon != null)
        {
            Debug.Log($"[ShowThoughtBubble] ������ ��� {itemToShow.itemName} �������. �������� activeThoughtBubble.Show()."); // ��� 6
            activeThoughtBubble.Show(itemToShow.itemIcon);
        }
        else
        {
            Debug.LogWarning($"������� �������� ������� ��� {animalData.speciesName}, �� � �������� {itemToShow?.itemName} ��� ������!");
            activeThoughtBubble.Hide(); // ��������, ���� ������ ���
        }
    }

    private void HideThoughtBubble()
    {
        if (activeThoughtBubble != null)
        {
            activeThoughtBubble.Hide();
        }
        currentNeedIcon = null; 
    }


    public string GetCurrentStateName()
    {
        return currentState.ToString(); // ���������� ��� ��������� ("Idle", "Walking", "NeedsAttention")
    }


    public void AttemptInteraction()
    {
        if (inventoryManager == null)
        {
            Debug.LogError("��� ������ �� InventoryManager!");
            return;
        }

        bool interactionSuccessful = false;
        ItemData itemInvolved = null; 

        // 1. ���������, ����� �� �������
        if (needsFeeding)
        {
            Debug.Log($"������� ��������������: {animalData.speciesName} ��������� � {animalData.requiredFood.itemName}");
            itemInvolved = animalData.requiredFood;

            // ���������, ��� � ��� ���� ������ �� InventoryManager
            if (inventoryManager == null)
            {
                Debug.LogError($"�� ���� ��������� ��������� ��� ��������� {animalData.speciesName} - ������ �� InventoryManager �����������!");
                interactionSuccessful = false; // �� ����� ����������
            }
            else
            {
                // �������� ��������� ������� � ������ ���������� �����
                InventoryItem selectedItem = inventoryManager.GetSelectedItem();
                int selectedIndex = inventoryManager.SelectedSlotIndex; // ���������� ����� ��������

                // ���������, ���� �� ��������� ������� � ��������� �� �� � ������ ����
                if (selectedItem != null && !selectedItem.IsEmpty && selectedItem.itemData == animalData.requiredFood)
                {
                    // ���, ����� ������ ������ ���!
                    Debug.Log($"����� ������ {selectedItem.itemData.itemName} � ����� {selectedIndex}. �������� ������� 1 ��.");

                    // �������� ������� 1 ������� �������� �� ���������� �����
                    inventoryManager.RemoveItem(selectedIndex, 1);

                    // ���������� ������ ��������� � ���� �����������
                    ResetFeedTimer();
                    needsFeeding = false;
                    interactionSuccessful = true; // ��������� �������!
                    Debug.Log($"������� ��������� {animalData.speciesName}");
                }
                else
                {
                    // ���������� ������� ������� ��� ����� �������������� ����
                    string reason = "����������� �������";
                    if (selectedItem == null || selectedItem.IsEmpty)
                    {
                        reason = "� ��������� ����� (������) ��� ��������";
                    }
                    else if (selectedItem.itemData != animalData.requiredFood)
                    {
                        reason = $"������ �������� ������� ({selectedItem.itemData.itemName}), ����� {animalData.requiredFood.itemName}";
                    }
                    Debug.Log($"�� ������� ��������� {animalData.speciesName}: {reason}.");
                    interactionSuccessful = false; // ��������� �� �������
                }
            }
        }
        // 2. �������� ����� �������� (��������!)
        else if (hasProductReady)
        {
            itemInvolved = animalData.productProduced; // �������� ������� ��� ����
            Debug.Log($"������� ������� {itemInvolved.itemName} c {animalData.speciesName}");

            // -------- �������� ����������� --------
            bool toolCheckPassed = true; // �� ��������� �������, ��� ���������� �� ����� ��� ����
            if (animalData.requiredToolForHarvest != null) // ��������� �� ����������?
            {
                Debug.Log($"��� ����� {itemInvolved.itemName} ��������� {animalData.requiredToolForHarvest.itemName}");
                InventoryItem selectedItem = inventoryManager.GetSelectedItem();

                if (selectedItem == null || selectedItem.IsEmpty || selectedItem.itemData != animalData.requiredToolForHarvest)
                {
                    // ���������� �� ������ ��� �� ���
                    Debug.Log($"�� ������� �������: ����� �� ������ {animalData.requiredToolForHarvest.itemName}. �������: {selectedItem?.itemData?.itemName ?? "������"}");
                    toolCheckPassed = false;
                    // ����� �������� ����� "���������" ������, ��� ����� ����������
                    // ��������, ����� UI ��� ������ ������� �����
                }
                else
                {
                    Debug.Log($"����� ������ ������ ����������: {selectedItem.itemData.itemName}");
                    // ���������� ����������, toolCheckPassed �������� true
                }
            }
            // ---------------------------------------

            // ���� �������� ����������� �������� (��� �� �� ����������)
            if (toolCheckPassed)
            {
                // �������� �������� ������� � ���������
                bool added = inventoryManager.AddItem(animalData.productProduced, animalData.productAmount);

                if (added)
                {
                    Debug.Log($"������� ������� {animalData.productAmount} {animalData.productProduced.itemName}");
                    // ���������� ���� �� ������ �������, ����� UpdateAppearance �������� ���������
                    hasProductReady = false;
                    ResetProductionTimer(); // ���� ����� ������� UpdateAppearance()
                    interactionSuccessful = true;
                }
                else
                {
                    Debug.Log("�� ������� ������� ������� - ��������� �����!");
                    // ��� �� ������, ����������� ��������
                }
            }
            // ���� ���������� �� ���, interactionSuccessful �������� false
        }

        // 3. ���������, ������ �� ��������� (���� �� ������� � �� �������� �������)
        else if (hasFertilizerReady)
        {
            Debug.Log($"������� ������� {animalData.fertilizerProduced.itemName} c {animalData.speciesName}");
            bool added = inventoryManager.AddItem(animalData.fertilizerProduced, animalData.fertilizerAmount);

            if (added)
            {
                Debug.Log($"������� ������� {animalData.fertilizerAmount} {animalData.fertilizerProduced.itemName}");
                ResetFertilizerTimer();
                hasFertilizerReady = false; // ���������� ����
                interactionSuccessful = true;
            }
            else
            {
                Debug.Log("�� ������� ������� ��������� - ��������� �����!");
            }
        }

        // ���� �������������� ���� ��������, ���������, ���� �� ��� �����������
        // ���� �������������� ���� ��������
        if (interactionSuccessful)
        {
            Debug.Log($"�������������� � {animalData.speciesName} (�������: {itemInvolved?.itemName}) ���� ��������.");
            // �������� ������� �������
            HideThoughtBubble();

            // ����� ���������, ��� �� ��������� ����������� �� �������
            Debug.Log("��������� ����� ����� ����� ��������� ��������������...");
            CheckNeeds(); // ���� ����� ��������� currentState ���� � NeedsAttention, ���� � Idle (�������� ��������� 1)

            Debug.Log($"��������� ����� CheckNeeds: {currentState}. ������������� StateMachineCoroutine.");

            // ������������� �������� ������ ����� ������
            // ��� ������ ������ � ��� ����������, ������� ��������� CheckNeeds
            StopAllCoroutines();
            StartCoroutine(StateMachineCoroutine());

            Debug.Log($"{animalData.speciesName} ��������� �������� �������������� � ������������ StateMachine.");
        }
        else // ���� interactionSuccessful == false
        {
            // ������� �������� ��������� ��� �����������
            Debug.Log($"�������������� � {animalData.speciesName} (�������: {itemInvolved?.itemName ?? "NULL"}) ���� ����������. ������� ���������: {currentState}. �����: Feed={needsFeeding}, Prod={hasProductReady}, Fert={hasFertilizerReady}");
        }
    }

}