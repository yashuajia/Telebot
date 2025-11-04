using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    [Header("Prefab & Layout")]
    [SerializeField] private GameObject itemSlotPrefab; // ItemSlot 预制体
    [SerializeField] private Transform slotRoot;        // 生成槽的父节点

    private List<BulletModifierObj> inventory = new List<BulletModifierObj>();
    private ItemSlot headSlot = null; // 链表头

    void Awake()
    {
        GameEvents.OnPlayerDeath += OnPlayerDeath;
    }

    /// <summary>
    /// 注册新物品到背包（添加到链尾）
    /// </summary>
    public void RegisterItem(BulletModifierObj item)
    {
        if (item == null || inventory.Contains(item))
            return;

        inventory.Add(item);
        Debug.Log($"[Inventory] 注册新物品：{item.name}");

        // 创建一个新的 ItemSlot
        GameObject slotObj = Instantiate(itemSlotPrefab, slotRoot);

        ItemSlot newSlot = slotObj.GetComponent<ItemSlot>();


        // 如果是第一个物品
        if (headSlot == null)
        {
            headSlot = newSlot;
            newSlot.Initialize(transform, item.transform); // 跟随玩家或背包位置
        }
        else
        {
            // 否则让它跟随上一个槽
            ItemSlot previousSlot = slotRoot.GetChild(slotRoot.childCount - 2).GetComponent<ItemSlot>();
            newSlot.Initialize(previousSlot.transform, item.transform);
        }
    }

    /// <summary>
    /// 从背包中移除最后一个物品
    /// </summary>
    public BulletModifierObj PopLastItem()
    {
        if (inventory.Count == 0)
            return null;

        // 取出最后一个
        BulletModifierObj lastItem = inventory[inventory.Count - 1];
        inventory.RemoveAt(inventory.Count - 1);

        // 销毁对应的槽
        //here call multiple times in one frame will cause trouble
        // Transform lastSlot = slotRoot.GetChild(slotRoot.childCount - 1);

        Transform lastSlot = lastItem.transform.parent;
        Destroy(lastSlot.gameObject);

        Debug.Log($"[Inventory] 移除物品：{lastItem.name}");

        if (inventory.Count == 0)
            headSlot = null;

        return lastItem;
    }

    private void OnPlayerDeath()
    {
        while (inventory.Count != 0)
        {
            Debug.Log(inventory.Count);
            BulletModifierObj item = PopLastItem();
            Destroy(item.gameObject);
        }
    }

    /// <summary>
    /// 获取物品数量
    /// </summary>
    public int GetItemCount() => inventory.Count;

    /// <summary>
    /// 获取所有物品
    /// </summary>
    public List<BulletModifierObj> GetAllItems() => new List<BulletModifierObj>(inventory);
}