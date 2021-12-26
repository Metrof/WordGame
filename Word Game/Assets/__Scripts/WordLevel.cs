using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]//позволяет просматривать классы, которые не подключены к MonoBehaviour
public class WordLevel
{
    public int levelNum;
    public int longWordIndex;
    public string word;
    //Словарь со всеми буквами в слове
    public Dictionary<char, int> charDict;
    //Все слова, которые можно составить из букв в CharDick
    public List<string> subWords;
    //Статическая функция, подсчитывает количество вхождений символов в строку и возвращает словарь Dictionary<char, int> с этой информацией
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
    //Статический метод, проверяет возможность составить слово из символов в level.charDict
    public static bool CheckWordInLevel(string str, WordLevel level)
    {
        Dictionary<char, int> counts = new Dictionary<char, int>();
        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            //Если charDict содержит символ с
            if (level.charDict.ContainsKey(c))
            {
                //Если counts еще не содержит ключа с символом с
                if (!counts.ContainsKey(c))
                {
                    //Добавить новый ключ со значением 1
                    counts.Add(c, 1);
                } else
                {
                    //В противном случае прибавить 1 к текущемуу значению
                    counts[c]++;
                }
                //Если число вхождений превысило доступное кол-во в level.charDict
                if (counts[c] > level.charDict[c])
                {
                    //Вернуть false
                    return false;
                }
            }
            else
            {
                //Символ с отсутсвует в level.word, вернуть false
                return false;
            }
        } 
        return true;
    }
}
