using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [System.Serializable]
    public struct ItemSpawnInfo
    {
        public ItemData itemData;
        public Vector3 position;
    }

    [Header("�������� ������")]
    [Tooltip("������, ������� ����� �������������� ��� ������ ��� ���� ����������� ���������.")]
    public GameObject worldItemPrefab;

    [Header("��������� �����")]
    [Tooltip("������ ��������� ��� ������ ��� ������ ����.")]
    public ItemSpawnInfo[] itemsToSpawnAtStart;

    [Header("��������� �� ���������")]
    [Tooltip("������� �� ���������, ���� �� ������ ������ ��� ������ SpawnItem.")]
    public Vector3 defaultSpawnScale = Vector3.one;

    [Header("������ �� �����������")]
    [Tooltip("������ �� ���������� ������/������ ��� ����������� ������������� ������.")]
    public TrainCameraController trainController;

    void Start()
    {
        if (trainController == null)
        {
            Debug.LogError("TrainCameraController �� �������� � ItemSpawner! ���������� ���������� ������������ ������.");
        }

        SpawnInitialItems();
    }

    void SpawnInitialItems()
    {
        // ��������� �������� � ��� worldItemPrefab �����, ���� �������� ������ � SpawnItem
        if (itemsToSpawnAtStart == null || itemsToSpawnAtStart.Length == 0) { return; }

        foreach (ItemSpawnInfo spawnInfo in itemsToSpawnAtStart)
        {
            // ���������, ����� �� worldItemPrefab ����� ������� ������ �������� ��������
            if (spawnInfo.itemData != null && spawnInfo.itemData.itemType != ItemType.Animal && worldItemPrefab == null)
            {
                Debug.LogError($"World Item Prefab �� �������� � ItemSpawner, �� �������� ���������� ������� {spawnInfo.itemData.itemName} ��� ������!");
                continue; // ���������� ����� ����� ��������
            }
            SpawnItem(spawnInfo.itemData, spawnInfo.position, defaultSpawnScale);
        }
    }

    public GameObject SpawnItem(ItemData dataToSpawn, Vector3 spawnPosition, Vector3 spawnScale)
    {
        if (dataToSpawn == null)
        {
            Debug.LogWarning("������� ���������� ������� � null ItemData.");
            return null;
        }

        if (dataToSpawn.itemType == ItemType.Animal && dataToSpawn.associatedAnimalData != null)
        {
            // --- ��� ��������! ---
            AnimalData animalData = dataToSpawn.associatedAnimalData;
            if (animalData.animalPrefab == null)
            {
                Debug.LogError($"� AnimalData '{animalData.speciesName}' �� �������� animalPrefab � ����������!");
                return null;
            }

            // 1. ������� ����� ������� ���������
            GameObject animalObject = Instantiate(animalData.animalPrefab, spawnPosition, Quaternion.identity);

            // 2. ������������� �������
            animalObject.transform.localScale = spawnScale;

            // 3. ----- ��������� �������� (������) � ��������� ��� TRANSFORM -----
            Transform parentWagon = null; // ���������� ��� �������� ������ �� �����
            bool parentAssignedSuccessfully = false; // ���� ��� ������������ ������

            if (trainController != null)
            {
                // �������� �����. �� ������ ���� ������������� ��������, ���� ������� �����.
                // ����� ���������� true, ���� ��������� ������ �������, ����� false.
                parentAssignedSuccessfully = trainController.AssignParentWagonByPosition(animalObject.transform, spawnPosition);

                if (parentAssignedSuccessfully)
                {
                    // ���� �������� ������� ��������, �������� ������ �� ���� �� �������
                    parentWagon = animalObject.transform.parent;

                    // �������������� ��������: ��������, ��� �������� ������������� �����������
                    if (parentWagon == null)
                    {
                        Debug.LogError($"AssignParentWagonByPosition ������ true, �� �������� � {animalObject.name} �� �����������! �������� ����������.", animalObject);
                        Destroy(animalObject);
                        return null;
                    }
                    // Debug.Log($"�������� {animalObject.name} ������� ��������� � ������ {parentWagon.name}");
                }
                else // ���� AssignParentWagonByPosition ������ false
                {
                    Debug.LogError($"�� ������� ����� ��� ��������� ������������ ����� ��� ��������� '{animalData.speciesName}' � ������� {spawnPosition}. �������� ����������.");
                    Destroy(animalObject);
                    return null;
                }
            }
            else
            {
                // trainController �� ��������, �� ��� ������ ������ � Start, �� ��������� ����� ��� ����������
                Debug.LogWarning("TrainController �� �������� � ItemSpawner. ���������� ���������� ����� ��� ���������. �������� ����������.");
                Destroy(animalObject);
                return null;
            }

            // 4. �������� ��������� AnimalController
            AnimalController animalController = animalObject.GetComponent<AnimalController>();
            if (animalController != null)
            {
                // 5. ��������� ������ ��������� (�� ������� ��� ������ ����, �� ��� ���������� ����� ��������)
                animalController.animalData = animalData;

                // 6. ----- ���� ������� � ������ � �������� �� ��������� -----
                // ���������� parentWagon, ������� �� �������� ����.
                // �������� parentWagon != null ����� �����, ���� ��� ������ ���� ��������, ���� �� ����� ����.
                if (parentWagon != null)
                {
                    string placementAreaName = animalData.speciesName.Replace(" ", "") + "PlacementArea";
                    Transform placementAreaTransform = parentWagon.Find(placementAreaName);
                    if (placementAreaTransform != null)
                    {
                        Collider2D boundsCollider = placementAreaTransform.GetComponent<Collider2D>();
                        if (boundsCollider != null)
                        {
                            // ���! ����� ���������, �������� ��� ������� � AnimalController
                            animalController.InitializeMovementBounds(boundsCollider.bounds);
                            Debug.Log($"������� {boundsCollider.bounds} �������� ��������� {animalData.speciesName} ({animalObject.name})");
                        }
                        else
                        {
                            Debug.LogError($"�� ������� 'AnimalPlacementArea' � ������ '{parentWagon.name}' ����������� ��������� Collider2D! �������� �� ������ ��������� ���������.");
                            // ����, ��� ������: ���������� ��� ��������� ���� ��� ������?
                            // Destroy(animalObject); return null;
                        }
                    }
                    else
                    {
                        Debug.LogError($"�� ������ �������� ������ � ������ 'AnimalPlacementArea' � ������ '{parentWagon.name}'! �������� �� ������ ��������� ���������.");
                        // ����, ��� ������: ���������� ��� ��������� ���� ��� ������?
                        // Destroy(animalObject); return null;
                    }
                }
                else
                {
                    // ��� ������ �� ������ ��������� ��� ���������� ������, �� ������� �� ������ ������
                    Debug.LogError($"�� ������� �������� Transform ������������� ������ ({animalObject.name}) ��� ������ AnimalPlacementArea. �������� ����������.");
                    Destroy(animalObject);
                    return null;
                }

                Debug.Log($"��������� ��������: {animalData.speciesName} � ������� {spawnPosition}");
                return animalObject; // ���������� ��������� ������ ���������
            }
            else
            {
                Debug.LogError($"�� ������� ��������� '{animalData.animalPrefab.name}' ({animalObject.name}) ����������� ��������� AnimalController! �������� ����������.");
                Destroy(animalObject);
                return null;
            }
        }
        else
        {
            // --- ��� ������� ������� (WorldItem) ---
            if (worldItemPrefab == null)
            {
                Debug.LogError($"World Item Prefab �� �������� � ItemSpawner! ���������� ���������� ������� '{dataToSpawn.itemName}'.");
                return null;
            }

            // 1. ������� ����� ������� ��������
            GameObject newItemObject = Instantiate(worldItemPrefab, spawnPosition, Quaternion.identity);

            // 2. ������������� �������
            newItemObject.transform.localScale = spawnScale;

            // 3. ----- ��������� �������� (������) -----
            if (trainController != null)
            {
                // �������� ����� � ��������� ��� bool ���������.
                // ���� �� ������� ��������� �������� (����� ������ false)...
                if (!trainController.AssignParentWagonByPosition(newItemObject.transform, spawnPosition))
                {
                    Debug.LogError($"�� ������� ����� ��� ��������� ������������ ����� ��� �������� '{dataToSpawn.itemName}' � ������� {spawnPosition}. ������� ���������.");
                    Destroy(newItemObject);
                    return null;
                }
                // ���� ����� ������ true, �������� ��� �������� ������ AssignParentWagonByPosition.
            }
            else
            {
                // ������� ����� � �� ����������� � ������, ���� trainController �� �����, �� ������� ��������������
                Debug.LogWarning($"TrainController �� �������� � ItemSpawner. ������� '{dataToSpawn.itemName}' �� ����� �������� � ������.");
                // ����, ����� �� ���������� ������� � ���� ������:
                // Destroy(newItemObject); return null;
            }

            // 4. �������� ��������� WorldItem
            WorldItem worldItemComponent = newItemObject.GetComponent<WorldItem>();
            if (worldItemComponent != null)
            {
                // 5. ����������� WorldItem
                worldItemComponent.itemData = dataToSpawn;
                worldItemComponent.InitializeVisuals();
                Debug.Log($"��������� �������: {dataToSpawn.itemName} � ������� {spawnPosition}");
                return newItemObject;
            }
            else
            {
                Debug.LogError($"�� ������� '{worldItemPrefab.name}' ����������� ��������� WorldItem! ������� '{dataToSpawn.itemName}' ���������.");
                Destroy(newItemObject);
                return null;
            }
        }
    }

    // ���������� ������ ��� ��������
    public GameObject SpawnItem(ItemData dataToSpawn, Vector3 spawnPosition)
    {
        // ���������� ������� �� ��������� ��� ����� ����� ��������
        return SpawnItem(dataToSpawn, spawnPosition, defaultSpawnScale);
    }
}