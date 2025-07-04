using UnityEngine;
using System;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public bool IsGamePaused { get; private set; }
    public event Action<bool> OnPauseStateChanged;

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

    public void RequestPause(object requester)
    {
        if (IsGamePaused) return;

        IsGamePaused = true;
        // <<< ���������: ������� Time.timeScale
        // Time.timeScale = 0f; 
        OnPauseStateChanged?.Invoke(true);
        Debug.Log($"<color=orange>���� ������ ������������. ��������: {requester.GetType().Name}</color>");
    }

    public void RequestResume(object requester)
    {
        if (!IsGamePaused) return;

        IsGamePaused = false;
        // <<< ���������: ������� Time.timeScale
        // Time.timeScale = 1f; 
        OnPauseStateChanged?.Invoke(false);
        Debug.Log($"<color=cyan>���� ������ �������������. ��������: {requester.GetType().Name}</color>");
    }
}