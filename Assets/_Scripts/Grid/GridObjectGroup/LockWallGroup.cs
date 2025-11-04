using System;
using UnityEngine;

//actually duplicated code, but whatever
public class LockWallGroup : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private LockWall[] walls;
    void Start()
    {
        walls = GetComponentsInChildren<LockWall>();
    }

    private WallGroupState wallGroupState = WallGroupState.Intact;

    //public event Action tryBreak;
    public event Action triggerBreak;
    public event Action triggerRecover;

    // Update is called once per frame

    public void TriggerBreakAll()
    {
        wallGroupState = WallGroupState.AllBroken;
        triggerBreak?.Invoke();
    }

    public void TriggerRecoverAll()
    {
        wallGroupState = WallGroupState.Intact;
        triggerRecover?.Invoke();
        foreach (var lockWall in walls)
        {
            if (lockWall.IsBroken) wallGroupState = WallGroupState.PartialBroken;
            //如果全都broken了那其实应该是allbroken，但是现在没必要管
        }
    }

    //recover when player dead



    // public void OnRoomChanged(Vector2Int oldRoomPos, Vector2Int newRoomPos)
    // {
    //     if (wallGroupState == WallGroupState.Intact) return;
    //     TriggerRecoverAll();
    // }

}
