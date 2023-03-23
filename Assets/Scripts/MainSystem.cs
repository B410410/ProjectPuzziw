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
            {//���ʰʵe�i�椤
                return PlayingStatus.Moving;
            }
            else if (matchQueue.Count > 0)
            {//�t��ռƤj��0�G�i��t�����
                readyToDrop = true;
                return PlayingStatus.Matching;
            }
            else if (readyToDrop)
            {
                return PlayingStatus.ReadyToDrop;
            }
            else if (dropAction != null)
            {//���ʰʵe�i�椤
                return PlayingStatus.Droping;
            }
            else if (selectElement != null)
            {//�w��������G�ާ@��
                return PlayingStatus.Operating;
            }
            else return PlayingStatus.Waiting;
        }
    }
    private bool readyToDrop;

    #region �򥻳]�w
    /// <summary>
    /// ����G���}�C
    /// </summary>
    public Grid[][] grids;
    [Header("�ѽL�ؤo")]
    public int sizeX;
    public int sizeY;

    /// <summary>
    /// �������ҪO
    /// </summary>
    [Header("�������ҪO")]
    public Element elementTmp;
    public Transform elementGroup;
    public Transform selector;
    #endregion

    #region ����ާ@�����Ѽ�
    /// <summary>
    /// ������A�G�O or �_
    /// �̷ӬO�_�����������^
    /// </summary>
    private bool isSelected
    {
        get
        {
            return status == PlayingStatus.Operating;
        }
    }
    /// <summary>
    /// ��e�������������
    /// </summary>
    public Element selectElement
    {
        get; private set;
    }
    /// <summary>
    /// ������������󲾰ʨӷ�(�q���Ӥ���)
    /// </summary>
    private Direction selectFrom;
    /// <summary>
    /// �ؼФ�������(�����������n�洫����H)
    /// </summary>
    public Element targetElement
    {
        get; private set;
    }
    /// <summary>
    /// �ؼЪ��������󲾰ʨӷ�(�q���Ӥ���)
    /// </summary>
    private Direction targetFrom;
    #endregion

    #region �����t�ﵥ�Ŭ����Ѽ�
    /// <summary>
    /// �����t�ﵥ��(��ƹ���A���n�����ϥ�)
    /// 1LV�G3�s(���u)
    /// 2LV�G4�s(���u)�B5�s(����)
    /// 3LV�G5�s(���u)
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
            //���󵥯Ū��B��
            Debug.Log($"�t�ﵥ�Ŵ��ɦ� LV.{_matchLevel}");
        }
    }
    private MatchColor matchColor;

    #endregion

    public ElementDir[] elementDirs = new ElementDir[2];
    public Checker[] checker = new Checker[2];

    private Func<Element> moveAction;
    private Func<Element> dropAction;

    /// <summary>
    /// �t���ݮ����������զX�M��
    /// </summary>
    private Queue<Queue<Element>> matchQueue = new Queue<Queue<Element>>();

    //�C������ɡA����@��(��l��)
    void Start()
    {
        //������Ұ�(�إ�)
        ObjectPoolSystem.CreateElementDropPool(sizeX);

        //���ÿ����
        ClearSelector();

        //�إߺ���
        CreateGrids(sizeX, sizeY);

        //�إߤ���
        for (int x = 0; x < sizeX; x++)
        {
            CreateElementsCol(x);
        }

        //�t��θ�Ƶ��c��l��
        matchColor.Initial();

        //�]�w�洫�Ϊ��ˬd�\�� ���~�ؼ� �̧��ˬd
        for (int i = 0; i < checker.Length; i++)
        {
            checker[i] = MatchChecker;
        }
    }

    #region ��l�Ƭ����\��
    /// <summary>
    /// �إߺ���}�C [ X ][ Y ]
    /// </summary>
    /// <param name="sizeX">X�ƶq</param>
    /// <param name="sizeY">Y�ƶq</param>
    public void CreateGrids(int sizeX, int sizeY)
    {
        //X�b�ƶq�ظm
        grids = new Grid[sizeX][];

        //for�j�� (�_�l��; ���I��; ���N��)
        for (int x = 0; x < sizeX; x++)
        {
            //Y�b�ƶq�ظm (Y���X��)
            grids[x] = new Grid[sizeY];

            for (int y = 0; y < sizeY; y++)
            {
                grids[x][y] = new Grid(x, y, this);
            }
        }
    }

    /// <summary>
    /// �إߪ��椸��
    /// </summary>
    /// <param name="x">��츹�X</param>
    public void CreateElementsCol(int x)
    {
        for (int y = 0; y < sizeY; y++)
        {
            //�P���ˬd
            //ColorChecker(x, y);
            //��{��"����"
            Instantiate(elementTmp, elementGroup)
                .SetElement(GetRandomType(ColorChecker(x, y)), grids[x][y]);
        }
    }


    /// <summary>
    /// ���o��b�V���ư��M��
    /// </summary>
    /// <param name="x">X�w��</param>
    /// <param name="y">Y�w��</param>
    /// <returns>�ư��M��</returns>
    List<ElementType> ColorChecker(int x, int y)
    {
        //���o�P�����ҵ��G X �M Y �b
        ElementType xType = ColorCheckerX(x, y);
        ElementType yType = ColorCheckerY(x, y);
        List<ElementType> elements = new List<ElementType>();
        //�p�G�P��[�J�ư��M�椺
        if (xType != ElementType.None) elements.Add(xType);
        //y �d�쪺�C��M x ���P�A�~�ݭn�[�J�ư�
        if (yType != ElementType.None && yType != xType) elements.Add(yType);
        return elements;
    }

    /// <summary>
    /// ���X�b�����O�_�۾F��ӬۦP����
    /// </summary>
    /// <param name="x">X�w��</param>
    /// <param name="y">Y�w��</param>
    /// <returns>�ۦP����</returns>
    ElementType ColorCheckerX(int x, int y)
    {
        //��l�Ƥ�ﵲ�G = �L
        ElementType type = ElementType.None;
        //X�b >= 2 �ݭn���
        if (x >= 2)
        {//�ݭn������� 1-2 ��A�O�_�ۦP type�A�ۦP�����ܧ��ﵲ�G
            if (grids[x - 1][y].element.type == grids[x - 2][y].element.type)
                type = grids[x - 1][y].element.type;
        }
        return type;
    }

    /// <summary>
    /// ���Y�b�����O�_�۾F��ӬۦP����
    /// </summary>
    /// <param name="x">X�w��</param>
    /// <param name="y">Y�w��</param>
    /// <returns>�ۦP����</returns>
    ElementType ColorCheckerY(int x, int y)
    {
        //��l�Ƥ�ﵲ�G = �L
        ElementType type = ElementType.None;
        //Y�b >= 2 �ݭn���
        if (y >= 2)
        {//�ݭn���U��� 1-2 ��A�O�_�ۦP type�A�ۦP�����ܧ��ﵲ�G
            if (grids[x][y - 1].element.type == grids[x][y - 2].element.type)
                type = grids[x][y - 1].element.type;
        }
        return type;
    }

    /// <summary>
    /// ���o�H��������(�̷ӱư��M��B�z)
    /// </summary>
    /// <param name="elements">�ư��M��</param>
    /// <returns>��������</returns>
    ElementType GetRandomType(List<ElementType> elements)
    {
        if (elements.Count > 0)
        {//�ݭn�ư����� (�z�LLinq�u���ഫ Enum �ƾ�)
            List<ElementType> randomList =
                new List<ElementType>(Enum.GetValues(typeof(ElementType)).Cast<ElementType>());
            //�s�o���� if
            while (elements.Count > 0)
            {
                //Debug.Log(elements[0]);
                //�n�H�����M��������A�ư��M��W���Ĥ@�Ӫ���
                randomList.Remove(elements[0]);
                //�A��ư��M��W���Ĥ@�Ӫ���]������ (��ֱư��M��ƶq�A���F�j�鰱��)
                elements.RemoveAt(0);
            }

            return RandomType(randomList);
        }
        else
        {//�����ư��A�έ�l�M��B�z
            return RandomType();
        }
    }

    /// <summary>
    /// �H�����ͤ�������
    /// </summary>
    /// <returns>��������</returns>
    ElementType RandomType()
    {
        //C#��ͪ����üƥ\��G��J (�̤p , �̤j) �ѼơA���̤j�Ȥ��|�Q����
        //System.Random random = new System.Random();
        //int num = random.Next(1, 7);

        return (ElementType)UnityEngine.Random.Range(1, 7);
    }

    /// <summary>
    /// �ư��L�᪺�H������
    /// </summary>
    /// <param name="elements">�ư��L�������M��</param>
    /// <returns>��������</returns>
    ElementType RandomType(List<ElementType> elements)
    {
        int num = UnityEngine.Random.Range(1, elements.Count);
        return elements[num];
    }
    #endregion

    #region �ާ@��������\��
    /// <summary>
    /// ���ʿ���إ\��(���)
    /// </summary>
    /// <param name="pos">�w��</param>
    public void MoveSelector(Vector3 pos)
    {
        if (isSelected)
        {
            //Ĳ�o������ʿ����
            selector.position = pos;
        }
        else
        {
            //�w��L�M�����
            ClearSelector();
        }
    }

    /// <summary>
    /// �����������
    /// </summary>
    /// <param name="element">�������������</param>
    public void SelectElement(Element element)
    {
        //���������(������)
        selectElement = element;
        //Debug.Log($"����G{selectElement.name}�C");
    }

    /// <summary>
    /// �ާ@��������ؼЪ���
    /// </summary>
    /// <param name="element">�ؼЪ���������</param>
    public void TargetElement(Element element)
    {
        //�w���������A�]�w�ؼ�(����ާ@)
        targetElement = element;
        //Debug.Log($"�ؼСG{targetElement.name}�C");

        //���ؼЬO�_���F�񤸯�
        if (selectElement.CheckNearElement(targetElement))
        {
            //���o��������ӷ����
            selectFrom = selectElement.DirChecker(targetElement);
            targetFrom = targetElement.DirChecker(selectElement);

            //Debug.Log($"�洫�ˬd��m�C");
            selectElement.ChangeGrid(targetElement);

            //���}�l
            //MatchChecker(selectElement, selectFrom);
            elementDirs[0].SetData(selectElement, selectFrom);
            //�������G������������t��
            //MatchChecker(targetElement, targetFrom);
            elementDirs[1].SetData(targetElement, targetFrom);

            if (StartMatchChecker())
            {//�i����洫
                AddMove(selectElement);
                AddMove(targetElement);
            }
            else
            {
                //Debug.Log($"���^�ˬd��m�C");
                selectElement.ChangeGrid(targetElement);
                SelectDone();
            }
        }
        else
        {
            //Debug.Log($"���i�ާ@{selectElement.name}��{targetElement.name}�C");
            SelectDone();
        }
    }

    /// <summary>
    /// �M������إ\��(�M��)
    /// </summary>
    public void ClearSelector()
    {
        selector.position = Vector3.back * 10;
    }

    /// <summary>
    /// ����ާ@����
    /// </summary>
    public void SelectDone()
    {
        selectElement = null;
        targetElement = null;
    }
    #endregion

    #region �t���޿����
    /// <summary>
    /// �t���ˬd�y�{
    /// ����}�l -> �洫��H
    /// �̧ǰ��t��T�{
    /// </summary>
    /// <returns>�O�_����洫</returns>
    bool StartMatchChecker()
    {
        int totalLevel = 0;
        for (int i = 0; i < elementDirs.Length; i++)
        {
            totalLevel += checker[i](elementDirs[i].element, elementDirs[i].direction);
        }
        //Debug.Log(totalLevel > 0 ? "�i����洫" : "���i����洫");
        return totalLevel > 0;
    }

    int MatchChecker(Element element, Direction from)
    {
        matchLevel = 0;
        matchColor.SetDefault(element);
        //Debug.Log($"{element.name} �q {from} ���ʹL��");
        //�������ߤ�������
        //ElementType type = element.type;
        //�T�{�|�P�O�_�i��B��(�簣�ӷ���줣�i����)
        for (int i = 0; i < 4; i++)
        {
            if (from != (Direction)i)
            {
                //Debug.Log((Direction)i); �簣�Ӫ���V 
                if (CheckMatch(element, (Direction)i) == 2) matchLevel++;
            }
        }

        //��b��@�s�u�G�t�ﵥ�� +1
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
    /// ���k or �W�U�s�u�ˬd(�ܤ֤@��)
    /// </summary>
    /// <param name="from">�ˬd��V</param>
    /// <param name="match">�O�_����</param>
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
    /// �t��T�{(�|�V)
    /// </summary>
    /// <param name="center">����(�ާ@��H)</param>
    /// <param name="from">�����</param>
    /// <returns>�t��ƶq</returns>
    int CheckMatch(Element center, Direction from)
    {
        //�w�]���t��
        //bool match = false;
        //�Ӥ�V�P��ƶq
        int matchCount = 0;
        Element E1 = grids.AroundElement(center, from);
        if (from.PathChecker(E1, center.type))
        {
            //���@��
            matchCount++;
            matchColor.AddElement(E1);

            Element E2 = grids.AroundElement(E1, from);
            //������
            if (from.PathChecker(E2, center.type))
            {
                matchCount++;
                matchColor.AddElement(E2);
            }
        }
        //�ܤ�1�H�W���k or �W�U�s�u����
        //MatchVH(from, matchCount >= 1);
        if (matchCount >= 1) matchColor.SetMatch(from);
        return matchCount;
    }
    #endregion

    #region �����洫���ʬ���
    /// <summary>
    /// �[�J�����첾��ʤ�k
    /// </summary>
    /// <param name="element">�e�U������</param>
    public void AddMove(Element element)
    {
        moveAction += element.MoveElement();
    }

    /// <summary>
    /// ���������첾��ʤ�k
    /// </summary>
    /// <param name="element">�e�U������</param>
    public void RemoveMove(Element element)
    {
        if (element.onGrid)
        {
            moveAction -= element.MoveElement();
        }
    }

    /// <summary>
    /// ���ʤ����D�\��
    /// </summary>
    private void MoveElement()
    {
        if (status == PlayingStatus.Moving)
        {
            //Debug.Log("���ʤ�..");
            //moveAction();
            //RemoveMove(selectElement);
            //RemoveMove(targetElement);
            RemoveMove(moveAction());
            //���ʧ�����M���������
            if (moveAction == null) SelectDone();
        }
    }
    #endregion

    #region �����\��
    /// <summary>
    /// �M��(�t��)�����D�\��
    /// </summary>
    private void ClearUpMatchElement()
    {
        if (status == PlayingStatus.Matching)
        {
            if (matchQueue.Peek().Count == 0)
            {//��e�B�@�������աA�w�M�šG����
                matchQueue.Dequeue();
            }
            else
            {
                //Debug.Log(matchQueue.Peek().Peek().name);

                //�R���C������(����)�A��������S��
                if (matchQueue.Peek().Peek().DestroyElement())
                {
                    matchQueue.Peek().Peek().ClearGrid();
                    //�q�M���������
                    matchQueue.Peek().Dequeue();
                }
                //Debug.Log(matchQueue.Count);
            }
        }
    }
    #endregion

    #region �����\��
    /// <summary>
    /// �����D�\��
    /// </summary>
    void DropElements()
    {
        if (status == PlayingStatus.ReadyToDrop)
        {
            //���Q�����A���i�H�Y���������ɦ�
            GridScannerForDrop();
            //�Q�����L�������ɦ�
            this.ReadyToDrop();
            readyToDrop = false;
        }
        else if (status == PlayingStatus.Droping)
        {
            //�}�l�첾����������
            Debug.Log("������..");
            //dropAction();
            RemoveDropMove(dropAction());
        }
    }
    /// <summary>
    /// ���汽�y�G����i�H�Y���������ɦ�
    /// </summary>
    void GridScannerForDrop()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                //�j���Ů�
                if (grids[x][y].isEmpty)
                {
                    //���Ů��A�����M�i�ɦ쪺����
                    for (int i = y + 1; i < sizeY; i++)
                    {
                        if (!grids[x][i].isEmpty)
                        {
                            //�������s�]�w����(�m���᭱��l������)
                            grids[x][i].element.SetGrid(grids[x][y]);
                            //�Q�m����������l�M������
                            grids[x][i].ClearElement();
                            //�[�J���ʱ���Action
                            AddDropMove(grids[x][y].element);
                            break;
                        }
                    }
                }
            }
        }
    }
    /// <summary>
    /// �[�J�������ʪ�����
    /// </summary>
    /// <param name="element">��������</param>
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

    //�C���B�椤�A�C 1 �V(FPS)����@��
    void Update()
    {
        Debug.Log(status);
        MoveElement();
        ClearUpMatchElement();
        DropElements();
    }

}
