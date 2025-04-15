using UnityEngine;

public class CreateGrid : MonoBehaviour
{

    [SerializeField]GameObject bed;

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
        GenerateGrid();

    }

    void GenerateGrid()
    {
        for (float i = (PosX + gridSizeX /2) - spacingX; i < PosX+((CountBedsX + spacingX) * gridSizeX) + gridSizeX; i+= spacingX + gridSizeX) {

            for (float j = (PosY + gridSizeY / 2) - spacingY; j < PosY + ((CountBedsY + spacingY) * gridSizeY ); j+= spacingY + gridSizeY) {
                if(j  + spacingY == 0) continue; // ������� ��� 2 ����
                Vector3 spawnPosition = new Vector3(i + spacingX, j + spacingY, 0);
                GameObject newCube = Instantiate(bed,spawnPosition,Quaternion.identity);
                BedsScripts.AddBed(newCube);
                Debug.Log(newCube.gameObject.name);
                newCube.transform.localScale =new Vector3(gridSizeX,gridSizeY);
               

            }
        }
    }
}
