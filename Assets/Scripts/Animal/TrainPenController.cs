using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class PenInfo
{
    public AnimalData animalData;
    public Collider2D placementArea;
    public Transform animalParent;
}

public class TrainPenController : MonoBehaviour
{
    public static TrainPenController Instance { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private ItemSpawner itemSpawner;

    [Header("Configuration")]
    [SerializeField] private List<PenInfo> penConfigurations;

    private List<AnimalController> spawnedAnimals = new List<AnimalController>();
    private bool hasSpawned = false;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
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

    private void SpawnAnimalsFromData()
    {
        Debug.Log("TrainPenController: ������� ����� �������� �� ������ �� AnimalPenManager...");

        foreach (var penConfig in penConfigurations)
        {
            // �������� �� ������ ����������, � ������ ���������!
            List<AnimalStateData> statesToSpawn = AnimalPenManager.Instance.GetStatesForAnimalType(penConfig.animalData);

            if (statesToSpawn.Count > 0)
            {
                Debug.Log($"������� ������ ��� {penConfig.animalData.speciesName}. ����� ����������: {statesToSpawn.Count}");

                // �������� �� ������� ������������ ��������� � ������� ��� ���� ��������
                foreach (var animalState in statesToSpawn)
                {
                    SpawnSingleAnimal(penConfig, animalState); // �������� ��������� � ����� ������
                }
            }
        }
    }


    private void SpawnSingleAnimal(PenInfo penConfig, AnimalStateData stateToLoad)
    {
        var animalData = penConfig.animalData;

        if (animalData.correspondingItemData == null)
        {
            Debug.LogError($"� {animalData.name} �� ������ correspondingItemData!", animalData);
            return;
        }

        Debug.Log($"<color=lightblue>[TrainPenController]</color> ������� ��������� ��� {stateToLoad.animalData.speciesName}. " +
            $"�������� ������ ������� {stateToLoad.lastPosition}" +
          $"�������� ���� hasBeenPlaced. ��� ��������: <color=yellow>{stateToLoad.hasBeenPlaced}</color>");

        Vector3 spawnPos;

        if (stateToLoad.hasBeenPlaced)
        {
            // ���� � ��������� ���� ����������� �������, ���������� ��
            spawnPos = stateToLoad.lastPosition;
            Debug.Log($"<color=lightblue>[TrainPenController]</color> �������������� {animalData.speciesName} �� ����������� ������� {spawnPos}");
        }
        else
        {
            // ���� ��� ����� ��������, ���������� ��������� �������
            spawnPos = GetRandomSpawnPosition(penConfig.placementArea.bounds);
            Debug.Log($"<color=lightblue>[TrainPenController]</color> ������ ������ {animalData.speciesName} �� ��������� ������� {spawnPos}");
        }

        Debug.Log($"������������� ������� ����� ������� {spawnPos}");


        GameObject animalGO = itemSpawner.SpawnItem(animalData.correspondingItemData, spawnPos);

        if (animalGO != null)
        {
            animalGO.transform.SetParent(penConfig.animalParent, true);

            AnimalController newAnimal = animalGO.GetComponent<AnimalController>();
            if (newAnimal != null)
            {
                newAnimal.InitializeWithState(stateToLoad, penConfig.placementArea.bounds);
                spawnedAnimals.Add(newAnimal);
            }
            else
            {
                Debug.LogError($"������, ��������� ��������� ��� {animalData.name}, �� ����� ���������� AnimalController!", animalGO);
            }
        }
    }

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
}