using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 網格物件
/// </summary>
public class Grid
{
    /// <summary>
    /// 網格 X 座標
    /// </summary>
    public int x
    {
        get;
        private set;
    }

    /// <summary>
    /// 網格 Y 座標
    /// </summary>
    public int y
    {
        get;
        private set;
    }

    /// <summary>
    /// 座標文字資訊 (只能讀取)
    /// </summary>
    public string posInfo
    {
        get
        {
            //return "座標：" + x + "," + y;
            return $"座標 (X:{x},Y:{y})";
        }
    }
    private Vector3 _pos;
    public Vector3 pos
    {
        get
        {
            _pos.x = x;
            _pos.y = y;
            _pos.z = 0;
            return _pos;
        }
    }
    /// <summary>
    /// 當前位於格點上的元素
    /// </summary>
    public Element element
    {
        get; private set;
    }
    /// <summary>
    /// 網格是否為空：無所屬元素
    /// </summary>
    public bool isEmpty
    {
        get { return element == null; }
    }

    /// <summary>
    /// 主系統
    /// </summary>
    private MainSystem mainSystem;

    /// <summary>
    /// 網格單元建構式 (設定 X , Y 座標參數)
    /// </summary>
    /// <param name="x">網格 X 值</param>
    /// <param name="y">網格 Y 值</param>
    public Grid(int x, int y, MainSystem mainSystem)
    {
        this.x = x;
        this.y = y;
        this.mainSystem = mainSystem;
    }

    /// <summary>
    /// 成為焦點功能
    /// </summary>
    public void Focus()
    {
        //在非可以操作(待命中)狀態下，禁止聚焦元素
        if (mainSystem.status == PlayingStatus.Waiting)
        {
            //通知系統：紀錄選取物件
            mainSystem.SelectElement(element);
        }
        else if (mainSystem.status == PlayingStatus.Operating)
        {
            //通知系統：操作選取物件到目標物件
            mainSystem.TargetElement(element);
        }

        //通知系統：移動選取框到自己身上
        mainSystem.MoveSelector(pos);
    }

    /// <summary>
    /// 設定位於格點上的元素
    /// </summary>
    /// <param name="element">要設定的元素</param>
    public void SetElement(Element element)
    {
        this.element = element;
    }

    /// <summary>
    /// 清除格點上的元素紀錄
    /// </summary>
    public void ClearElement()
    {
        element = null;
    }
}
