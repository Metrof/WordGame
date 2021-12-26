using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Letter : MonoBehaviour
{
    [Header("Set in Inspector")]
    public float timeDiration = 0.5f;
    public string easingCuve = Easing.InOut;//Функция сглаживания из Utils.cs

    [Header("Set Dinamically")]
    public TextMesh tMesh;//TextMesh отображает символ
    public Renderer tRend;//Компонент Renderer обьекта 3D text. Он будет определять видимость символа
    public bool big = false;//Большие и малые плитки действуют по разному

    //Поля для линейной интерполяции
    public List<Vector3> pts = null;
    public float timeStart = -1;

    private char _c;//Символ, отображенный на плитке
    private Renderer rend;
    //Свойство для чтения/записи буквы в поле _c, отображаемой объектом 3D text

    private void Awake()
    {
        tMesh = GetComponentInChildren<TextMesh>();
        tRend = tMesh.GetComponent<Renderer>();
        rend = GetComponent<Renderer>();
        visible = false;
    }
    public char c
    {
        get { return _c; }
        set
        {
            _c = value;
            tMesh.text = _c.ToString();
        }
    }
    //Свойство для чтения/записи буквы в поле _c в виде строки
    public string str
    {
        get { return _c.ToString(); }
        set { c = value[0]; }
    }
    //Разрешает или запрещает отображение 3D text, что делает букву видимой или невидимой соответственно
    public bool visible
    {
        get { return tRend.enabled; }
        set { tRend.enabled = value; }
    }
    //свойство для чтения/записи цвета плитки
    public Color color
    {
        get { return rend.material.color; }
        set { rend.material.color = value; }
    }
    //свойство для чтения/записи координат плитки
    //Теперь настраиваем кривую Безье для плавного перемещения в новые координаты
    public Vector3 pos
    {
        set
        {
            //transform.position = value;
            //найти среднюю точку на случайном расстоянии от фактической средней точки между текущей и новой(value) позицией
            Vector3 mid = (transform.position + value) / 2f;
            //Случайное расстояние не превышает 1/4 расстояния до фактической средней точки
            float mag = (transform.position - value).magnitude;
            mid += Random.insideUnitSphere * mag * 0.25f;
            //Создать List<Vector3> точек, определяющих кривую Безье
            pts = new List<Vector3>() { transform.position, mid, value };
            //Если timeStart содержит значение по умолчанию -1, установить текущее время
            if (timeStart == -1) timeStart = Time.time;
        }
    }
    //Немедленно перемещает в новую позицию
    public Vector3 posImmediate
    {
        set
        {
            transform.position = value;
        }
    }
    //Код реализующий анимационный эффект
    private void Update()
    {
        if (timeStart == -1) return;
        //Стандартная линия интерполяции
        float u = (Time.time - timeStart) / timeDiration;
        u = Mathf.Clamp01(u);
        float u1 = Easing.Ease(u, easingCuve);
        Vector3 v = Utils.Bezier(u1, pts);
        transform.position = v;
        //Если интерполяция закончена, записать -1 в timeStart
        if (u == 1) timeStart = -1;
    }

    public void DestroyLet()
    {
        Destroy(gameObject);
    }
}
