using UnityEngine;
using UnityEngine.UI;

public class SpawnButtonHandler : MonoBehaviour
{
    [Tooltip("������ �� ������� ������� � �����")]
    public ItemSpawner mainSpawner; 

    [Header("��� �������� ���� �������")]
    public ItemData itemToSpawn;
    public Vector3 spawnPosition = Vector3.zero;
    public Vector3 spawnScale = Vector3.one;

    public void HandleSpawnClick()
    {
        if (mainSpawner == null)
        {
            Debug.LogError("Main Spawner �� ��������!");
            return;
        }
        if (itemToSpawn == null)
        {
            Debug.LogError("ItemData ��� ������ �� �������� �� ���� ������!");
            return;
        }

        GameObject spawnedObject = mainSpawner.SpawnItem(itemToSpawn, spawnPosition, spawnScale);

        if (spawnedObject != null)
        {
            Debug.Log($"������ ������� ���������� {itemToSpawn.itemName}");
        }
        else
        {
            Debug.LogWarning($"������ �� ������� ���������� {itemToSpawn.itemName}");
        }
    }
}