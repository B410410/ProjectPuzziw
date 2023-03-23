using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Element : MonoBehaviour
{
    /// <summary>
    /// �Ϥ���V������s���m
    /// </summary>
    private SpriteRenderer _SR;
    /// <summary>
    /// �I�s�Ϊ����}���f
    /// </summary>
    public SpriteRenderer SR
    {
        get
        {
            //�Ϥ���V�� == ��(null) => GetComponent���(�u����@��)
            if (_SR == null) _SR = GetComponent<SpriteRenderer>();
            return _SR;
        }
    }
    /// <summary>
    /// ��������
    /// </summary>
    public ElementType type
    {
        get; private set;
    }
    /// <summary>
    /// ����Icon�M��
    /// </summary>
    [SerializeField]
    private List<Sprite> icons = new List<Sprite>();

    /// <summary>
    /// ����Icon�M��
    /// </summary>
    [SerializeField]
    private List<Sprite> clearEffect = new List<Sprite>();
    private int effectIndex;

    /// <summary>
    /// �Ӥ��������I�k��
    /// </summary>
    public Grid grid
    {
        get; private set;
    }

    /// <summary>
    /// �w�b���ݮ��I�y��
    /// �O�_���������첾
    /// </summary>
    public bool onGrid
    {
        get
        {
            return transform.position == grid.pos;
        }
    }

    #region �]�w�������
    /// <summary>
    /// �]�w����
    /// </summary>
    /// <param name="type">��������</param>
    /// <param name="pos">�y�Ц�m</param>
    public void SetElement(ElementType type, Grid grid)
    {
        //�]�w��������
        this.type = type;
        //�]�w���I��m
        SetGrid(grid);
        //�]�w�����m
        transform.position = grid.pos;
        gameObject.name = $"{type} ({grid.pos.x},{grid.pos.y})";
        //Type��ƭȨ��o�Ϥ��}�C�s��
        SR.sprite = icons[(int)type];
    }

    public void SetPos(float x, float y, float z = 0)
    {
        transform.position = new Vector3(x, y, z);
    }
    /// <summary>
    /// �]�w���I��m
    /// </summary>
    /// <param name="grid">���I</param>
    public void SetGrid(Grid grid)
    {
        //�]�w���ݮ��I
        this.grid = grid;
        //�Ю��I�����Ӥ���
        this.grid.SetElement(this);
    }
    #endregion

    #region ���ʤ���
    /// <summary>
    /// ���ʤ�������w��m
    /// </summary>
    public Func<Element> MoveElement()
    {
        //�ܧ󪫥�W�� => �MType�ۦP
        gameObject.name = $"{type} ({grid.pos.x},{grid.pos.y})";
        return MoveAction;
    }

    Element MoveAction()
    {
        //����m
        transform.position =
            Vector3.Lerp(transform.position, grid.pos, Time.deltaTime * 15);
        //transform.position = grid.pos;
        return this;
    }
    #endregion

    #region ��������
    public bool DestroyElement()
    {
        //�̧���ܯS�ĹϤ�
        SR.sprite = clearEffect[effectIndex];
        effectIndex++;
        return effectIndex >= clearEffect.Count;
    }

    /// <summary>
    /// �M����l�����A�^������
    /// </summary>
    public void ClearGrid()
    {
        //�^���ܩ��ݪ��ƽs���������
        this.AddToDropPool(grid.x);

        //�M�����I��Ӥ���������
        grid.ClearElement();

        //����B��M�����A(None)
        type = ElementType.Purple;
        //Type��ƭȨ��o�Ϥ��}�C�s��
        SR.sprite = icons[(int)type];

    }
    #endregion

    private void OnMouseDown()
    {
        //��X�G��������(�y��)
        //Debug.Log(name);
        grid.Focus();
    }
}
