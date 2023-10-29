using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EJScoreManager : MonoBehaviour
{
    public static EJScoreManager instance;

    public TextMeshProUGUI textScore;
    public TextMeshProUGUI numScore;
    public TextMeshProUGUI[] score4rails;

    float score;

    public Canvas canvas;
    //public GameObject[] scoreTexts;

    private void Awake()
    {
        instance = this;    
    }

    // Start is called before the first frame update
    void Start()
    {
        SCORE = 0;
    }

    // Update is called once per frame
    void Update()
    {
                   
    }

    public float SCORE
    {
        get
        {
            return score;
        }
        set
        {
            score = value;
            numScore.text = "Score : " + score;
        }
    }

    public void SetSCORE(int value)
    {
        score = value;
    }

    public float GetSCORE()
    {
        return score;
    }

    public IEnumerator showScoreText(string sss, int railIdx, int score)
    {

        //Ã³À½
        //GameObject scoreText = Instantiate(scoreTexts[n], canvas.transform.position - Vector3.forward, Quaternion.identity);
        //scoreText.transform.SetParent(canvas.transform);

        //Destroy(scoreText, 0.5f);
        textScore.text = sss + "!";

        if (!(sss == "Miss"))
        {
            score4rails[railIdx].text = "+" + score;
        }

        yield return 0.2f;
        score4rails[railIdx].text = "";

    }

    public void StartShowScoreText(string sss, int railIdx, int score)
    {
        StartCoroutine(showScoreText(sss, railIdx, score));
    }


}
