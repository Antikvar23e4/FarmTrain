using UnityEngine;
using UnityEngine.UI;

public class TestAnimalSpawnerButton : MonoBehaviour
{
    public ItemSpawner itemSpawner;
    public ItemData animalItemToSpawn; 
    public Transform spawnPoint;

    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(SpawnAnimal);
        }
        if (itemSpawner == null || animalItemToSpawn == null || spawnPoint == null)
        {
            Debug.LogError("�� ��� ���� ��������� � TestAnimalSpawnerButton!", gameObject);
        }
    }

    public void SpawnAnimal()
    {
        if (itemSpawner != null && animalItemToSpawn != null && spawnPoint != null)
        {
            if (animalItemToSpawn.itemType != ItemType.Animal || animalItemToSpawn.associatedAnimalData == null)
            {
                Debug.LogError($"������� {animalItemToSpawn.itemName} �� �������� �������� ��� ��� ���� �� ������� ������ (AnimalData). ���� �� ����� ��������.");
                return; 
            }

            Debug.Log($"������ ������ ������ {animalItemToSpawn.itemName}");

            AnimalPenManager.Instance.AddAnimal(animalItemToSpawn.associatedAnimalData);

            Debug.Log($"������ ���������. ������ � AnimalPenManager ���� ����� �������� ���� {animalItemToSpawn.associatedAnimalData.speciesName}");
        }
        else
        {
            Debug.LogError("�� ���� ���������� �������� - �� ��� ������ �����������!");
        }
    }

}