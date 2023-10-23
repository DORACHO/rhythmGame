using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EJScoreManager : MonoBehaviour
{
    public static EJScoreManager instance;

    public TextMeshProUGUI textScore;
    int score;

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

    public int SCORE
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

    public int GetSCORE()
    {
        return score;
    }
}
