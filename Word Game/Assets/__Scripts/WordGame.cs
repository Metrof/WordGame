using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum GameMode
{
    preGame,//перед началом игры
    loading,//Список слов загружается и анализируется
    makeLevel,//Создается отдельный WordLevel
    levelPrep,//Создается уровень с визуальным представлением
    inLevel//Уровень запущен
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
        //Создать уровень и сохранить в currLevel текущий WordLevel
        currLevel = MakeWordLevel(lvl);
    }

    public WordLevel MakeWordLevel(int levelNum = -1)
    {
        WordLevel level = new WordLevel();
        if (levelNum == -1)
        {
            //Выбрать случайный уровень
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
    //Сопрограмма, отыскивающая слова, которые можно составить на этом уровне
    public IEnumerator FindSubWordsCoroutine(WordLevel level)
    {
        level.subWords = new List<string>();
        string str;
        List<string> words = WordList.GET_WORDS();
        //Выполнить обход всех слов в WordList
        for (int i = 0; i < WordList.WORD_COUNT; i++)
        {
            str = words[i];
            //Проверять, можно ли его составить из символов в level.charDict
            if (WordLevel.CheckWordInLevel(str, level))
            {
                level.subWords.Add(str);
            }
            //Приостановится после анализа заданного числа слов в этом кадре
            if (i%WordList.NUM_TO_PARSE_BEFORE_YIELD == 0)
            {
                //Приостановится до следующего кадра
                yield return null;
            }
        }
        level.subWords.Sort();
        level.subWords = SortWordsByLength(level.subWords).ToList();
        //Сопрограмма завершила анализ, а потому вызываем SubWordSearchComplete()
        SubWordSearchComplete();
    }
    //Используется LINQ для сортировки массива и возвращает его копию
    public static IEnumerable<string> SortWordsByLength(IEnumerable<string> ws)
    {
        ws = ws.OrderBy(s => s.Length);
        return ws;
    }
    public void SubWordSearchComplete()
    {
        mode = GameMode.levelPrep;
        Layout();//Вызвать Layuot один раз после выполнения WordSearch
    }

    void Layout()
    {
        //поместить на экран плитки с буквами каждого возможного слова текущего уровня
        wyrds = new List<Wyrd>();
        //Обьявить локальные переменные, которые будут использоватся методом
        GameObject go;
        Letter lett;
        string word;
        Vector3 pos;
        float left = 0;
        float columnWidth = 3;
        char c;
        Color color;
        Wyrd wyrd;

        //Определить, сколько рядов плиток уместится на экране
        int numRows = Mathf.RoundToInt(wordArea.height / letterSize);
        //Создать экзембляр Wyrd для каждого слова в level.subWords
        for (int i = 0; i < currLevel.subWords.Count; i++)
        {
            wyrd = new Wyrd();
            word = currLevel.subWords[i];
            //Если слово длиннее, чем columnWidth, развернуть его
            columnWidth = Mathf.Max(columnWidth, word.Length);
            //Создать экзембляр Prefabletter для каждой буквы в слове
            for (int j = 0; j < word.Length; j++)
            {
                c = word[j];
                go = Instantiate(prefabLetter);
                go.transform.SetParent(letterAnchor);
                lett = go.GetComponent<Letter>();
                lett.c = c;
                //Установить координаты плитки Letter
                pos = new Vector3(wordArea.x + left + j * letterSize, wordArea.y, 0);
                //Оператор % помогает выстроить плитки по вертикали
                pos.y -= (i % numRows) * letterSize;
                //Переместить плитку lett немедленно за верхний край экрана
                lett.posImmediate = pos + Vector3.up * (20 + i % numRows);
                //Затем начать ее перемещение в новую позицию pos
                lett.pos = pos;
                //Увеличить lett.timeStart для перемещения слов в разные времена
                lett.timeStart = Time.time + i * 0.05f;
                go.transform.localScale = Vector3.one * letterSize;
                wyrd.Add(lett);
            }
            if (showAllWyrds) wyrd.visible = true;
            //Определить цвет слова исходя из его длинны
            wyrd.color = wyrdPalette[word.Length - WordList.WORD_LENGTH_MIN];
            wyrds.Add(wyrd);
            //Если достигнут последний ряд в столбце, начать новый столбец
            if (i%numRows == numRows-1)
            {
                left += (columnWidth + 0.5f) * letterSize;
            }
        }
        //Поместить на экран большие плитки с буквами
        //Инициализировать список больших букв
        bigLetters = new List<Letter>();
        bigLetterActive = new List<Letter>();
        //Создать большую плитку для каждой буквы в целевом слове
        for (int i = 0; i < currLevel.word.Length; i++)
        {
            //Напоминает процедуру создания маленьких плиток
            c = currLevel.word[i];
            go = Instantiate(prefabLetter);
            go.transform.SetParent(bigLetterAnchor);
            lett = go.GetComponent<Letter>();
            lett.c = c;
            go.transform.localScale = Vector3.one * bigLetterSize;
            //Первоначально поместить большие плитки ниже края экрана
            pos = new Vector3(0, -100, 0);
            lett.posImmediate = pos;
            lett.pos = pos;
            //Увеличить lett.TimeStart, чтобы большие плитки с буквами появились последними
            lett.timeStart = Time.time + currLevel.subWords.Count * 0.05f;
            lett.easingCuve = Easing.Sin + "-0.18";//упругое Easing
            color = bigColorDim;
            lett.color = color;
            lett.visible = true;//Всегда true для больших плиток
            lett.big = true;
            bigLetters.Add(lett);
        }
        //Перемещение плитки
        bigLetters = ShuffleLetters(bigLetters);
        //вывести на экран
        ArrangeBigLetters();
        //Установить режим mode -- " в игре"
        mode = GameMode.inLevel;
    }
    //Этот метод перемешивает элементы в списке List<Letter> и возвращает результат
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
    //Этот метод выводит большие плитки на экран
    void ArrangeBigLetters()
    {
        //Найти середину для вывода ряда больших плиток с центрированием по горизонтали
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
        //Обьявить пару вспомогательных переменных
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
                //Выполнить обход всех символов, введенных игроком в этом кадре
                foreach (char cIt in Input.inputString)
                {
                    //Преобразовать в верхний регистр
                    c = char.ToUpperInvariant(cIt);
                    //Проверить, есть ли такая буква верхнего регистра
                    if (upperCase.Contains(c))//Любая буква верхнего регистра
                    {
                        //найти доступную плитку с этой буквой в bigLetters
                        ltr = FindNextLetterByChar(c);
                        //Если плитка найдена
                        if (ltr != null)
                        {
                            //добавить этот символ в textWord и переместить соответствующую плитку Letter в biglettersActive
                            testWord += c.ToString();
                            //Переместить из списка неактивных в список активных плиток
                            bigLetterActive.Add(ltr);
                            bigLetters.Remove(ltr);
                            ltr.color = bigColorSelected;//Придать плитке активный вид
                            ArrangeBigLetters();
                        }
                    }
                    if (c == '\b')//backspase
                    {
                        //Удалить последнюю плитку Letter из biglettersActive
                        if (bigLetterActive.Count == 0) return;
                        if (testWord.Length > 1)
                        {
                            //Удалить последнюю букву из testWord
                            testWord = testWord.Substring(0, testWord.Length - 1);
                        } else
                        {
                            testWord = "";
                        }
                        ltr = bigLetterActive[bigLetterActive.Count - 1];
                        //Переместить из списка активных в список неактивных плиток
                        bigLetterActive.Remove(ltr);
                        bigLetters.Add(ltr);
                        ltr.color = bigColorDim;//Придать плитке неактивный вид
                        ArrangeBigLetters();//Отобразить плитки
                    }
                    if (c == '\n' || c == '\r')//Return/Enter
                    {
                        //Проверить наличие сконструированного слова в WordLevel
                        CheckWord();
                    }
                    if (c == ' ')//Пробел
                    {
                        //Перемешать плитки в bigletters
                        bigLetters = ShuffleLetters(bigLetters);
                        ArrangeBigLetters();
                    }
                }
                break;
            default:
                break;
        }
    }
    //Этот метод отыскивает плитку Letter с символом с в bigLetters
    //Если такой плитки нет, аозвращает null
    Letter FindNextLetterByChar(char c)
    {
        //Проверить каждую плитку Letter в bigLetters
        foreach (Letter ltr in bigLetters)
        {
            //Если содержит тот же символ, что указан в с
            if (ltr.c == c)
            {
                //...вернуть ее
                return ltr;
            }
        }
        return null;//Иначе null
    }

    public void CheckWord()
    {
        //Проверяет присутствие слова testWord в списке level.subWords
        string subWord;
        bool foundTestWord = false;
        //Создать список List<int> для хранения индексов других слов, присутсвующих в testWords
        List<int> containedWords = new List<int>();
        for (int i = 0; i < currLevel.subWords.Count; i++)
        {
            //Проверить,было ли уже найдено Wyrd
            if (wyrds[i].found)
            {
                continue;
            }
            subWord = currLevel.subWords[i];
            //Проверить,входило ли это слово subWord в testWord
            if (string.Equals(testWord, subWord))
            {
                HighlightWyrd(i);
                ScoreManager.SCORE(wyrds[i], 1);//Подсчитать очки
                myScore += wyrds[i].letters.Count;
                foundTestWord = true;
            }
            else if (testWord.Contains(subWord))
            {
                containedWords.Add(i);
            }
        }
        if (foundTestWord)//Если проверяемое слово присутствует в списке
        {//...подсветить другие слова, содержащиеся в testWord
            int numContained = containedWords.Count;
            int ndx;
            //Подсвечивать слова в обратном порядке
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
        //Очистить список активных плиток letters независимо от того, является ли testWord допустимым
        ClearBigLettersActive();
    }
    //Подсвечивает экзембляр Wyrd
    void HighlightWyrd(int ndx)
    {
        //Активировать слово
        wyrds[ndx].found = true;//Усстановить признак что оно найдено
        //выделить цветом
        wyrds[ndx].color = (wyrds[ndx].color + Color.white) / 2f;
        wyrds[ndx].visible = true;//Сделать компонент 3D text видимым
    }
    //Удаляет все плитки Letters из bigLetterActive
    void ClearBigLettersActive()
    {
        testWord = "";//очистить testWord
        foreach (Letter ltr in bigLetterActive)
        {
            bigLetters.Add(ltr);//добавить каждую плитку в bigLetters
            ltr.color = bigColorDim;//придать ей неактивный вид
        }
        bigLetterActive.Clear();//очистить список
        ArrangeBigLetters();//Повторно вывести плитки на экран
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
