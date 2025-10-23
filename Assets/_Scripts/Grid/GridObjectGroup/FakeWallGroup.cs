using System;
using UnityEngine;

public enum FakeWallGroupState
{
    Intact,
    PartialBroken,
    AllBroken,
}
public class FakeWallGroup : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private FakeWall[] walls;
    void Start()
    {
        walls = GetComponentsInChildren<FakeWall>();
        RoomManager.OnRoomChanged += OnRoomChanged;
    }

    private FakeWallGroupState fakeWallGroupState = FakeWallGroupState.Intact;

    //public event Action tryBreak;
    public event Action triggerBreak;
    public event Action triggerRecover;

    // Update is called once per frame

    public void TriggerBreakAll()
    {
        fakeWallGroupState = FakeWallGroupState.AllBroken;
        triggerBreak?.Invoke();
    }

    public void TriggerRecoverAll()
    {
        fakeWallGroupState = FakeWallGroupState.Intact;
        triggerRecover?.Invoke();
        foreach (var fakewall in GetComponentsInChildren<FakeWall>())
        {
            if (fakewall.IsBroken) fakeWallGroupState = FakeWallGroupState.PartialBroken;
            //如果全都broken了那其实应该是allbroken，但是现在没必要管
        }
    }

    public void OnRoomChanged(Vector2Int oldRoomPos, Vector2Int newRoomPos)
    {
        if (fakeWallGroupState == FakeWallGroupState.Intact) return;
        TriggerRecoverAll();
    }

}
