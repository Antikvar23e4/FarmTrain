using UnityEngine;

public class CreateGrid : MonoBehaviour
{
    

[SerializeField]GameObject slot;
    [SerializeField] GameObject bedprefab;
    [SerializeField] GameObject BedManager;
    BedsManagerScript bedsManagerScript;

    [Header("Position")]
    public float PosX; // ������� ����� �� X
    public float PosY; // ������� ����� �� Y

    [Header("Size 1 slot")]
    public float gridSizeX; // ������ ����� �� x
    public float gridSizeY; // ������ ����� �� y

    [Header("gridSize")]
    public float spacingX; // ������ ����� ��������
    public float spacingY; // ������ ����� ��������
    public int CountBedsX; // ������ ����� X
    public int CountBedsY; // ������ ����� Y

    void Start()
    {
        bedsManagerScript = BedManager.GetComponent<BedsManagerScript>();
        GenerateGrid();

    }

    void GenerateGrid()
    {
        int col = 2;
        int count = 0;
        for (float i = (PosX + gridSizeX /2) - spacingX; i < PosX+((CountBedsX + spacingX) * gridSizeX) + gridSizeX; i+= spacingX + gridSizeX) {
            for (float j = (PosY + gridSizeY / 2) - spacingY; j < PosY + ((CountBedsY + spacingY) * gridSizeY ); j+= spacingY + gridSizeY) {
                count++;
                if (count == col) { 
                    col += 3;
                    continue; } 
                Vector3 spawnPosition = new Vector3(i + spacingX, j + spacingY, 0);
                GameObject newCube = Instantiate(slot, spawnPosition,Quaternion.identity);
                GenerateBed(bedprefab, newCube);
                newCube.transform.localScale =new Vector3(gridSizeX,gridSizeY);
            }
        }
    }
    void GenerateBed(GameObject smallSquarePrefab, GameObject largeSquare)
    {
        
        float smallSquareOffset = 0.2f; // ���������� ����� ���������� ����������
        
        // ������� ����� ����
        GameObject smallSquare1 = Instantiate(smallSquarePrefab, largeSquare.transform.position + new Vector3(-smallSquareOffset, smallSquareOffset, 0), Quaternion.identity);
        
        smallSquare1.transform.parent = largeSquare.transform;
        bedsManagerScript.AddBed(smallSquare1);

        // ������� ������ ����
        GameObject smallSquare2 = Instantiate(smallSquarePrefab, largeSquare.transform.position + new Vector3(smallSquareOffset, smallSquareOffset, 0), Quaternion.identity);
        smallSquare2.transform.parent = largeSquare.transform;
        bedsManagerScript.AddBed(smallSquare2);
        // ������ ����� ����
        GameObject smallSquare3 = Instantiate(smallSquarePrefab, largeSquare.transform.position + new Vector3(-smallSquareOffset  , -smallSquareOffset , 0), Quaternion.identity);
        smallSquare3.transform.parent = largeSquare.transform;
        bedsManagerScript.AddBed(smallSquare3);
        // ������ ������ ����
        GameObject smallSquare4 = Instantiate(smallSquarePrefab, largeSquare.transform.position + new Vector3(smallSquareOffset , -smallSquareOffset , 0), Quaternion.identity);
        smallSquare4.transform.parent = largeSquare.transform;
        bedsManagerScript.AddBed(smallSquare4);
    }

}


