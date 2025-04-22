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
        if (worldItemPrefab == null) { Debug.LogError("..."); return; }
        if (itemsToSpawnAtStart == null || itemsToSpawnAtStart.Length == 0) { return; }

        foreach (ItemSpawnInfo spawnInfo in itemsToSpawnAtStart)
        {
            // �������� ����� ����� �����, ��������� ������� �� ���������
            SpawnItem(spawnInfo.itemData, spawnInfo.position, defaultSpawnScale);
        }
    }

    public GameObject SpawnItem(ItemData dataToSpawn, Vector3 spawnPosition, Vector3 spawnScale)
    {
        if (worldItemPrefab == null)
        {
            Debug.LogError("World Item Prefab �� �������� � ItemSpawner!");
            return null;
        }
        if (dataToSpawn == null)
        {
            Debug.LogWarning("������� ���������� ������� � null ItemData.");
            return null; 
        }

        // 1. ������� ����� �������
        GameObject newItemObject = Instantiate(worldItemPrefab, spawnPosition, Quaternion.identity);

        // 2. ������������� ���������� �������
        newItemObject.transform.localScale = spawnScale;

        // ----- ��������� �������� -----
        if (trainController != null)
        {
            // �������� ����� ����������� ��� ������ � ��������� ��������
            trainController.AssignParentWagonByPosition(newItemObject.transform, spawnPosition);
        }
        else
        {
            Debug.LogWarning("TrainController �� ��������");
            Destroy(newItemObject);
            return null;       
        }

        // 3. �������� ��������� WorldItem
        WorldItem worldItemComponent = newItemObject.GetComponent<WorldItem>();

        // 4. ����������� WorldItem
        if (worldItemComponent != null)
        {
            worldItemComponent.itemData = dataToSpawn; // ��������� ���������� ������
            worldItemComponent.InitializeVisuals();   // �������������� ������
            Debug.Log($"��������� {dataToSpawn.itemName} � ������� {spawnPosition} � ��������� {spawnScale}");
            return newItemObject; 
        }
        else
        {
            Debug.LogError($"�� ������� {worldItemPrefab.name} ����������� ��������� WorldItem!");
            Destroy(newItemObject);
            return null;
        }
    }

    public GameObject SpawnItem(ItemData dataToSpawn, Vector3 spawnPosition)
    {
        return SpawnItem(dataToSpawn, spawnPosition, defaultSpawnScale);
    }
}