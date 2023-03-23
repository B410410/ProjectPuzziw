using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���檫��
/// </summary>
public class Grid
{
    /// <summary>
    /// ���� X �y��
    /// </summary>
    public int x
    {
        get;
        private set;
    }

    /// <summary>
    /// ���� Y �y��
    /// </summary>
    public int y
    {
        get;
        private set;
    }

    /// <summary>
    /// �y�Ф�r��T (�u��Ū��)
    /// </summary>
    public string posInfo
    {
        get
        {
            //return "�y�СG" + x + "," + y;
            return $"�y�� (X:{x},Y:{y})";
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
    /// ��e�����I�W������
    /// </summary>
    public Element element
    {
        get; private set;
    }
    /// <summary>
    /// ����O�_���šG�L���ݤ���
    /// </summary>
    public bool isEmpty
    {
        get { return element == null; }
    }

    /// <summary>
    /// �D�t��
    /// </summary>
    private MainSystem mainSystem;

    /// <summary>
    /// ����椸�غc�� (�]�w X , Y �y�аѼ�)
    /// </summary>
    /// <param name="x">���� X ��</param>
    /// <param name="y">���� Y ��</param>
    public Grid(int x, int y, MainSystem mainSystem)
    {
        this.x = x;
        this.y = y;
        this.mainSystem = mainSystem;
    }

    /// <summary>
    /// �����J�I�\��
    /// </summary>
    public void Focus()
    {
        //�b�D�i�H�ާ@(�ݩR��)���A�U�A�T��E�J����
        if (mainSystem.status == PlayingStatus.Waiting)
        {
            //�q���t�ΡG�����������
            mainSystem.SelectElement(element);
        }
        else if (mainSystem.status == PlayingStatus.Operating)
        {
            //�q���t�ΡG�ާ@��������ؼЪ���
            mainSystem.TargetElement(element);
        }

        //�q���t�ΡG���ʿ���ب�ۤv���W
        mainSystem.MoveSelector(pos);
    }

    /// <summary>
    /// �]�w�����I�W������
    /// </summary>
    /// <param name="element">�n�]�w������</param>
    public void SetElement(Element element)
    {
        this.element = element;
    }

    /// <summary>
    /// �M�����I�W����������
    /// </summary>
    public void ClearElement()
    {
        element = null;
    }
}
