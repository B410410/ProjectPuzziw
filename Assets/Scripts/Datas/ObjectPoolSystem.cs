using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ������t��
/// �^�����ƨϥΪ���
/// </summary>
public static class ObjectPoolSystem
{
    /// <summary>
    /// �^���A���������������(���Ƭ����)
    /// KEY�G�ƽs��
    /// VAL�G�����M��
    /// </summary>
    private static Dictionary<int, List<Element>> dropElements
        = new Dictionary<int, List<Element>>();
    /// <summary>
    /// �إߤ����^���M��d��
    /// </summary>
    /// <param name="sizeX">�Ƽƶq</param>
    public static void CreateElementDropPool(int sizeX)
    {
        for (int i = 0; i < sizeX; i++)
        {
            dropElements[i] = new List<Element>();
        }
    }
    /// <summary>
    /// (�X�R�\��)�����[�J�^���M��
    /// </summary>
    /// <param name="element">����(�X�R��H)</param>
    /// <param name="sizeX">�ƽs��</param>
    public static void AddToDropPool(this Element element, int sizeX)
    {
        dropElements[sizeX].Add(element);
    }
    /// <summary>
    /// �w�ƭ��s����
    /// </summary>
    public static void ReadyToDrop(this MainSystem ms)
    {
        //�C�@���ư���ƧǳB�z
        for (int x = 0; x < dropElements.Count; x++)
        {
            //�^���w�Ʊ������ƶq
            int dropCount = dropElements[x].Count;
            //���ƱƧ�
            for (int y = 0; y < dropElements[x].Count; y++)
            {
                //��m��ѽL�泻�ݱƶ�(�y��)
                dropElements[x][y].SetPos(x, y + ms.sizeY);
                //�]�w�n�e���� Grid ��H�A
                //�_�l���X�q(�`�� - �ʤf)�}�l�ɡA
                //�C���ɦ� + �ƶ����Ǹ��̧ǻ��W
                dropElements[x][y].SetGrid(ms.grids[x][ms.sizeY - dropCount + y]);
                //�[�J���ʱ���Action
                ms.AddDropMove(dropElements[x][y]);
            }
        }
    }

    public static void Droping()
    {

    }
}
