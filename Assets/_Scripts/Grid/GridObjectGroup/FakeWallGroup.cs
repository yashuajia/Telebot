using System;
using Unity.VisualScripting;
using UnityEngine;

public class FakeWallGroup : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private FakeWall[] walls;
    void Start()
    {
        walls = GetComponentsInChildren<FakeWall>();
        RoomManager.OnRoomChanged += OnRoomChanged;
    }

    //public event Action tryBreak;
    public event Action triggerBreak;
    public event Action triggerRecover;

    // Update is called once per frame

    public void TriggerBreakAll()
    {
        triggerBreak?.Invoke();
    }

    public void TriggerRecoverAll()
    {
        triggerRecover?.Invoke();
    }

    public void OnRoomChanged(Vector2Int oldRoomPos, Vector2Int newRoomPos)
    {
        TriggerRecoverAll();
    }

}
