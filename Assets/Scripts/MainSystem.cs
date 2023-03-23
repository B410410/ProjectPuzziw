using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UnityEngine;


public class MainSystem : MonoBehaviour
{
    public PlayingStatus status
    {
        get
        {
            if (moveAction != null)
            {//移動動畫進行中
                return PlayingStatus.Moving;
            }
            else if (matchQueue.Count > 0)
            {//配對組數大於0：進行配對消除
                readyToDrop = true;
                return PlayingStatus.Matching;
            }
            else if (readyToDrop)
            {
                return PlayingStatus.ReadyToDrop;
            }
            else if (dropAction != null)
            {//移動動畫進行中
                return PlayingStatus.Droping;
            }
            else if (selectElement != null)
            {//已選取元素：操作中
                return PlayingStatus.Operating;
            }
            else return PlayingStatus.Waiting;
        }
    }
    private bool readyToDrop;

    #region 基本設定
    /// <summary>
    /// 網格二維陣列
    /// </summary>
    public Grid[][] grids;
    [Header("棋盤尺寸")]
    public int sizeX;
    public int sizeY;

    /// <summary>
    /// 元素的模板
    /// </summary>
    [Header("元素的模板")]
    public Element elementTmp;
    public Transform elementGroup;
    public Transform selector;
    #endregion

    #region 選取操作相關參數
    /// <summary>
    /// 選取狀態：是 or 否
    /// 依照是否有選取元素返回
    /// </summary>
    private bool isSelected
    {
        get
        {
            return status == PlayingStatus.Operating;
        }
    }
    /// <summary>
    /// 當前選取的元素物件
    /// </summary>
    public Element selectElement
    {
        get; private set;
    }
    /// <summary>
    /// 選取的元素物件移動來源(從哪個方位來)
    /// </summary>
    private Direction selectFrom;
    /// <summary>
    /// 目標元素物件(選取元素物件要交換的對象)
    /// </summary>
    public Element targetElement
    {
        get; private set;
    }
    /// <summary>
    /// 目標的元素物件移動來源(從哪個方位來)
    /// </summary>
    private Direction targetFrom;
    #endregion

    #region 消除配對等級相關參數
    /// <summary>
    /// 消除配對等級(資料實體，不要直接使用)
    /// 1LV：3連(直線)
    /// 2LV：4連(直線)、5連(直角)
    /// 3LV：5連(直線)
    /// </summary>
    private int _matchLevel;
    public int matchLevel
    {
        get
        {
            return _matchLevel;
        }
        set
        {
            _matchLevel = value;
            //關於等級的運算
            Debug.Log($"配對等級提升至 LV.{_matchLevel}");
        }
    }
    private MatchColor matchColor;

    #endregion

    public ElementDir[] elementDirs = new ElementDir[2];
    public Checker[] checker = new Checker[2];

    private Func<Element> moveAction;
    private Func<Element> dropAction;

    /// <summary>
    /// 配對後待消除的元素組合清單
    /// </summary>
    private Queue<Queue<Element>> matchQueue = new Queue<Queue<Element>>();

    //遊戲播放時，執行一次(初始化)
    void Start()
    {
        //物件池啟動(建立)
        ObjectPoolSystem.CreateElementDropPool(sizeX);

        //隱藏選取框
        ClearSelector();

        //建立網格
        CreateGrids(sizeX, sizeY);

        //建立元素
        for (int x = 0; x < sizeX; x++)
        {
            CreateElementsCol(x);
        }

        //配對用資料結構初始化
        matchColor.Initial();

        //設定交換用的檢查功能 選取~目標 依序檢查
        for (int i = 0; i < checker.Length; i++)
        {
            checker[i] = MatchChecker;
        }
    }

    #region 初始化相關功能
    /// <summary>
    /// 建立網格陣列 [ X ][ Y ]
    /// </summary>
    /// <param name="sizeX">X數量</param>
    /// <param name="sizeY">Y數量</param>
    public void CreateGrids(int sizeX, int sizeY)
    {
        //X軸數量建置
        grids = new Grid[sizeX][];

        //for迴圈 (起始值; 終點值; 迭代值)
        for (int x = 0; x < sizeX; x++)
        {
            //Y軸數量建置 (Y有幾格)
            grids[x] = new Grid[sizeY];

            for (int y = 0; y < sizeY; y++)
            {
                grids[x][y] = new Grid(x, y, this);
            }
        }
    }

    /// <summary>
    /// 建立直欄元素
    /// </summary>
    /// <param name="x">欄位號碼</param>
    public void CreateElementsCol(int x)
    {
        for (int y = 0; y < sizeY; y++)
        {
            //同色檢查
            //ColorChecker(x, y);
            //具現化"元素"
            Instantiate(elementTmp, elementGroup)
                .SetElement(GetRandomType(ColorChecker(x, y)), grids[x][y]);
        }
    }


    /// <summary>
    /// 取得兩軸向的排除清單
    /// </summary>
    /// <param name="x">X定位</param>
    /// <param name="y">Y定位</param>
    /// <returns>排除清單</returns>
    List<ElementType> ColorChecker(int x, int y)
    {
        //取得同色驗證結果 X 和 Y 軸
        ElementType xType = ColorCheckerX(x, y);
        ElementType yType = ColorCheckerY(x, y);
        List<ElementType> elements = new List<ElementType>();
        //如果同色加入排除清單內
        if (xType != ElementType.None) elements.Add(xType);
        //y 查到的顏色和 x 不同，才需要加入排除
        if (yType != ElementType.None && yType != xType) elements.Add(yType);
        return elements;
    }

    /// <summary>
    /// 比對X軸左側是否相鄰兩個相同元素
    /// </summary>
    /// <param name="x">X定位</param>
    /// <param name="y">Y定位</param>
    /// <returns>相同元素</returns>
    ElementType ColorCheckerX(int x, int y)
    {
        //初始化比對結果 = 無
        ElementType type = ElementType.None;
        //X軸 >= 2 需要比對
        if (x >= 2)
        {//需要往左比對 1-2 格，是否相同 type，相同的話變更比對結果
            if (grids[x - 1][y].element.type == grids[x - 2][y].element.type)
                type = grids[x - 1][y].element.type;
        }
        return type;
    }

    /// <summary>
    /// 比對Y軸左側是否相鄰兩個相同元素
    /// </summary>
    /// <param name="x">X定位</param>
    /// <param name="y">Y定位</param>
    /// <returns>相同元素</returns>
    ElementType ColorCheckerY(int x, int y)
    {
        //初始化比對結果 = 無
        ElementType type = ElementType.None;
        //Y軸 >= 2 需要比對
        if (y >= 2)
        {//需要往下比對 1-2 格，是否相同 type，相同的話變更比對結果
            if (grids[x][y - 1].element.type == grids[x][y - 2].element.type)
                type = grids[x][y - 1].element.type;
        }
        return type;
    }

    /// <summary>
    /// 取得隨機的元素(依照排除清單處理)
    /// </summary>
    /// <param name="elements">排除清單</param>
    /// <returns>元素類型</returns>
    ElementType GetRandomType(List<ElementType> elements)
    {
        if (elements.Count > 0)
        {//需要排除元素 (透過Linq工具轉換 Enum 數據)
            List<ElementType> randomList =
                new List<ElementType>(Enum.GetValues(typeof(ElementType)).Cast<ElementType>());
            //連發版的 if
            while (elements.Count > 0)
            {
                //Debug.Log(elements[0]);
                //要隨機的清單先替除，排除清單上的第一個物件
                randomList.Remove(elements[0]);
                //再把排除清單上的第一個物件也移除掉 (減少排除清單數量，為了迴圈停止)
                elements.RemoveAt(0);
            }

            return RandomType(randomList);
        }
        else
        {//不須排除，用原始清單處理
            return RandomType();
        }
    }

    /// <summary>
    /// 隨機產生元素類型
    /// </summary>
    /// <returns>元素類型</returns>
    ElementType RandomType()
    {
        //C#原生版的亂數功能：填入 (最小 , 最大) 參數，但最大值不會被產生
        //System.Random random = new System.Random();
        //int num = random.Next(1, 7);

        return (ElementType)UnityEngine.Random.Range(1, 7);
    }

    /// <summary>
    /// 排除過後的隨機元素
    /// </summary>
    /// <param name="elements">排除過的元素清單</param>
    /// <returns>元素類型</returns>
    ElementType RandomType(List<ElementType> elements)
    {
        int num = UnityEngine.Random.Range(1, elements.Count);
        return elements[num];
    }
    #endregion

    #region 操作選取相關功能
    /// <summary>
    /// 移動選取框功能(選取)
    /// </summary>
    /// <param name="pos">定位</param>
    public void MoveSelector(Vector3 pos)
    {
        if (isSelected)
        {
            //觸發選取移動選取框
            selector.position = pos;
        }
        else
        {
            //已選過清除選取
            ClearSelector();
        }
    }

    /// <summary>
    /// 紀錄選取物件
    /// </summary>
    /// <param name="element">選取的元素物件</param>
    public void SelectElement(Element element)
    {
        //未選取物件(執行選取)
        selectElement = element;
        //Debug.Log($"選取：{selectElement.name}。");
    }

    /// <summary>
    /// 操作選取物件到目標物件
    /// </summary>
    /// <param name="element">目標的元素物件</param>
    public void TargetElement(Element element)
    {
        //已有選取物件，設定目標(執行操作)
        targetElement = element;
        //Debug.Log($"目標：{targetElement.name}。");

        //比對目標是否為鄰近元素
        if (selectElement.CheckNearElement(targetElement))
        {
            //取得元素物件來源方位
            selectFrom = selectElement.DirChecker(targetElement);
            targetFrom = targetElement.DirChecker(selectElement);

            //Debug.Log($"交換檢查位置。");
            selectElement.ChangeGrid(targetElement);

            //比對開始
            //MatchChecker(selectElement, selectFrom);
            elementDirs[0].SetData(selectElement, selectFrom);
            //延後執行：先完成選取的配對
            //MatchChecker(targetElement, targetFrom);
            elementDirs[1].SetData(targetElement, targetFrom);

            if (StartMatchChecker())
            {//可執行交換
                AddMove(selectElement);
                AddMove(targetElement);
            }
            else
            {
                //Debug.Log($"換回檢查位置。");
                selectElement.ChangeGrid(targetElement);
                SelectDone();
            }
        }
        else
        {
            //Debug.Log($"不可操作{selectElement.name}→{targetElement.name}。");
            SelectDone();
        }
    }

    /// <summary>
    /// 清除選取框功能(清除)
    /// </summary>
    public void ClearSelector()
    {
        selector.position = Vector3.back * 10;
    }

    /// <summary>
    /// 選取操作完畢
    /// </summary>
    public void SelectDone()
    {
        selectElement = null;
        targetElement = null;
    }
    #endregion

    #region 配對邏輯相關
    /// <summary>
    /// 配對檢查流程
    /// 選取開始 -> 交換對象
    /// 依序做配對確認
    /// </summary>
    /// <returns>是否執行交換</returns>
    bool StartMatchChecker()
    {
        int totalLevel = 0;
        for (int i = 0; i < elementDirs.Length; i++)
        {
            totalLevel += checker[i](elementDirs[i].element, elementDirs[i].direction);
        }
        //Debug.Log(totalLevel > 0 ? "可執行交換" : "不可執行交換");
        return totalLevel > 0;
    }

    int MatchChecker(Element element, Direction from)
    {
        matchLevel = 0;
        matchColor.SetDefault(element);
        //Debug.Log($"{element.name} 從 {from} 移動過來");
        //紀錄中心元素類型
        //ElementType type = element.type;
        //確認四周是否進行運算(剔除來源方位不進行比對)
        for (int i = 0; i < 4; i++)
        {
            if (from != (Direction)i)
            {
                //Debug.Log((Direction)i); 剔除來的方向 
                if (CheckMatch(element, (Direction)i) == 2) matchLevel++;
            }
        }

        //橫直軸其一連線：配對等級 +1
        if (matchColor.V || matchColor.H) matchLevel++;

        if (matchLevel > 0)
        {
            //Debug.Log(matchColor.GetComboList().Count);
            matchQueue.Enqueue(matchColor.GetComboList());
            Debug.Log(matchQueue.Count);
        }
        return matchLevel;
    }

    /// <summary>
    /// 左右 or 上下連線檢查(至少一顆)
    /// </summary>
    /// <param name="from">檢查方向</param>
    /// <param name="match">是否成立</param>
    void MatchVH(Direction from, bool match)
    {
        switch (from)
        {
            case Direction.UP:
                matchColor.up = match;
                break;
            case Direction.DOWN:
                matchColor.down = match;
                break;
            case Direction.LEFT:
                matchColor.left = match;
                break;
            case Direction.RIGHT:
                matchColor.right = match;
                break;
        }
    }
    /// <summary>
    /// 配對確認(四向)
    /// </summary>
    /// <param name="center">中心(操作對象)</param>
    /// <param name="from">比對方位</param>
    /// <returns>配對數量</returns>
    int CheckMatch(Element center, Direction from)
    {
        //預設未配對
        //bool match = false;
        //該方向同色數量
        int matchCount = 0;
        Element E1 = grids.AroundElement(center, from);
        if (from.PathChecker(E1, center.type))
        {
            //找到一顆
            matchCount++;
            matchColor.AddElement(E1);

            Element E2 = grids.AroundElement(E1, from);
            //找到兩顆
            if (from.PathChecker(E2, center.type))
            {
                matchCount++;
                matchColor.AddElement(E2);
            }
        }
        //至少1以上左右 or 上下連線紀錄
        //MatchVH(from, matchCount >= 1);
        if (matchCount >= 1) matchColor.SetMatch(from);
        return matchCount;
    }
    #endregion

    #region 元素交換移動相關
    /// <summary>
    /// 加入元素位移行動方法
    /// </summary>
    /// <param name="element">委託的元素</param>
    public void AddMove(Element element)
    {
        moveAction += element.MoveElement();
    }

    /// <summary>
    /// 移除元素位移行動方法
    /// </summary>
    /// <param name="element">委託的元素</param>
    public void RemoveMove(Element element)
    {
        if (element.onGrid)
        {
            moveAction -= element.MoveElement();
        }
    }

    /// <summary>
    /// 移動元素主功能
    /// </summary>
    private void MoveElement()
    {
        if (status == PlayingStatus.Moving)
        {
            //Debug.Log("移動中..");
            //moveAction();
            //RemoveMove(selectElement);
            //RemoveMove(targetElement);
            RemoveMove(moveAction());
            //移動完成後清除選取紀錄
            if (moveAction == null) SelectDone();
        }
    }
    #endregion

    #region 消除功能
    /// <summary>
    /// 清除(配對)元素主功能
    /// </summary>
    private void ClearUpMatchElement()
    {
        if (status == PlayingStatus.Matching)
        {
            if (matchQueue.Peek().Count == 0)
            {//當前運作的元素組，已清空：移除
                matchQueue.Dequeue();
            }
            else
            {
                //Debug.Log(matchQueue.Peek().Peek().name);

                //摧毀遊戲物件(元素)，執行消除特效
                if (matchQueue.Peek().Peek().DestroyElement())
                {
                    matchQueue.Peek().Peek().ClearGrid();
                    //從清單紀錄移除
                    matchQueue.Peek().Dequeue();
                }
                //Debug.Log(matchQueue.Count);
            }
        }
    }
    #endregion

    #region 掉落功能
    /// <summary>
    /// 掉落主功能
    /// </summary>
    void DropElements()
    {
        if (status == PlayingStatus.ReadyToDrop)
        {
            //未被消除，但可以墜落的元素補位
            GridScannerForDrop();
            //被消除過的元素補位
            this.ReadyToDrop();
            readyToDrop = false;
        }
        else if (status == PlayingStatus.Droping)
        {
            //開始位移掉落的元素
            Debug.Log("掉落中..");
            //dropAction();
            RemoveDropMove(dropAction());
        }
    }
    /// <summary>
    /// 網格掃描：執行可以墜落的元素補位
    /// </summary>
    void GridScannerForDrop()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                //搜索空格
                if (grids[x][y].isEmpty)
                {
                    //找到空格後，往後找尋可補位的元素
                    for (int i = y + 1; i < sizeY; i++)
                    {
                        if (!grids[x][i].isEmpty)
                        {
                            //元素重新設定網格(搶走後面格子的元素)
                            grids[x][i].element.SetGrid(grids[x][y]);
                            //被搶走元素的格子清除紀錄
                            grids[x][i].ClearElement();
                            //加入移動掉落Action
                            AddDropMove(grids[x][y].element);
                            break;
                        }
                    }
                }
            }
        }
    }
    /// <summary>
    /// 加入掉落移動的元素
    /// </summary>
    /// <param name="element">掉落元素</param>
    public void AddDropMove(Element element)
    {
        dropAction += element.MoveElement();
    }
    public void RemoveDropMove(Element element)
    {
        if (element.onGrid)
        {
            dropAction -= element.MoveElement();
        }
    }
    #endregion

    //遊戲運行中，每 1 幀(FPS)執行一次
    void Update()
    {
        Debug.Log(status);
        MoveElement();
        ClearUpMatchElement();
        DropElements();
    }

}
