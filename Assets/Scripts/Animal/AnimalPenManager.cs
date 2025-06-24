using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AnimalPenManager : MonoBehaviour
{
    public static AnimalPenManager Instance { get; private set; }

    private List<AnimalStateData> allAnimals = new List<AnimalStateData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void AddAnimal(AnimalData animalData)
    {
        AnimalStateData newState = new AnimalStateData(animalData);
        allAnimals.Add(newState);
        Debug.Log($"<color=green>[AnimalPenManager]</color> ��������� ����� ��������: {animalData.speciesName}. " +
                  $"����� � ������: <color=yellow>{allAnimals.Count}</color> ��������. " +
                  $"�� ��� ����� ����: <color=yellow>{GetAnimalCount(animalData)}</color>");
    }

    public bool SellAnimal(AnimalData animalData)
    {
        AnimalStateData animalToRemove = allAnimals.FirstOrDefault(a => a.animalData == animalData);

        if (animalToRemove != null)
        {
            allAnimals.Remove(animalToRemove);
            Debug.Log($"�� AnimalPenManager ������� ��������: {animalData.speciesName}. ��������: {GetAnimalCount(animalData)}");
            return true;
        }

        Debug.LogWarning($"������� ������� {animalData.speciesName}, �� � ������ �� �� �������.");
        return false;
    }


    public int GetAnimalCount(AnimalData animalData)
    {
        return allAnimals.Count(a => a.animalData == animalData);
    }

    public List<AnimalStateData> GetStatesForAnimalType(AnimalData animalData)
    {
        return allAnimals.Where(a => a.animalData == animalData).ToList();
    }
}