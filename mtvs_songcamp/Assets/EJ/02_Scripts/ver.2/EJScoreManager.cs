using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EJScoreManager : MonoBehaviour
{
    public static EJScoreManager instance;

    public TextMeshProUGUI textScore;
    float score;

    public Canvas canvas;
    public GameObject[] scoreTexts;

    SCORE_STATE scoreState;
    public enum SCORE_STATE
    {
        Excellent,  
        Great,
        Good,
        Bad,
        Miss
    }

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
        //이게 Update문에 있는게 맞나?
        //changeState로 하는 법
        //if (scoreState == SCORE_STATE.Excellent)
        //{
        //    showScoreText(0);
        //}
        //else if (scoreState == SCORE_STATE.Great)
        //{
        //    showScoreText(1);
        //}
        //else if (scoreState == SCORE_STATE.Good)
        //{
        //    showScoreText(2);
        //} else if (scoreState == SCORE_STATE.Bad)
        //{
        //    showScoreText(3);
        //}
        //else if (scoreState == SCORE_STATE.Miss)
        //{
        //    showScoreText(4);
        //}       
        
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
            textScore.text = "Score : " + score;
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

    void showScoreText(int n)
    {
        GameObject scoreText = Instantiate(scoreTexts[n], canvas.transform.position - Vector3.forward, Quaternion.identity);
        scoreText.transform.SetParent(canvas.transform);

        Destroy(scoreText, 0.5f);
    }
}
