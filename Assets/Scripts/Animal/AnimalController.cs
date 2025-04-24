using UnityEngine;
using System.Collections; // ����� ��� ������� (��������)

public class AnimalController : MonoBehaviour
{
    
    [Header("Data & Links")]
    public AnimalData animalData; // ���� ����� ��������� ScriptableObject � ������� ������
    public GameObject thoughtBubblePrefab; // ������ "������� ������" (�������� ��� �����)

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.0f; // �������� ������������
    [SerializeField] private float minIdleTime = 2.0f; // ���. ����� ������� �� �����
    [SerializeField] private float maxIdleTime = 5.0f; // ����. ����� ������� �� �����
    [SerializeField] private float minWalkTime = 3.0f; // ���. ����� ������
    [SerializeField] private float maxWalkTime = 6.0f; // ����. ����� ������
    [SerializeField] private Vector3 thoughtBubbleOffset = new Vector3(0, 1.2f, 0); // �������� ������� ��� ��������

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

    //=========================================================================
    // �������������
    //=========================================================================

    void Awake() // ���������� Awake ������ Start ��� ������������� ������
    {
        myTransform = transform; // �������������� �����! ������ ��� ���������� �� InitializeMovementBounds
        spriteRenderer = GetComponent<SpriteRenderer>(); // � ������ GetComponent ����� ������ �����
        inventoryManager = InventoryManager.Instance; // � ����� ��������� ����
        if (inventoryManager == null)
        {
            // �������������� ����� �������� � � Start, �� ����� ����� ������� �����
            Debug.LogError($"InventoryManager �� ������! Awake() � AnimalController.");
        }
    }

    void Start()
    {
        // myTransform ��� ��������������� � Awake()

        if (animalData == null)
        {
            Debug.LogError($"AnimalData �� ��������� ��� {gameObject.name}! �������� �� ����� ��������.", gameObject);
            enabled = false; // ��������� ������
            return;
        }
        if (thoughtBubblePrefab == null)
        {
            Debug.LogError($"ThoughtBubblePrefab �� �������� ��� {gameObject.name}! �� ������ �������� �����������.", gameObject);
            // ����� �� ���������
        }
        if (inventoryManager == null) // ��������� ��������, ���� Awake �� �����
        {
            Debug.LogError($"InventoryManager �� ������ �� �����! ���� ��������� �� ����� ��������.", gameObject);
            // ����� �� ���������
        }

        // ��������� ��������� �������
        ResetFeedTimer();
        ResetProductionTimer();
        ResetFertilizerTimer();

        // ������������� ��������� ��������� � ������ ��� ����
        currentState = AnimalState.Idle;
        SetNewStateTimer(AnimalState.Idle);

        // �����: ������� � ������ ��������� ����� � InitializeMovementBounds
        // �� �������� StartCoroutine �����!
    }


    // ���� ����� ������ ���������� ����� (��������, �� ��������) ����� �������� ���������
    public void InitializeMovementBounds(Bounds bounds)
    {
        // ������� �������� �� ������ ������, ���� Awake ������ ��� ���������
        if (myTransform == null)
        {
            Debug.LogError($"������: myTransform ��� ��� null � InitializeMovementBounds! ��������� Awake() � {gameObject.name}", gameObject);
            myTransform = transform; // ������� ���������������� ��������
        }

        movementBounds = bounds;
        boundsInitialized = true;
        Debug.Log($"{animalData.speciesName} ({gameObject.name}) ������� ������� ��������: {movementBounds}");

        // ������������� ��������� ������� ������ ������ (�� ������ ������)
        // ������ myTransform ������ ���� �� null
        myTransform.position = GetRandomPositionInBounds(); // ������ ~101 - ������ ������ ��������

        // ������ ����� ������� ������ ���� ��� ��������, ���� �����
        PickNewWanderTarget();

        // ��������� �������� ��������� �����, ����� ������������� ������!
        if (boundsInitialized) // ���. ��������, ��� ��� ������
        {
            StartCoroutine(StateMachineCoroutine());
            Debug.Log($"StateMachineCoroutine ������� ��� {animalData.speciesName} ({gameObject.name})");
        }
        else
        {
            Debug.LogError($"�� ������� ��������� StateMachineCoroutine ��� {animalData.speciesName}, �.�. ������� �� ����������������!", gameObject);
        }
    }


    //=========================================================================
    // ���������� (Update) - ���������� ��������� � �����������
    //=========================================================================

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
            activeThoughtBubble.transform.position = myTransform.position + thoughtBubbleOffset;
        }

        // ������� ������ �������� ������� (�����������)
        if (isMoving && spriteRenderer != null)
        {
            // ������������ ������ �����/������ � ����������� �� ����������� � ����
            spriteRenderer.flipX = (currentTargetPosition.x < myTransform.position.x);
        }
    }

    // �������� ��� ���������� ����������� Idle/Walking
    // �������� ��� ���������� ����������� Idle/Walking
    private IEnumerator StateMachineCoroutine()
    {
        Debug.Log($"StateMachineCoroutine ����� ������ ��� {gameObject.name}. ��������� ���������: {currentState}"); // ������� ��� ��� ������

        while (boundsInitialized) // ��������, ���� ��� ������
        {
            // ----- ��������� ��������� � ������ ����� -----
            if (currentState == AnimalState.NeedsAttention)
            {
                // ���� ����� ��������, ������ ���� ���� ���� � ��������� �����.
                // �� ���� stateChangeTimer!
                // Debug.Log($"{gameObject.name}: � ��������� NeedsAttention, �������� 1 �����.");
                isMoving = false; // ��������, ��� �� ���������
                yield return null; // ����� 1 ����
                continue; // ��������� � ������ ����� while � ��������� ��������� �����
            }

            // ----- ���� ��������� �� NeedsAttention (�.�. Idle ��� Walking) -----

            // Debug.Log($"{gameObject.name}: � ��������� {currentState}. �������� {stateChangeTimer} ������.");
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
        // ������������ ����� ������ � FixedUpdate ��� ������������ ������ (���� � ��� �� ���)
        // ��� ������ � Update, ���� ��� Rigidbody
        if (currentState == AnimalState.Walking && isMoving && boundsInitialized)
        {
            MoveTowardsTarget();
        }
    }

    //=========================================================================
    // ������ �������� � ������������
    //=========================================================================

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
        ItemData nextNeedIcon = null; // ��������� ���������� ��� ����

        if (!needsFeeding && feedTimer <= 0)
        {
            Debug.Log($"[CheckNeeds] ���������� �����������: ��� ({animalData.requiredFood.itemName})"); // ��� 1
            needsFeeding = true;
            nextNeedIcon = animalData.requiredFood;
            needsAttentionNow = true;
        }
        // ���������� else if, ����� ��������� ������� ������ ���� �� �������
        else if (!hasProductReady && productionTimer <= 0)
        {
            Debug.Log($"[CheckNeeds] ���������� �����������: ������� ({animalData.productProduced.itemName})"); // ��� 2
            hasProductReady = true;
            nextNeedIcon = animalData.productProduced;
            needsAttentionNow = true;
        }
        // ���������� else if, ����� ��������� ��������� ������ ���� �� ������� � ��� ��������
        else if (!hasFertilizerReady && fertilizerTimer <= 0)
        {
            Debug.Log($"[CheckNeeds] ���������� �����������: ��������� ({animalData.fertilizerProduced.itemName})"); // ��� 3
            hasFertilizerReady = true;
            nextNeedIcon = animalData.fertilizerProduced;
            needsAttentionNow = true;
        }

        // ���� ���-�� ���������, ��������� � ��������� NeedsAttention
        if (needsAttentionNow)
        {
            // �������, ����� �� ������������� ������ ��������� (�����������)
            if (currentState != AnimalState.NeedsAttention || currentNeedIcon != nextNeedIcon)
            {
                // Debug.LogWarning($"[CheckNeeds] ������� ����������� ({nextNeedIcon?.itemName}). ������������� NeedsAttention.");
                currentState = AnimalState.NeedsAttention;
                currentNeedIcon = nextNeedIcon; // ����� �������� ������!
                isMoving = false;
                ShowThoughtBubble(currentNeedIcon);
            }
            // else { Debug.Log($"[CheckNeeds] ����������� ({nextNeedIcon?.itemName}) ��� �������. ��������� �� ������."); }
        }
        // ----- �����: ���� ������������ ���, � ��������� ���� NeedsAttention -----
        else if (currentState == AnimalState.NeedsAttention)
        {
            // ���� ��� �������� ������������ (needsAttentionNow == false),
            // � �� ��� ��� � ��������� NeedsAttention, ������, ����������� ���� ������ ��� �������������
            // ��� �������� ������. ���������� � Idle.
            Debug.LogWarning($"[CheckNeeds] ������������ �� �������, �� ��������� ���� NeedsAttention. ���������� � Idle.");
            HideThoughtBubble(); // �������� ������� �� ������ ������
            currentState = AnimalState.Idle;
            SetNewStateTimer(currentState);
            // �������� ������ ���� ���������� ����� ��������� Idle ��� ��������� ��������
            // �� ������������� �������� �����, ����� �������� ��������� ���������� � ������������ � AttemptInteraction
        }
        // else if (needsAttentionNow && nextNeedIcon == null) {
        //     Debug.LogError("[CheckNeeds] needsAttentionNow is true, but nextNeedIcon is null! ��� �� ������ �����������.");
        // }
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

    //=========================================================================
    // ������ ������������
    //=========================================================================

    private void PickNewWanderTarget()
    {
        currentTargetPosition = GetRandomPositionInBounds();
        // Debug.Log($"{animalData.speciesName} ���� � {currentTargetPosition}");
    }

    private Vector2 GetRandomPositionInBounds()
    {
        if (!boundsInitialized) return myTransform.position; // ������������

        // ���������� ��������� ����� ������ �������������� movementBounds
        float randomX = Random.Range(movementBounds.min.x, movementBounds.max.x);
        float randomY = Random.Range(movementBounds.min.y, movementBounds.max.y);

        // ���������� ��� Vector2 (��� Vector3 � ������ Z, ���� ����)
        return new Vector2(randomX, randomY);
    }

    private void MoveTowardsTarget()
    {
        if (!isMoving) return;

        // ������� ������ � ���� � �������� ���������
        Vector2 currentPosition = myTransform.position;
        Vector2 direction = (currentTargetPosition - currentPosition).normalized;
        Vector2 newPosition = Vector2.MoveTowards(currentPosition, currentTargetPosition, moveSpeed * Time.fixedDeltaTime);

        // �����: ���������, �� ������ �� ����� ��� �� �������
        // ���� GetRandomPositionInBounds �������� ����� ������, MoveTowards ����� ������� �� ���� �� ��������� ����
        // ������� ������ - ������ ������ ������� ������ ������
        newPosition.x = Mathf.Clamp(newPosition.x, movementBounds.min.x, movementBounds.max.x);
        newPosition.y = Mathf.Clamp(newPosition.y, movementBounds.min.y, movementBounds.max.y);


        myTransform.position = newPosition;


        // ���������, �������� �� �� ���� (� ��������� ������������)
        if (Vector2.Distance(currentPosition, currentTargetPosition) < 0.1f)
        {
            isMoving = false; // ��������������� �� ����������
                              // ��������� �������� �� ������� stateChangeTimer � ��������
                              // Debug.Log($"{animalData.speciesName} ������ ����.");
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
        currentNeedIcon = null; // ���������� ������� �����������
    }

    //=========================================================================
    // �������������� � �������
    //=========================================================================

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
        ItemData itemInvolved = null; // ��������, ����� ������� ����������

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
        // 2. ���������, ����� �� ������� (���� �� �������)
        else if (hasProductReady)
        {
            Debug.Log($"������� ������� {animalData.productProduced.itemName} c {animalData.speciesName}");
            // �������� �������� ������� � ���������
            bool added = inventoryManager.AddItem(animalData.productProduced, animalData.productAmount);

            if (added)
            {
                Debug.Log($"������� ������� {animalData.productAmount} {animalData.productProduced.itemName}");
                ResetProductionTimer();
                hasProductReady = false; // ���������� ����
                interactionSuccessful = true;
            }
            else
            {
                Debug.Log("�� ������� ������� ������� - ��������� �����!");
                // ������ �� ������, ������� ��������
            }
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