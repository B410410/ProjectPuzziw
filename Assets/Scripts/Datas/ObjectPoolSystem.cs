using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 物件池系統
/// 回收重複使用物件
/// </summary>
public static class ObjectPoolSystem
{
    /// <summary>
    /// 回收再掉落的元素物件池(直排為單位)
    /// KEY：排編號
    /// VAL：元素清單
    /// </summary>
    private static Dictionary<int, List<Element>> dropElements
        = new Dictionary<int, List<Element>>();
    /// <summary>
    /// 建立元素回收清單查詢
    /// </summary>
    /// <param name="sizeX">排數量</param>
    public static void CreateElementDropPool(int sizeX)
    {
        for (int i = 0; i < sizeX; i++)
        {
            dropElements[i] = new List<Element>();
        }
    }
    /// <summary>
    /// (擴充功能)元素加入回收清單
    /// </summary>
    /// <param name="element">元素(擴充對象)</param>
    /// <param name="sizeX">排編號</param>
    public static void AddToDropPool(this Element element, int sizeX)
    {
        dropElements[sizeX].Add(element);
    }
    /// <summary>
    /// 預備重新掉落
    /// </summary>
    public static void ReadyToDrop(this MainSystem ms)
    {
        //每一直排執行排序處理
        for (int x = 0; x < dropElements.Count; x++)
        {
            //回收預備掉落的數量
            int dropCount = dropElements[x].Count;
            //直排排序
            for (int y = 0; y < dropElements[x].Count; y++)
            {
                //放置於棋盤格頂端排隊(座標)
                dropElements[x][y].SetPos(x, y + ms.sizeY);
                //設定要前往的 Grid 對象，
                //起始號碼從(總數 - 缺口)開始補，
                //每次補位 + 排隊的序號依序遞增
                dropElements[x][y].SetGrid(ms.grids[x][ms.sizeY - dropCount + y]);
                //加入移動掉落Action
                ms.AddDropMove(dropElements[x][y]);
            }
        }
    }

    public static void Droping()
    {

    }
}
