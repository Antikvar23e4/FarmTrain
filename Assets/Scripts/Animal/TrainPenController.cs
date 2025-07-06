// TrainPenController.cs

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TrainPenController : MonoBehaviour
{
    public static TrainPenController Instance { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private ItemSpawner itemSpawner;

    // <<< ���������: ��� ��� "�����" ��� ������ �� ������� �����
    private List<PenRuntimeInfo> livePenInfo = new List<PenRuntimeInfo>();

    private List<AnimalController> spawnedAnimals = new List<AnimalController>();
    private bool hasSpawned = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

    }

    void Start()
    {
        // � ������� ������ Start(), ��� Awake() ��� �������������� �����������.
        // AnimalPenManager.Instance ����� �� ����� null.
        InitializePenInfo();
        UpdateAllPenVisuals();

        if (itemSpawner == null)
        {
            Debug.LogError("ItemSpawner �� �������� � TrainPenController!", this);
            return;
        }

        if (!hasSpawned)
        {
            SpawnAnimalsFromData();
            hasSpawned = true;
        }
    }

    private void UpdateAllPenVisuals()
    {
        foreach (var penInfo in livePenInfo)
        {
            UpdatePenVisuals(penInfo.config.animalData);
        }
    }

    // <<< ����� ��������� �����
    public void UpdatePenVisuals(AnimalData forAnimal)
    {
        var penInfo = livePenInfo.FirstOrDefault(p => p.config.animalData == forAnimal);
        if (penInfo == null) return;

        int currentLevel = AnimalPenManager.Instance.GetCurrentPenLevel(forAnimal);
        if (currentLevel < 0 || currentLevel >= penInfo.config.upgradeLevels.Count)
        {
            Debug.LogError($"������������ ������� {currentLevel} ��� ������ {forAnimal.speciesName}");
            return;
        }

        penInfo.currentLevel = currentLevel;
        PenLevelData levelData = penInfo.config.upgradeLevels[currentLevel];

        // ������������� ���������� ������
        penInfo.penSpriteRenderer.sprite = levelData.penSprite;
        Debug.Log($"�������� ������ ��� ������ {forAnimal.speciesName} �� ������� {currentLevel}.");
    }


    // <<< ����� �����: ������� ������� �� ����� � �������� ������
    private void InitializePenInfo()
    {
        // �������� ��� "�����" ������ �� ����������� ���������
        List<PenConfigData> configs = AnimalPenManager.Instance.GetAllPenConfigs();

        foreach (var config in configs)
        {
            Transform penRendererTransform = FindDeepChild(transform, config.penSpriteRendererName);
            Transform placementAreaTransform = FindDeepChild(transform, config.placementAreaName);
            Transform animalParentTransform = FindDeepChild(transform, config.animalParentName);

            if (penRendererTransform == null) { /* ������ */ continue; }

            SpriteRenderer penRenderer = penRendererTransform.GetComponent<SpriteRenderer>();
            if (penRenderer == null) { /* ������ */ continue; }

            if (placementAreaTransform == null)
            {
                Debug.LogError($"TrainPenController �� ����� ����� ������ � ������ '{config.placementAreaName}'!", gameObject);
                continue;
            }
            if (animalParentTransform == null)
            {
                Debug.LogError($"TrainPenController �� ����� ����� ������ � ������ '{config.animalParentName}'!", gameObject);
                continue;
            }

            Collider2D areaCollider = placementAreaTransform.GetComponent<Collider2D>();
            if (areaCollider == null)
            {
                Debug.LogError($"�� ������� '{config.placementAreaName}' ����������� Collider2D!", placementAreaTransform);
                continue;
            }

            livePenInfo.Add(new PenRuntimeInfo
            {
                config = config,
                penSpriteRenderer = penRenderer, // <<< ��������� SpriteRenderer
                animalParent = animalParentTransform,
                placementArea = areaCollider
            });

        }
        Debug.Log($"<color=orange>[TRAIN DEBUG]</color> ���������������� {livePenInfo.Count} �������. �������� ��: ");
        foreach (var info in livePenInfo)
        {
            int capacity = AnimalPenManager.Instance.GetMaxCapacityForAnimal(info.config.animalData);
            Debug.Log($" - ����� ��� '{info.config.animalData.speciesName}', �����������: {capacity}, parent: {info.animalParent.name}");
        }
    }

    // <<< ���������: ���� ����� ������ ���� � ��������� ���� livePenInfo
    public PenRuntimeInfo GetLivePenInfoForAnimal(AnimalData animalData)
    {
        return livePenInfo.FirstOrDefault(p => p.config.animalData == animalData);
    }


    private void SpawnAnimalsFromData()
    {
        Debug.Log("<color=orange>[TRAIN DEBUG]</color> ������� SpawnAnimalsFromData...");

        if (livePenInfo.Count == 0)
        {
            Debug.LogError("<color=red>[TRAIN DEBUG]</color> ������: livePenInfo ����! �� ���� ��������, �.�. ��� ���������� � �������.");
            return;
        }

        // ������ �� ����������� �� ����� "�����" �������
        foreach (var penInfo in livePenInfo)
        {
            var animalData = penInfo.config.animalData;
            int countInManager = AnimalPenManager.Instance.GetAnimalCount(animalData);
            Debug.Log($"<color=orange>[TRAIN DEBUG]</color> �������� ����� ��� '{animalData.speciesName}'. � ��������� ��������: {countInManager} ��.");

            List<AnimalStateData> statesToSpawn = AnimalPenManager.Instance.GetStatesForAnimalType(penInfo.config.animalData);

            if (statesToSpawn.Count > 0)
            {
                Debug.Log($"������� ������ ��� {penInfo.config.animalData.speciesName}. ����� ����������: {statesToSpawn.Count}");
                foreach (var animalState in statesToSpawn)
                {
                    SpawnSingleAnimal(penInfo, animalState); // �������� "�����" PenRuntimeInfo
                }
            }
        }
    }

    // <<< ���������: ����� ��������� PenRuntimeInfo
    private void SpawnSingleAnimal(PenRuntimeInfo penInfo, AnimalStateData stateToLoad)
    {
        var animalData = penInfo.config.animalData;

        // ... (������ ����������� spawnPos �������� ��� ��) ...
        Vector3 spawnPos;
        if (stateToLoad.hasBeenPlaced)
        {
            spawnPos = stateToLoad.lastPosition;
        }
        else
        {
            spawnPos = GetRandomSpawnPosition(penInfo.placementArea.bounds);
        }

        GameObject animalGO = itemSpawner.SpawnItem(animalData.correspondingItemData, spawnPos);

        if (animalGO != null)
        {
            // ���������� "�����" ������ �� ��������
            animalGO.transform.SetParent(penInfo.animalParent, true);

            AnimalController newAnimal = animalGO.GetComponent<AnimalController>();
            if (newAnimal != null)
            {
                // ���������� "�����" ������ �� �������
                newAnimal.InitializeWithState(stateToLoad, penInfo.placementArea.bounds);
                spawnedAnimals.Add(newAnimal);
            }
            else
            {
                Debug.LogError($"������, ��������� ��������� ��� {animalData.name}, �� ����� ���������� AnimalController!", animalGO);
            }
        }
    }

    // ... (DespawnAnimal � GetRandomSpawnPosition �������� ����� ��� ���������) ...
    public bool DespawnAnimal(AnimalData animalData)
    {
        AnimalController animalToDespawn = spawnedAnimals.FirstOrDefault(a => a.animalData == animalData);
        if (animalToDespawn != null)
        {
            spawnedAnimals.Remove(animalToDespawn);
            Destroy(animalToDespawn.gameObject);
            return true;
        }
        return false;
    }

    private Vector3 GetRandomSpawnPosition(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            0
        );
    }
    private Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform result = FindDeepChild(child, childName);
            if (result != null)
                return result;
        }
        return null;
    }
}