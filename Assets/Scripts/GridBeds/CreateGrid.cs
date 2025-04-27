using UnityEngine;

public class CreateGrid : MonoBehaviour
{
    

[SerializeField]GameObject slot;
    [SerializeField] GameObject bedprefab;
    [SerializeField] GameObject BedManager;
    BedsManagerScript bedsManagerScript;

    [Header("Position")]
    public float GridPosX; // ������� ����� �� X
    public float GridPosY; // ������� ����� �� Y


    [Header("gridSize")]
    //public float spacingX; // ������ ����� ��������
    public float spacingY; // ������ ����� ��������
    public int CountBedsX; // ������ ����� X
    public int CountBedsY; // ������ ����� Y

    private float slotPosX;
    private float slotPosY;
    GameObject newSlot;
    void Start()
    {
        slotPosX = GridPosX;
        slotPosY = GridPosY;
        bedsManagerScript = BedManager.GetComponent<BedsManagerScript>();
        GenerateGrid();

    }

    void GenerateGrid()
    {
   
        for (float i = 1; i < CountBedsX; i++) {
           
            for (int j = 0; j < CountBedsY; j++) {
                Vector3 spawnPosition = new Vector3(slotPosX, slotPosY, 0);
                newSlot = Instantiate(slot, spawnPosition, Quaternion.identity);
                newSlot.transform.position = spawnPosition;
                GenerateBed(bedprefab, newSlot);
                slotPosY += newSlot.transform.localScale.y + spacingY;
            }
            slotPosX += newSlot.transform.localScale.x;
            slotPosY = GridPosY;
        }
    }
    void GenerateBed(GameObject smallSquarePrefab, GameObject largeSquare)
    {
        

        float lengthSlot = largeSquare.transform.localScale.x;
        float widthSlot = largeSquare.transform.localScale.y;
        Vector3 sizeBed = new Vector3(lengthSlot / 2, widthSlot / 2);

        // ������� ����� ����
        GameObject smallSquare1 = Instantiate(smallSquarePrefab, largeSquare.transform.position + new Vector3(-lengthSlot/4,  widthSlot/4, 0), Quaternion.identity);
        smallSquare1.transform.localScale = sizeBed;
        smallSquare1.transform.parent = largeSquare.transform;
        

        // ������� ������ ����
        GameObject smallSquare2 = Instantiate(smallSquarePrefab, largeSquare.transform.position + new Vector3(-lengthSlot / 4, -widthSlot / 4, 0), Quaternion.identity);
        smallSquare2.transform.localScale = sizeBed;
        smallSquare2.transform.parent = largeSquare.transform;


        // ������ ����� ����
        GameObject smallSquare3 = Instantiate(smallSquarePrefab, largeSquare.transform.position + new Vector3(lengthSlot / 4, -widthSlot / 4, 0), Quaternion.identity);
        smallSquare3.transform.localScale = sizeBed;
        smallSquare3.transform.parent = largeSquare.transform;


        // ������ ������ ����
        GameObject smallSquare4 = Instantiate(smallSquarePrefab, largeSquare.transform.position + new Vector3(lengthSlot / 4, widthSlot / 4, 0), Quaternion.identity);
        smallSquare4.transform.localScale = sizeBed;
        smallSquare4.transform.parent = largeSquare.transform;

        bedsManagerScript.AddBed(largeSquare,new GameObject[] { smallSquare1 , smallSquare2, smallSquare3, smallSquare4});

    }

  
}


