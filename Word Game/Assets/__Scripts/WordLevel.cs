using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]//��������� ������������� ������, ������� �� ���������� � MonoBehaviour
public class WordLevel
{
    public int levelNum;
    public int longWordIndex;
    public string word;
    //������� �� ����� ������� � �����
    public Dictionary<char, int> charDict;
    //��� �����, ������� ����� ��������� �� ���� � CharDick
    public List<string> subWords;
    //����������� �������, ������������ ���������� ��������� �������� � ������ � ���������� ������� Dictionary<char, int> � ���� �����������
    static public Dictionary<char, int> MakeCharDick(string w)
    {
        Dictionary<char, int> dict = new Dictionary<char, int>();
        char c;
        for (int i = 0; i < w.Length; i++)
        {
            c = w[i];
            if (dict.ContainsKey(c))
            {
                dict[c]++;
            } else
            {
                dict.Add(c, 1);
            }
        }
        return dict;
    }
    //����������� �����, ��������� ����������� ��������� ����� �� �������� � level.charDict
    public static bool CheckWordInLevel(string str, WordLevel level)
    {
        Dictionary<char, int> counts = new Dictionary<char, int>();
        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            //���� charDict �������� ������ �
            if (level.charDict.ContainsKey(c))
            {
                //���� counts ��� �� �������� ����� � �������� �
                if (!counts.ContainsKey(c))
                {
                    //�������� ����� ���� �� ��������� 1
                    counts.Add(c, 1);
                } else
                {
                    //� ��������� ������ ��������� 1 � ��������� ��������
                    counts[c]++;
                }
                //���� ����� ��������� ��������� ��������� ���-�� � level.charDict
                if (counts[c] > level.charDict[c])
                {
                    //������� false
                    return false;
                }
            }
            else
            {
                //������ � ���������� � level.word, ������� false
                return false;
            }
        } 
        return true;
    }
}
