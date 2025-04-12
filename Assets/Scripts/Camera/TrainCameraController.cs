using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TrainCameraController : MonoBehaviour
{
    [Header("Wagon Setup")]
    public List<Transform> wagons; // ������ ���� ������� (������� ���������)
    public int startingWagonIndex = 1; // ������ ������, � �������� ��������

    [Header("Camera Settings")]
    public float transitionSpeed = 5f; // �������� ����������� � ����
    public float zoomInSize = 5f;  // ������ ������ ��� �������� �����
    public float zoomOutSize = 10f; // ������ ������ ��� ������ ����

    [Header("Panning Settings (Overview)")]
    public float panSpeed = 50f;   // �������� ����������� ������ ��� ������� ������ ������ ���� � ������ ������
    public float minPanX = 10f;    // ����������� ������� X ��� ���������������
    public float maxPanX = 70f;    // ������������ ������� X ��� ���������������

    private Camera mainCamera;
    private Vector3 targetPosition; // ������� �������, � ������� ������ ����� ������ ���������
    private float targetOrthographicSize; // ������� ��������������� ������, � �������� ������ ����� ������ ����������
    private int currentWagonIndex = -1; // ������ ������, �� ������� ������ ������ �������������
    private int lastWagonIndex = 1;    // ���������� ������ ������, �� ������� ������ ���� ������������� ��������� ��� ����� ��������� � ����� ������
    private bool isOverview = false; // ���� (�������������): true - ������ � ������ ������, false - ������ ������������� �� ������

    // Panning State
    private bool isPanning = false; // ����: true - ����� ������ ����� ������ ������ ���� � ������� ������
    private Vector3 panOrigin; // ��������������� ���������� ��� ������� �������� ��� ���������������

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null || !mainCamera.orthographic)
        {
            Debug.LogError("TrainCameraController requires an Orthographic Camera component on the same GameObject");
            enabled = false;
            return;
        }

        // ��������� ������ �� �� X-���������� (����� �������)
        wagons = wagons.OrderBy(w => w.position.x).ToList();

        // �������� �� ���������� ���������� �������
        if (startingWagonIndex <= 0 || startingWagonIndex >= wagons.Count)
        {
            Debug.LogWarning($"Starting wagon index ({startingWagonIndex}) is invalid or points to locomotive");
            startingWagonIndex = 1;
        }
        if (wagons.Count <= 1)
        {
            Debug.LogError("Not enough wagons assigned to the TrainCameraController");
            enabled = false;
            return;
        }

        // ��������� ���������
        currentWagonIndex = startingWagonIndex; // ����� �� ������ ������
        lastWagonIndex = currentWagonIndex; // �� ����� ������ ��� ��������� �����
        isOverview = false; // ����� ������
        targetPosition = GetTargetPositionForWagon(currentWagonIndex); // ������� ������� - �������
        targetOrthographicSize = zoomInSize; // ������� ��� - �������

        transform.position = targetPosition;
        mainCamera.orthographicSize = targetOrthographicSize;

        CalculatePanLimits(); // ������������ ������� ���������������
    }

    void Update()
    {
        HandleInput(); // ��������� ����� ������������
        SmoothCameraMovement(); // ������� ����������� ������ � ������� ������� � �������� ����
    }

    void HandleInput() // ��������� ����� ������������
    {
        // --- ��������� �������� ���� ---
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll < 0f && !isOverview) // ��������� � ������ ������
        {
            EnterOverviewMode(); // ������� � ����� ������
        }
        else if (scroll > 0f && isOverview) // ����������� � ������ ������
        {
            ExitOverviewMode(lastWagonIndex); // ������������ � ���������� ������
        }

        // --- ��������� ������ ���� ---
        if (Input.GetMouseButtonDown(0)) // ����� ������ ����
        {
            HandleLeftClick();
        }

        // --- ��������� ��������������� (������ ������ ����) ---
        HandlePanning();
    }
     
    void HandleLeftClick() // ��������� ������ ����
    {
        // ��������� ��� �� ������ � ����� ������, ��� �������� ����
        RaycastHit2D hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero); 

        if (hit.collider != null) // ��� ����� � �����-�� ���������
        {
            if (hit.collider.CompareTag("Wagon")) // �������� �� ������
            {
                int clickedWagonIndex = wagons.IndexOf(hit.transform);

                if (isOverview) // ����� ������
                {
                    if (clickedWagonIndex > 0 && clickedWagonIndex < wagons.Count) // ���������� ������� ������
                    {
                        ExitOverviewMode(clickedWagonIndex); // ������� � ������ ������ �� ��������� �����
                    }
                }
                else // ����� ������
                {
                    // ���� �������� �� ������� �������� �����
                    if (clickedWagonIndex == currentWagonIndex + 1 || clickedWagonIndex == currentWagonIndex - 1)
                    {
                        if (clickedWagonIndex > 0 && clickedWagonIndex < wagons.Count) // ���������� ������� ������
                        {
                            MoveToWagon(clickedWagonIndex); // ������� � ���������� ������
                        }
                    }
                }
            }
        }
    }

    void HandlePanning() // ������������ ��������������� ������ � ������� ������ ������ ����
    {
        // ������� �������� ������ � ������ ������

        if (!isOverview)
        {
            isPanning = false; // ��������, ��� ���� �������
            return;
        }

        if (Input.GetMouseButtonDown(1)) // ������ ������ ������
        {
            isPanning = true;
            // ���������� ��������� ������� ������ (����� �������)
            panOrigin = transform.position;
            // ���������� ������� ���� � ���
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            panOrigin -= mouseWorldPos; // ��������� � ��������� �������� ����� ������� � ������ ����� ����

            // ��� �������� ��� �������, ����� �� �������� ��� � ����� �����

        }

        if (Input.GetMouseButton(1) && isPanning) 
        {
            // ������������ �������� ������� ������ 
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 desiredPos = panOrigin + mouseWorldPos;

            // ������������ �������� �� X (������ �� ����� ������ ����� minPanX � ������ maxPanX)
            float clampedX = Mathf.Clamp(desiredPos.x, minPanX, maxPanX);

            // ��������� ������ X ���������� ������� �������
            targetPosition = new Vector3(clampedX, targetPosition.y, targetPosition.z);
        }

        if (Input.GetMouseButtonUp(1)) // ������ ������ ��������
        {
            isPanning = false;
        }
    }


    void EnterOverviewMode() // ������� � ����� ������
    {
        isOverview = true;
        lastWagonIndex = currentWagonIndex; // ���������, ������ ����
        currentWagonIndex = -1; // ��� ������
        targetOrthographicSize = zoomOutSize; // ������������� ������� ������ ��� ���������

        targetPosition = GetTargetPositionForWagon(lastWagonIndex); // ������� �� ������� ���������� ������, �� � ������� �����
        CalculatePanLimits(); // ����������� ������ ��� ��������� ����
    }

    void ExitOverviewMode(int targetIndex) // ������� � ����� ������
    {
        if (targetIndex <= 0 || targetIndex >= wagons.Count) // ���������� ������� ������
        {
            Debug.LogWarning($"Cannot focus on invalid index {targetIndex}. Returning to last valid wagon {lastWagonIndex}");
            targetIndex = lastWagonIndex; // ������� � ���������� ���������
            if (targetIndex <= 0) targetIndex = 1; // ������� ������, ���� lastWagonIndex ��� ������������
        }

        isOverview = false;
        isPanning = false; // ��������� ��������������� ��� ������ �� ������
        currentWagonIndex = targetIndex;
        lastWagonIndex = currentWagonIndex;
        targetOrthographicSize = zoomInSize;
        targetPosition = GetTargetPositionForWagon(currentWagonIndex);
        Debug.Log($"Exiting Overview Mode, focusing on Wagon {currentWagonIndex}");
    }

    void MoveToWagon(int index) // ������������� ���� ��� �������� �������� ������ � ������ � �������� ��������
    {
        if (index > 0 && index < wagons.Count && !isOverview) // ���������� ������� ������
        {
            currentWagonIndex = index;
            lastWagonIndex = currentWagonIndex;
            targetPosition = GetTargetPositionForWagon(currentWagonIndex);
            Debug.Log($"Moving focus to Wagon {currentWagonIndex}");
        }
        else
        {
            Debug.LogWarning($"Cannot move to index {index}. It might be the locomotive, out of bounds, or in overview mode.");
        }
    }

    void SmoothCameraMovement() // ��������� ������� �������� � ��� ������ ������ ����
    {
        // ������� �����������
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, transitionSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // ������� ���
        float smoothedSize = Mathf.Lerp(mainCamera.orthographicSize, targetOrthographicSize, transitionSpeed * Time.deltaTime);
        mainCamera.orthographicSize = smoothedSize;
    }

    // ��������������� ������� ��� ��������� ������� ������� �� ������ ������
    Vector3 GetTargetPositionForWagon(int index)
    {
        if (index >= 0 && index < wagons.Count) // ���������� ������� ������
        {
            return new Vector3(wagons[index].position.x, GetBaseYPosition(), transform.position.z);
        }
        return transform.position;
    }

    // ���������� ������� Y-������� ������
    float GetBaseYPosition()
    {
        // Y ������� ������ 
        return wagons.Count > 0 ? wagons[0].position.y : 0f;
    }


    void CalculatePanLimits()     // ������������ ������� ��� ��������������� � ������ ������
    {
        if (wagons.Count == 0) return;


        // ������������ ����� ������ ��������� ������ ������ � ������ ������� �������
        minPanX = wagons[0].position.x; // ����� ������ �� ����� ����������
        maxPanX = wagons[wagons.Count - 1].position.x; // ����� ������ �� ������ ���������� ������

        Debug.Log($"Calculated Pan Limits: MinX={minPanX}, MaxX={maxPanX}");
    }


    // --- ��������� ������ ��� ���������� ������������� �� ������ �������� ---

    public bool IsInOverview()
    {
        return isOverview;
        // true - ������ � ������ ������, false - ������ ������������� �� ������
    }

    public int GetCurrentWagonIndex()
    {
        return isOverview ? -1 : currentWagonIndex;
    }

    public Transform GetCurrentWagonTransform()
    {
        if (!isOverview && currentWagonIndex > 0 && currentWagonIndex < wagons.Count)
        {
            return wagons[currentWagonIndex];
        }
        return null;
    }
}