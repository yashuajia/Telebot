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
    }

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

}
