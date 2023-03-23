//struct : �i�H�B�~������\��]��1��
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �P���C��t���Ƶ��c
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
    /// �@���b�s�u(�P��)
    /// </summary>
    public bool V
    {
        get { return up && down; }
    }
    /// <summary>
    /// �@��b�s�u(�P��)
    /// </summary>
    public bool H
    {
        get { return left && right; }
    }

    /// <summary>
    /// �t��Ϊ����c��l��
    /// </summary>
    public void Initial()
    {
        center = null;
        elements = new List<Element>();
    }

    /// <summary>
    /// ��l�ƩP��P�������l��
    /// </summary>
    /// <param name="element">���ߤ�������</param>
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
    /// �[�J�ˬd�~����쪺�P�⤸��
    /// </summary>
    /// <param name="element">�P�⤸��</param>
    public void AddElement(Element element)
    {
        elements.Add(element);
    }

    /// <summary>
    /// �^�Ǻc���������󪺤����զX�M��
    /// </summary>
    /// <returns>�����զX�M��</returns>
    public Queue<Element> GetComboList()
    {
        //LogComboList();
        //�إ߶��X����ɡA�N�ߨ�a�J�@�ն��X��
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
    /// �]�w���w���O�_�P��
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
/// �ާ@�����������Ƶ��c
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