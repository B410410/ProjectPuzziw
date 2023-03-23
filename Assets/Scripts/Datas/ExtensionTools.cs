using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// �X�R�u��w (�R�A�}��)
/// </summary>
public static class ExtensionTools
{
    #region �������ʬ���
    /// <summary>
    /// �T�{�⤸���A�O�_���F���H
    /// </summary>
    /// <param name="A">����A</param>
    /// <param name="B">����B</param>
    /// <returns>�O or �_</returns>
    public static bool CheckNearElement(this Element A, Element B)
    {
        return Vector3.Distance(A.grid.pos, B.grid.pos) <= 1;
    }

    /// <summary>
    /// �⤸��������m
    /// </summary>
    /// <param name="A">����A</param>
    /// <param name="B">����B</param>
    public static void ChangeGrid(this Element A, Element B)
    {
        //�����UA����m
        Grid aGrid = A.grid;
        //A��m�]��B
        A.SetGrid(B.grid);
        //�A��B��m�]��������A
        B.SetGrid(aGrid);
    }
    #endregion

    #region �P�䤸���s�b�T�{
    /// <summary>
    /// �ˬd�]�w���ߥk��O�_����������
    /// </summary>
    /// <param name="grids">����</param>
    /// <param name="center">���ߤ���</param>
    /// <returns>��������</returns>
    public static Element RightElement(this Grid[][] grids, Element center)
    {
        //�ˬd�O�_�W�X�d�� -> ���ߪ��� X+1 �j�󵥩�}�C����
        bool outRange = center.grid.x + 1 >= grids.Length;
        if (!outRange)
        {//�b�d�� -> ��������
            Debug.LogWarning("����");
            return grids[center.grid.x + 1][center.grid.y].element;
        }
        else
        {//�W�X�d�� -> �L
            Debug.LogWarning("����");
            return null;
        }
    }

    /// <summary>
    /// �ˬd�]�w���ߥ���O�_����������
    /// </summary>
    /// <param name="grids">����</param>
    /// <param name="center">���ߤ���</param>
    /// <returns>��������</returns>
    public static Element LeftElement(this Grid[][] grids, Element center)
    {
        //�ˬd�O�_�W�X�d�� -> ���ߪ��� X-1 �p��0
        bool outRange = center.grid.x - 1 < 0;
        if (!outRange)
        {//�b�d�� -> ��������
            Debug.LogWarning("����");
            return grids[center.grid.x - 1][center.grid.y].element;
        }
        else
        {//�W�X�d�� -> �L
            Debug.LogWarning("����");
            return null;
        }
    }

    /// <summary>
    /// �ˬd�]�w���ߤW��O�_����������
    /// </summary>
    /// <param name="grids">����</param>
    /// <param name="center">���ߤ���</param>
    /// <returns>��������</returns>
    public static Element UpElement(this Grid[][] grids, Element center)
    {
        //�ˬd�O�_�W�X�d�� -> ���ߪ��� Y+1 �j�󵥩�}�C����
        bool outRange = center.grid.y + 1 >= grids[center.grid.x].Length;
        if (!outRange)
        {//�b�d�� -> ��������
            Debug.LogWarning("����");
            return grids[center.grid.x][center.grid.y + 1].element;
        }
        else
        {//�W�X�d�� -> �L
            Debug.LogWarning("����");
            return null;
        }
    }

    /// <summary>
    /// �ˬd�]�w���ߤU��O�_����������
    /// </summary>
    /// <param name="grids">����</param>
    /// <param name="center">���ߤ���</param>
    /// <returns>��������</returns>
    public static Element DownElement(this Grid[][] grids, Element center)
    {
        //�ˬd�O�_�W�X�d�� -> ���ߪ��� Y-1 �p��0
        bool outRange = center.grid.y - 1 < 0;
        if (!outRange)
        {//�b�d�� -> ��������
            Debug.LogWarning("����");
            return grids[center.grid.x][center.grid.y - 1].element;
        }
        else
        {//�W�X�d�� -> �L
            Debug.LogWarning("����");
            return null;
        }
    }

    /// <summary>
    /// �ˬd�]�w���ߩP��O�_����������(�̷ӫ��w�����)
    /// </summary>
    /// <param name="grids">����</param>
    /// <param name="center">���ߤ���</param>
    /// <param name="direction">���w�����</param>
    /// <returns>��������</returns>
    public static Element AroundElement(this Grid[][] grids, Element center, Direction direction)
    {
        //�Q�� Switch(�Ѽ�) ���������� Case �{���϶��A
        //�@�뱡�p�� break �h����(���_�{��)�϶��A�Y�ϥ� return �h�K
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

    #region ���������޿�u��
    public static Direction DirChecker(this Element select, Element target)
    {
        if (target.grid.x > select.grid.x)
        {//�ت��a���k�G�q����(�簣�������)
            return Direction.LEFT;
        }
        else if (target.grid.x < select.grid.x)
        {//�ت��a�����G�q�k��(�簣�k�����)
            return Direction.RIGHT;
        }
        else if (target.grid.y > select.grid.y)
        {//�ت��a���W�G�q�U��(�簣�U����)
            return Direction.DOWN;
        }
        //�ت��a���U�G�q�W��(�簣�W����)
        else return Direction.UP;
    }

    /// <summary>
    /// �������|�ˬd
    /// </summary>
    /// <param name="direction">�ˬd���</param>
    /// <param name="element">��H����</param>
    /// <param name="type">��露������</param>
    /// <returns>�O or �_</returns>
    public static bool PathChecker(this Direction direction, Element element, ElementType type)
    {
        if (element && element.type == type)
        {
            //Debug.Log($"{element.name} {direction} �B��");
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion
}