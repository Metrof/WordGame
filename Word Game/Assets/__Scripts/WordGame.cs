using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum GameMode
{
    preGame,//����� ������� ����
    loading,//������ ���� ����������� � �������������
    makeLevel,//��������� ��������� WordLevel
    levelPrep,//��������� ������� � ���������� ��������������
    inLevel//������� �������
}
public class WordGame : MonoBehaviour
{
    public static WordGame S;

    [Header("Set in Inspector")]
    public GameObject prefabLetter;
    public Rect wordArea = new Rect(-24, 19, 48, 28);
    public float letterSize = 1.5f;
    public bool showAllWyrds = true;
    public float bigLetterSize = 4f;
    public Color bigColorDim = new Color(0.8f, 0.8f, 0.8f);
    public Color bigColorSelected = new Color(1f, 0.9f, 0.7f);
    public Vector3 bigLetterCenter = new Vector3(0, -16, 0);
    public Color[] wyrdPalette;

    [Header("Set Dinamically")]
    public GameMode mode = GameMode.preGame;
    public WordLevel currLevel;
    public List<Wyrd> wyrds;
    public List<Letter> bigLetters;
    public List<Letter> bigLetterActive;
    public string testWord;
    public int lvl = 1;
    private string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private int myScore;
    private float lastCombo;

    private Transform letterAnchor, bigLetterAnchor;

    private void Awake()
    {
        S = this;
        letterAnchor = new GameObject("LetterAnchor").transform;
        bigLetterAnchor = new GameObject("BigLetterAnchor").transform;
    }

    private void Start()
    {
        mode = GameMode.loading;

        WordList.INIT();
    }

    public void WordListParseComplete()
    {
        mode = GameMode.makeLevel;
        //������� ������� � ��������� � currLevel ������� WordLevel
        currLevel = MakeWordLevel(lvl);
    }

    public WordLevel MakeWordLevel(int levelNum = -1)
    {
        WordLevel level = new WordLevel();
        if (levelNum == -1)
        {
            //������� ��������� �������
            level.longWordIndex = Random.Range(0, WordList.LONG_WORD_COUNT);
        } else
        {
            level.longWordIndex = levelNum;
        }
        level.levelNum = levelNum;
        level.word = WordList.GET_LONG_WORD(level.longWordIndex);
        level.charDict = WordLevel.MakeCharDick(level.word);

        StartCoroutine(FindSubWordsCoroutine(level));
        return level;
    }
    //�����������, ������������ �����, ������� ����� ��������� �� ���� ������
    public IEnumerator FindSubWordsCoroutine(WordLevel level)
    {
        level.subWords = new List<string>();
        string str;
        List<string> words = WordList.GET_WORDS();
        //��������� ����� ���� ���� � WordList
        for (int i = 0; i < WordList.WORD_COUNT; i++)
        {
            str = words[i];
            //���������, ����� �� ��� ��������� �� �������� � level.charDict
            if (WordLevel.CheckWordInLevel(str, level))
            {
                level.subWords.Add(str);
            }
            //�������������� ����� ������� ��������� ����� ���� � ���� �����
            if (i%WordList.NUM_TO_PARSE_BEFORE_YIELD == 0)
            {
                //�������������� �� ���������� �����
                yield return null;
            }
        }
        level.subWords.Sort();
        level.subWords = SortWordsByLength(level.subWords).ToList();
        //����������� ��������� ������, � ������ �������� SubWordSearchComplete()
        SubWordSearchComplete();
    }
    //������������ LINQ ��� ���������� ������� � ���������� ��� �����
    public static IEnumerable<string> SortWordsByLength(IEnumerable<string> ws)
    {
        ws = ws.OrderBy(s => s.Length);
        return ws;
    }
    public void SubWordSearchComplete()
    {
        mode = GameMode.levelPrep;
        Layout();//������� Layuot ���� ��� ����� ���������� WordSearch
    }

    void Layout()
    {
        //��������� �� ����� ������ � ������� ������� ���������� ����� �������� ������
        wyrds = new List<Wyrd>();
        //�������� ��������� ����������, ������� ����� ������������� �������
        GameObject go;
        Letter lett;
        string word;
        Vector3 pos;
        float left = 0;
        float columnWidth = 3;
        char c;
        Color color;
        Wyrd wyrd;

        //����������, ������� ����� ������ ��������� �� ������
        int numRows = Mathf.RoundToInt(wordArea.height / letterSize);
        //������� ��������� Wyrd ��� ������� ����� � level.subWords
        for (int i = 0; i < currLevel.subWords.Count; i++)
        {
            wyrd = new Wyrd();
            word = currLevel.subWords[i];
            //���� ����� �������, ��� columnWidth, ���������� ���
            columnWidth = Mathf.Max(columnWidth, word.Length);
            //������� ��������� Prefabletter ��� ������ ����� � �����
            for (int j = 0; j < word.Length; j++)
            {
                c = word[j];
                go = Instantiate(prefabLetter);
                go.transform.SetParent(letterAnchor);
                lett = go.GetComponent<Letter>();
                lett.c = c;
                //���������� ���������� ������ Letter
                pos = new Vector3(wordArea.x + left + j * letterSize, wordArea.y, 0);
                //�������� % �������� ��������� ������ �� ���������
                pos.y -= (i % numRows) * letterSize;
                //����������� ������ lett ���������� �� ������� ���� ������
                lett.posImmediate = pos + Vector3.up * (20 + i % numRows);
                //����� ������ �� ����������� � ����� ������� pos
                lett.pos = pos;
                //��������� lett.timeStart ��� ����������� ���� � ������ �������
                lett.timeStart = Time.time + i * 0.05f;
                go.transform.localScale = Vector3.one * letterSize;
                wyrd.Add(lett);
            }
            if (showAllWyrds) wyrd.visible = true;
            //���������� ���� ����� ������ �� ��� ������
            wyrd.color = wyrdPalette[word.Length - WordList.WORD_LENGTH_MIN];
            wyrds.Add(wyrd);
            //���� ��������� ��������� ��� � �������, ������ ����� �������
            if (i%numRows == numRows-1)
            {
                left += (columnWidth + 0.5f) * letterSize;
            }
        }
        //��������� �� ����� ������� ������ � �������
        //���������������� ������ ������� ����
        bigLetters = new List<Letter>();
        bigLetterActive = new List<Letter>();
        //������� ������� ������ ��� ������ ����� � ������� �����
        for (int i = 0; i < currLevel.word.Length; i++)
        {
            //���������� ��������� �������� ��������� ������
            c = currLevel.word[i];
            go = Instantiate(prefabLetter);
            go.transform.SetParent(bigLetterAnchor);
            lett = go.GetComponent<Letter>();
            lett.c = c;
            go.transform.localScale = Vector3.one * bigLetterSize;
            //������������� ��������� ������� ������ ���� ���� ������
            pos = new Vector3(0, -100, 0);
            lett.posImmediate = pos;
            lett.pos = pos;
            //��������� lett.TimeStart, ����� ������� ������ � ������� ��������� ����������
            lett.timeStart = Time.time + currLevel.subWords.Count * 0.05f;
            lett.easingCuve = Easing.Sin + "-0.18";//������� Easing
            color = bigColorDim;
            lett.color = color;
            lett.visible = true;//������ true ��� ������� ������
            lett.big = true;
            bigLetters.Add(lett);
        }
        //����������� ������
        bigLetters = ShuffleLetters(bigLetters);
        //������� �� �����
        ArrangeBigLetters();
        //���������� ����� mode -- " � ����"
        mode = GameMode.inLevel;
    }
    //���� ����� ������������ �������� � ������ List<Letter> � ���������� ���������
    List<Letter> ShuffleLetters(List<Letter> letters)
    {
        List<Letter> newL = new List<Letter>();
        int ndx;
        while (letters.Count > 0)
        {
            ndx = Random.Range(0, letters.Count);
            newL.Add(letters[ndx]);
            letters.RemoveAt(ndx);
        }
        return newL;
    }
    //���� ����� ������� ������� ������ �� �����
    void ArrangeBigLetters()
    {
        //����� �������� ��� ������ ���� ������� ������ � �������������� �� �����������
        float halfWidth = (bigLetters.Count) / 2f - 0.5f;
        Vector3 pos;
        for (int i = 0; i < bigLetters.Count; i++)
        {
            pos = bigLetterCenter;
            pos.x += (i - halfWidth) * bigLetterSize;
            bigLetters[i].pos = pos;
        }
        halfWidth = (bigLetterActive.Count) / 2f - 0.5f;
        for (int i = 0; i < bigLetterActive.Count; i++)
        {
            pos = bigLetterCenter;
            pos.x += (i - halfWidth) * bigLetterSize;
            pos.y += bigLetterSize * 1.25f;
            bigLetterActive[i].pos = pos;
        }
    }

    private void Update()
    {
        //�������� ���� ��������������� ����������
        Letter ltr;
        char c;

        switch (mode)
        {
            case GameMode.preGame:
                break;
            case GameMode.loading:
                break;
            case GameMode.makeLevel:
                break;
            case GameMode.levelPrep:
                break;
            case GameMode.inLevel:
                //��������� ����� ���� ��������, ��������� ������� � ���� �����
                foreach (char cIt in Input.inputString)
                {
                    //������������� � ������� �������
                    c = char.ToUpperInvariant(cIt);
                    //���������, ���� �� ����� ����� �������� ��������
                    if (upperCase.Contains(c))//����� ����� �������� ��������
                    {
                        //����� ��������� ������ � ���� ������ � bigLetters
                        ltr = FindNextLetterByChar(c);
                        //���� ������ �������
                        if (ltr != null)
                        {
                            //�������� ���� ������ � textWord � ����������� ��������������� ������ Letter � biglettersActive
                            testWord += c.ToString();
                            //����������� �� ������ ���������� � ������ �������� ������
                            bigLetterActive.Add(ltr);
                            bigLetters.Remove(ltr);
                            ltr.color = bigColorSelected;//������� ������ �������� ���
                            ArrangeBigLetters();
                        }
                    }
                    if (c == '\b')//backspase
                    {
                        //������� ��������� ������ Letter �� biglettersActive
                        if (bigLetterActive.Count == 0) return;
                        if (testWord.Length > 1)
                        {
                            //������� ��������� ����� �� testWord
                            testWord = testWord.Substring(0, testWord.Length - 1);
                        } else
                        {
                            testWord = "";
                        }
                        ltr = bigLetterActive[bigLetterActive.Count - 1];
                        //����������� �� ������ �������� � ������ ���������� ������
                        bigLetterActive.Remove(ltr);
                        bigLetters.Add(ltr);
                        ltr.color = bigColorDim;//������� ������ ���������� ���
                        ArrangeBigLetters();//���������� ������
                    }
                    if (c == '\n' || c == '\r')//Return/Enter
                    {
                        //��������� ������� ������������������ ����� � WordLevel
                        CheckWord();
                    }
                    if (c == ' ')//������
                    {
                        //���������� ������ � bigletters
                        bigLetters = ShuffleLetters(bigLetters);
                        ArrangeBigLetters();
                    }
                }
                break;
            default:
                break;
        }
    }
    //���� ����� ���������� ������ Letter � �������� � � bigLetters
    //���� ����� ������ ���, ���������� null
    Letter FindNextLetterByChar(char c)
    {
        //��������� ������ ������ Letter � bigLetters
        foreach (Letter ltr in bigLetters)
        {
            //���� �������� ��� �� ������, ��� ������ � �
            if (ltr.c == c)
            {
                //...������� ��
                return ltr;
            }
        }
        return null;//����� null
    }

    public void CheckWord()
    {
        //��������� ����������� ����� testWord � ������ level.subWords
        string subWord;
        bool foundTestWord = false;
        //������� ������ List<int> ��� �������� �������� ������ ����, ������������� � testWords
        List<int> containedWords = new List<int>();
        for (int i = 0; i < currLevel.subWords.Count; i++)
        {
            //���������,���� �� ��� ������� Wyrd
            if (wyrds[i].found)
            {
                continue;
            }
            subWord = currLevel.subWords[i];
            //���������,������� �� ��� ����� subWord � testWord
            if (string.Equals(testWord, subWord))
            {
                HighlightWyrd(i);
                ScoreManager.SCORE(wyrds[i], 1);//���������� ����
                myScore += wyrds[i].letters.Count;
                foundTestWord = true;
            }
            else if (testWord.Contains(subWord))
            {
                containedWords.Add(i);
            }
        }
        if (foundTestWord)//���� ����������� ����� ������������ � ������
        {//...���������� ������ �����, ������������ � testWord
            int numContained = containedWords.Count;
            int ndx;
            //������������ ����� � �������� �������
            for (int i = 0; i < containedWords.Count; i++)
            {
                ndx = numContained - i - 1;
                HighlightWyrd(containedWords[ndx]);
                ScoreManager.SCORE(wyrds[containedWords[ndx]], i + 2);
                myScore += wyrds[containedWords[ndx]].letters.Count * (i + 2);
                lastCombo = i;
            }
        }
        CheckLevelComplete();
        //�������� ������ �������� ������ letters ���������� �� ����, �������� �� testWord ����������
        ClearBigLettersActive();
    }
    //������������ ��������� Wyrd
    void HighlightWyrd(int ndx)
    {
        //������������ �����
        wyrds[ndx].found = true;//����������� ������� ��� ��� �������
        //�������� ������
        wyrds[ndx].color = (wyrds[ndx].color + Color.white) / 2f;
        wyrds[ndx].visible = true;//������� ��������� 3D text �������
    }
    //������� ��� ������ Letters �� bigLetterActive
    void ClearBigLettersActive()
    {
        testWord = "";//�������� testWord
        foreach (Letter ltr in bigLetterActive)
        {
            bigLetters.Add(ltr);//�������� ������ ������ � bigLetters
            ltr.color = bigColorDim;//������� �� ���������� ���
        }
        bigLetterActive.Clear();//�������� ������
        ArrangeBigLetters();//�������� ������� ������ �� �����
    }

    void CheckLevelComplete()
    {
        bool allWyrdsTrue = true;

        for (int i = 0; i < wyrds.Count; i++)
        {
            if (!wyrds[i].found)
            {
                allWyrdsTrue = false;
                continue;
            }
        }
        if (myScore >= 70 || allWyrdsTrue)
        {
            mode = GameMode.loading;
            float timeToRest = 2.5f + (1.2f * lastCombo);
            Invoke("LevelComplete", timeToRest);
        }
    }

    void LevelComplete()
    {
        Letter delLet;
        for (int i = 0; i < wyrds.Count; i++)
        {
            for (int j = 0; j < wyrds[i].letters.Count; j++)
            {
                delLet = wyrds[i].letters[j];
                delLet.DestroyLet();
            }
        }
        for (int i = 0; i < bigLetters.Count; i++)
        {
            delLet = bigLetters[i];
            delLet.DestroyLet();
        }
        wyrds = null;
        bigLetters = null;
        myScore = 0;
        StartNewLvl();
    }

    void StartNewLvl()
    {
        mode = GameMode.makeLevel;
        lvl++;
        currLevel = MakeWordLevel(lvl);
    }
}
