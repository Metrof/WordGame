using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    static public Game S;

    [Header("Set In Inspector")]
    public GameObject flor;
    public GameObject player;
    public int florSize = 400;
    static public Transform playerPref;

    [Header("Set Dinamicaly")]
    public Transform florAnchor;

    private void Awake()
    {
        S = this;
        playerPref = player.transform;
    }

    private void Start()
    {
        if (GameObject.Find("_Flor") == null)
        {
            GameObject anchorGO = new GameObject("_Flor");
            anchorGO.transform.position = new Vector3(-10 , 0, -10);
            florAnchor = anchorGO.transform;
        }
        for (int i = 0; i < florSize; i++)
        {
            GameObject florGO = Instantiate(flor);
            Flor florClone = florGO.GetComponent<Flor>();
            florGO.transform.SetParent(florAnchor);
            florGO.transform.localPosition = new Vector3( i % 20 , 0 , i / 20 );
            florClone.mat = florClone.GetComponent<Renderer>().material;
            florClone.mat.mainTextureOffset = new Vector2((0.2f * i)%1, 0);
        }
    }

    private void FixedUpdate()
    {
        playerPref = player.transform;
    }

}
