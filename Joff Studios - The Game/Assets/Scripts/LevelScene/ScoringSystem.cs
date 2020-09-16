using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoringSystem : MonoBehaviour
{
    public static ScoringSystem instance;

    [SerializeField]
    private List<GraveGhost> AllGraveGhosts;

    [SerializeField]
    private float maxScorePerGhost;
    [SerializeField]
    private float ScorePerCompletedQuest;
    [SerializeField]
    private float maxScoreForHappiness;

    [SerializeField]
    private float maxScore;
    [SerializeField]
    private float currentScore;

    private string grade = "F";

    private void Awake()
    {
        instance = this;
        maxScorePerGhost = ScorePerCompletedQuest + maxScoreForHappiness;
    }
    private void Start()
    {
        CalculateMaxScore();
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Z))
        {
            CalculateScore();
        }
    }
    public void InitAllGraves(List<Gravestone> graves)
    {
        foreach(Gravestone grave in graves)
        {
            AllGraveGhosts.Add(grave.GetComponentInChildren<GraveGhost>());
        }
    }

    public void CalculateMaxScore()
    {
        float score = 0f;
        foreach (GraveGhost ghost in AllGraveGhosts)
        {
            score += maxScorePerGhost;
            if(ghost.hasQuest)
            {
                score -= ScorePerCompletedQuest;
            }
        }
        maxScore = score;
    }

    public void CalculateScore()
    {
        float score = 0f;
        foreach(GraveGhost ghost in AllGraveGhosts)
        {
            if(ghost.QuestComplete && ghost.hasQuest)
            {
                score += ScorePerCompletedQuest;
            }
            score -= Mathf.Min(30, ghost.timesGraveWasDestroyed * 10);
            score += ghost.happiness.healthbar.fillAmount * maxScoreForHappiness;
        }
        currentScore = score;
    }
    public string CalculateGrade()
    {
        if(currentScore/maxScore <= 0.35f)
        {
            grade = "D";
        }
        else if(currentScore / maxScore <= 0.5f)
        {
            grade = "C";
        }
        else if (currentScore / maxScore <= 0.75f)
        {
            grade = "B";
        }
        else if (currentScore / maxScore <= 0.9f)
        {
            grade = "A";
        }
        else if(currentScore / maxScore <= 0.95f)
        {
            grade = "S";
        }
        else
        {
            grade = "S+";
        }
        return grade;
    }
}
