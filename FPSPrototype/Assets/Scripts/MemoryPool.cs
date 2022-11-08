using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryPool : MonoBehaviour
{
    private class PoolItem
    {
        public bool isActive;           // gameObject의 활성화 여부
        public GameObject gameObject;   // 실제 게임오브젝트
    }
    private int increaseCount = 5;
    private int maxCount;
    private int activeCount;

    private GameObject poolObject;
    private List<PoolItem> poolItemList;    // 관리되는 오브젝트를 관리하는 리스트

    public int MaxCount => maxCount;
    public int ActiveCount => activeCount;

    private Vector3 temPosition = new Vector3(48, 1, 48);
    public MemoryPool(GameObject poolObject)
    {
        maxCount = 0;
        activeCount = 0;
        this.poolObject = poolObject;
        poolItemList = new List<PoolItem>();
        InstantiateObjects();
    }

    public void InstantiateObjects()
    {
        maxCount += increaseCount;
        for(int i = 0; i < increaseCount; ++i)
        {
            PoolItem poolItem = new PoolItem();
            poolItem.isActive = false;
            poolItem.gameObject = GameObject.Instantiate(poolObject);
            poolItem.gameObject.transform.position = temPosition;
            poolItem.gameObject.SetActive(false);

            poolItemList.Add(poolItem);
        }
    }
    public void DestroyObjects()
    {
        if(poolItemList == null) return;

        int count = poolItemList.Count;
        for(int i = 0 ; i < count ; ++i)
        {
            GameObject.Destroy(poolItemList[i].gameObject);
        }
        poolItemList.Clear();
    }
    public GameObject ActivatePoolItem()
    {
        if(poolItemList == null) return null;

        if(maxCount == activeCount)
        {
            InstantiateObjects();
        }
        int count = poolItemList.Count;
        for(int i = 0; i < count; ++i)
        {
            PoolItem poolItem = poolItemList[i];
            if(poolItem.isActive == false)
            {
                activeCount++;
                poolItem.isActive = true;
                poolItem.gameObject.SetActive(true);

                return poolItem.gameObject;
            }
        }
        return null;
    }
    public void InactivatePoolItem(GameObject removeObject)
    {
        if (poolItemList == null || removeObject == null) return;

        int count = poolItemList.Count;
        for (int i = 0; i < count; ++i)
        {
            PoolItem poolItem = poolItemList[i];
            if(poolItem.gameObject == removeObject)
            {
                activeCount--;
                poolItem.gameObject.transform.position = temPosition;
                poolItem.isActive = false;
                poolItem.gameObject.SetActive(false);
                return;
            }
        }
    }
    public void InActivateAllPoolItems()
    {
        if (poolItemList == null) return;

        int count = poolItemList.Count;
        for(int i = 0; i<count; ++i)
        {
            PoolItem poolItem = poolItemList[i];
            if(poolItem.gameObject != null && poolItem.isActive == true)
            {
                poolItem.isActive = false;
                poolItem.gameObject.SetActive(false);
            }
        }
        activeCount = 0;
    }
}