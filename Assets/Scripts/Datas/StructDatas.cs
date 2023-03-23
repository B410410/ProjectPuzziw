//struct : 可以額外把相關功能包成1份
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 周邊顏色配對資料結構
/// </summary>
public struct MatchColor
{
    private Element center;
    private List<Element> elements;
    public bool up;
    public bool down;
    public bool left;
    public bool right;

    /// <summary>
    /// 一直軸連線(同色)
    /// </summary>
    public bool V
    {
        get { return up && down; }
    }
    /// <summary>
    /// 一橫軸連線(同色)
    /// </summary>
    public bool H
    {
        get { return left && right; }
    }

    /// <summary>
    /// 配對用的結構初始化
    /// </summary>
    public void Initial()
    {
        center = null;
        elements = new List<Element>();
    }

    /// <summary>
    /// 初始化周邊同色紀錄初始化
    /// </summary>
    /// <param name="element">中心元素物件</param>
    public void SetDefault(Element element)
    {
        center = element;
        elements.Clear();
        elements.Add(center);
        up = false;
        down = false;
        left = false;
        right = false;
    }
    /// <summary>
    /// 加入檢查途中找到的同色元素
    /// </summary>
    /// <param name="element">同色元素</param>
    public void AddElement(Element element)
    {
        elements.Add(element);
    }

    /// <summary>
    /// 回傳構成消除條件的元素組合清單
    /// </summary>
    /// <returns>元素組合清單</returns>
    public Queue<Element> GetComboList()
    {
        //LogComboList();
        //建立集合物件時，就立刻帶入一組集合物
        return new Queue<Element>(elements);
    }

    void LogComboList()
    {
        string msg = "";
        foreach (Element element in elements)
        {
            msg += element.name;
        }
        Debug.Log(msg);
    }

    /// <summary>
    /// 設定指定方位是否同色
    /// </summary>
    /// <param name="from"></param>
    public void SetMatch(Direction from)
    {
        switch (from)
        {
            case Direction.UP:
                up = true;
                break;
            case Direction.DOWN:
                down = true;
                break;
            case Direction.LEFT:
                left = true;
                break;
            case Direction.RIGHT:
                right = true;
                break;
        }
    }
}

/// <summary>
/// 操作元素的條件資料結構
/// </summary>
public struct ElementDir
{
    public Element element;
    public Direction direction;

    public void SetData(Element e, Direction d)
    {
        element = e;
        direction = d;
    }
}