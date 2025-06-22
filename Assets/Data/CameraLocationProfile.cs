using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewLocationProfile", menuName = "Camera/Location Profile")]
public class CameraLocationProfile : ScriptableObject
{
    [Header("Points of Interest")]
    // ����� ����� ������ ���� ������, ���� ������
    public List<Transform> focusPoints;
    public int startingPointIndex = 0; // � ����� ����� ��������

    [Header("Camera Settings")]
    public float transitionSpeed = 5f;
    public float zoomInSize = 5f;
    public float zoomOutSize = 10f;
}