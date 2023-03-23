using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 擴充工具庫 (靜態腳本)
/// </summary>
public static class ExtensionTools
{
    #region 元素移動相關
    /// <summary>
    /// 確認兩元素，是否為鄰近對象
    /// </summary>
    /// <param name="A">元素A</param>
    /// <param name="B">元素B</param>
    /// <returns>是 or 否</returns>
    public static bool CheckNearElement(this Element A, Element B)
    {
        return Vector3.Distance(A.grid.pos, B.grid.pos) <= 1;
    }

    /// <summary>
    /// 兩元素互換位置
    /// </summary>
    /// <param name="A">元素A</param>
    /// <param name="B">元素B</param>
    public static void ChangeGrid(this Element A, Element B)
    {
        //紀錄下A的位置
        Grid aGrid = A.grid;
        //A位置設成B
        A.SetGrid(B.grid);
        //再把B位置設成紀錄的A
        B.SetGrid(aGrid);
    }
    #endregion

    #region 周邊元素存在確認
    /// <summary>
    /// 檢查設定中心右方是否有元素物件
    /// </summary>
    /// <param name="grids">網格</param>
    /// <param name="center">中心元素</param>
    /// <returns>元素物件</returns>
    public static Element RightElement(this Grid[][] grids, Element center)
    {
        //檢查是否超出範圍 -> 中心物件的 X+1 大於等於陣列長度
        bool outRange = center.grid.x + 1 >= grids.Length;
        if (!outRange)
        {//在範圍內 -> 對應元素
            Debug.LogWarning("→●");
            return grids[center.grid.x + 1][center.grid.y].element;
        }
        else
        {//超出範圍 -> 無
            Debug.LogWarning("→○");
            return null;
        }
    }

    /// <summary>
    /// 檢查設定中心左方是否有元素物件
    /// </summary>
    /// <param name="grids">網格</param>
    /// <param name="center">中心元素</param>
    /// <returns>元素物件</returns>
    public static Element LeftElement(this Grid[][] grids, Element center)
    {
        //檢查是否超出範圍 -> 中心物件的 X-1 小於0
        bool outRange = center.grid.x - 1 < 0;
        if (!outRange)
        {//在範圍內 -> 對應元素
            Debug.LogWarning("←●");
            return grids[center.grid.x - 1][center.grid.y].element;
        }
        else
        {//超出範圍 -> 無
            Debug.LogWarning("←○");
            return null;
        }
    }

    /// <summary>
    /// 檢查設定中心上方是否有元素物件
    /// </summary>
    /// <param name="grids">網格</param>
    /// <param name="center">中心元素</param>
    /// <returns>元素物件</returns>
    public static Element UpElement(this Grid[][] grids, Element center)
    {
        //檢查是否超出範圍 -> 中心物件的 Y+1 大於等於陣列長度
        bool outRange = center.grid.y + 1 >= grids[center.grid.x].Length;
        if (!outRange)
        {//在範圍內 -> 對應元素
            Debug.LogWarning("↑●");
            return grids[center.grid.x][center.grid.y + 1].element;
        }
        else
        {//超出範圍 -> 無
            Debug.LogWarning("↑○");
            return null;
        }
    }

    /// <summary>
    /// 檢查設定中心下方是否有元素物件
    /// </summary>
    /// <param name="grids">網格</param>
    /// <param name="center">中心元素</param>
    /// <returns>元素物件</returns>
    public static Element DownElement(this Grid[][] grids, Element center)
    {
        //檢查是否超出範圍 -> 中心物件的 Y-1 小於0
        bool outRange = center.grid.y - 1 < 0;
        if (!outRange)
        {//在範圍內 -> 對應元素
            Debug.LogWarning("↓●");
            return grids[center.grid.x][center.grid.y - 1].element;
        }
        else
        {//超出範圍 -> 無
            Debug.LogWarning("↓○");
            return null;
        }
    }

    /// <summary>
    /// 檢查設定中心周圍是否有元素物件(依照指定的方位)
    /// </summary>
    /// <param name="grids">網格</param>
    /// <param name="center">中心元素</param>
    /// <param name="direction">指定的方位</param>
    /// <returns>元素物件</returns>
    public static Element AroundElement(this Grid[][] grids, Element center, Direction direction)
    {
        //利用 Switch(參數) 切換對應的 Case 程式區塊，
        //一般情況用 break 去切割(中斷程式)區塊，若使用 return 則免
        switch (direction)
        {
            case Direction.RIGHT:
                return RightElement(grids, center);
            //break;
            case Direction.LEFT:
                return LeftElement(grids, center);
            //break;
            case Direction.UP:
                return UpElement(grids, center);
            //break;
            case Direction.DOWN:
                return DownElement(grids, center);
            //break;
            default:
                return null;
        }
    }
    #endregion

    #region 元素消除邏輯工具
    public static Direction DirChecker(this Element select, Element target)
    {
        if (target.grid.x > select.grid.x)
        {//目的地往右：從左來(剔除左側比對)
            return Direction.LEFT;
        }
        else if (target.grid.x < select.grid.x)
        {//目的地往左：從右來(剔除右側比對)
            return Direction.RIGHT;
        }
        else if (target.grid.y > select.grid.y)
        {//目的地往上：從下來(剔除下方比對)
            return Direction.DOWN;
        }
        //目的地往下：從上來(剔除上方比對)
        else return Direction.UP;
    }

    /// <summary>
    /// 消除路徑檢查
    /// </summary>
    /// <param name="direction">檢查方位</param>
    /// <param name="element">對象元素</param>
    /// <param name="type">比對元素類型</param>
    /// <returns>是 or 否</returns>
    public static bool PathChecker(this Direction direction, Element element, ElementType type)
    {
        if (element && element.type == type)
        {
            //Debug.Log($"{element.name} {direction} 運算");
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion
}