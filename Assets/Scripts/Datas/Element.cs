using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Element : MonoBehaviour
{
    /// <summary>
    /// 圖片渲染器實體存放位置
    /// </summary>
    private SpriteRenderer _SR;
    /// <summary>
    /// 呼叫用的公開接口
    /// </summary>
    public SpriteRenderer SR
    {
        get
        {
            //圖片渲染器 == 空(null) => GetComponent抓取(只執行一次)
            if (_SR == null) _SR = GetComponent<SpriteRenderer>();
            return _SR;
        }
    }
    /// <summary>
    /// 元素類型
    /// </summary>
    public ElementType type
    {
        get; private set;
    }
    /// <summary>
    /// 元素Icon清單
    /// </summary>
    [SerializeField]
    private List<Sprite> icons = new List<Sprite>();

    /// <summary>
    /// 元素Icon清單
    /// </summary>
    [SerializeField]
    private List<Sprite> clearEffect = new List<Sprite>();
    private int effectIndex;

    /// <summary>
    /// 該元素的格點歸屬
    /// </summary>
    public Grid grid
    {
        get; private set;
    }

    /// <summary>
    /// 已在所屬格點座標
    /// 是否完全結束位移
    /// </summary>
    public bool onGrid
    {
        get
        {
            return transform.position == grid.pos;
        }
    }

    #region 設定元素資料
    /// <summary>
    /// 設定元素
    /// </summary>
    /// <param name="type">元素類型</param>
    /// <param name="pos">座標位置</param>
    public void SetElement(ElementType type, Grid grid)
    {
        //設定元素類型
        this.type = type;
        //設定格點位置
        SetGrid(grid);
        //設定物件位置
        transform.position = grid.pos;
        gameObject.name = $"{type} ({grid.pos.x},{grid.pos.y})";
        //Type轉數值取得圖片陣列編號
        SR.sprite = icons[(int)type];
    }

    public void SetPos(float x, float y, float z = 0)
    {
        transform.position = new Vector3(x, y, z);
    }
    /// <summary>
    /// 設定格點位置
    /// </summary>
    /// <param name="grid">格點</param>
    public void SetGrid(Grid grid)
    {
        //設定所屬格點
        this.grid = grid;
        //請格點紀錄該元素
        this.grid.SetElement(this);
    }
    #endregion

    #region 移動元素
    /// <summary>
    /// 移動元素到指定位置
    /// </summary>
    public Func<Element> MoveElement()
    {
        //變更物件名稱 => 和Type相同
        gameObject.name = $"{type} ({grid.pos.x},{grid.pos.y})";
        return MoveAction;
    }

    Element MoveAction()
    {
        //換位置
        transform.position =
            Vector3.Lerp(transform.position, grid.pos, Time.deltaTime * 15);
        //transform.position = grid.pos;
        return this;
    }
    #endregion

    #region 消除元素
    public bool DestroyElement()
    {
        //依序顯示特效圖片
        SR.sprite = clearEffect[effectIndex];
        effectIndex++;
        return effectIndex >= clearEffect.Count;
    }

    /// <summary>
    /// 清除格子紀錄，回收元素
    /// </summary>
    public void ClearGrid()
    {
        //回收至所屬直排編號的物件池
        this.AddToDropPool(grid.x);

        //清除格點對該元素的紀錄
        grid.ClearElement();

        //物件處於清除狀態(None)
        type = ElementType.Purple;
        //Type轉數值取得圖片陣列編號
        SR.sprite = icons[(int)type];

    }
    #endregion

    private void OnMouseDown()
    {
        //輸出：元素類型(座標)
        //Debug.Log(name);
        grid.Focus();
    }
}
