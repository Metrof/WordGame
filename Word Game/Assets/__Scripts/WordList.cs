using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordList : MonoBehaviour
{
    private static WordList S;

    [Header("Set in Inspector")]
    public TextAsset wordListText;
    public int numToParseBeforeYield = 10000;
    public int wordLengthMin = 3;
    public int wordLengthMax = 7;

    [Header("Set Dinamically")]
    public int currLine = 0;
    public int totalLines;
    public int longWordCount;
    public int wordCount;
    //Скрыть поля
    private string[] lines;
    private List<string> longWords;
    private List<string> words;

    private void Awake()
    {
        S = this;
    }

    public void Init()
    {
        lines = wordListText.text.Split('\n');
        totalLines = lines.Length;

        StartCoroutine(ParseLines());
    }

    static public void INIT()
    {
        S.Init();
    }
    //Все сопрограммы возвращают значение типа IEnumerator
    public IEnumerator ParseLines()
    {
        string word;
        //Инициализировать список для хранения дальнейших слов из числа допустимых
        longWords = new List<string>();
        words = new List<string>();

        for (currLine = 0; currLine < totalLines; currLine++)
        {
            word = lines[currLine];
            //Если длина слова равнаwordLengthMax...
            if (word.Length == wordLengthMax)
            {
                longWords.Add(word);//Сохранить его в список больших слов
            }
            //Если длина слова между wordLengthMax и wordLengthMin...
            if (word.Length >= wordLengthMin && word.Length <= wordLengthMax)
            {
                words.Add(word);
            }
            //Определить, не пора ли сделать перерыв
            if (currLine % numToParseBeforeYield == 0)
            {
                //Посчитать слова в каждом списке, чтобы показать, как протекает процесс анализа
                longWordCount = longWords.Count;
                wordCount = words.Count;
                //Приостановить выполнение сопрограммы до следующего кадра
                yield return null;
                //Инструкция yield приостановит выполнение этого метода, даст возможность выполнится другому коду и возобновить
                //выполнение сопрограммы с этой точки, начав следующую итерацию цикла for
            }
        }
        longWordCount = longWords.Count;
        wordCount = words.Count;
        //послать gameObject сообщение об окончании анализа
        gameObject.SendMessage("WordListParseComplete");
    }
    //Эти методы позволяют другим классам обращатся к скрытым полям List<string>
    static public List<string> GET_WORDS()
    {
        return S.words;
    }
    static public string GET_WORD(int ndx)
    {
        return S.words[ndx];
    }
    static public List<string> GET_LONG_WORDS()
    {
        return S.longWords;
    }
    static public string GET_LONG_WORD(int ndx)
    {
        return S.longWords[ndx];
    }
    static public int WORD_COUNT
    {
        get { return S.wordCount; }
    }
    static public int LONG_WORD_COUNT
    {
        get { return S.longWordCount; }
    }
    static public int NUM_TO_PARSE_BEFORE_YIELD
    {
        get { return S.numToParseBeforeYield; }
    }
    static public int WORD_LENGTH_MIN
    {
        get { return S.wordLengthMin; }
    }
    static public int WORD_LENGTH_MAX
    {
        get { return S.wordLengthMax; }
    }
}
