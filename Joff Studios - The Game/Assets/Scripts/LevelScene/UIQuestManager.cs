using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIQuestManager : MonoBehaviour
{
    public GameObject questParent;
    public List<GraveGhost> GraveGhosts = new List<GraveGhost>();
    public GameObject UIQuestPrefab;
    private List<UIQuest> Quests;

    int offset = 0;

    public GameObject QuestScrollMiddle;
    public GameObject QuestScrollBottom;

    public void CreateUIQuests()
    {
        Quests = new List<UIQuest>();
        foreach (GraveGhost ghost in GraveGhosts)
        {
            if (ghost.hasQuest)
            {
                GameObject quest = Instantiate(UIQuestPrefab);
                Quests.Add(quest.GetComponent<UIQuest>());
                quest.transform.parent = questParent.transform;
                quest.GetComponent<UIQuest>().QuestName.text = ghost.QuestName;
                quest.GetComponent<UIQuest>().assignedGhost = ghost;
                quest.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                quest.SetActive(false);
            }
        }
    }
    public void CheckOffQuest(GraveGhost ghost)
    {
        foreach(UIQuest quest in Quests)
        {
            if(ghost.Equals(quest.assignedGhost))
            {
                if(!quest.gameObject.activeSelf)
                {
                    ShowQuest(ghost);
                }
                quest.Crossout.SetActive(true);
                break;
            }
        }
    }
    public void ShowQuest(GraveGhost ghost)
    {
        foreach (UIQuest quest in Quests)
        {
            if (ghost.Equals(quest.assignedGhost))
            {
                quest.GetComponent<RectTransform>().anchoredPosition = Vector2.zero + new Vector2(0, offset * -30);
                quest.gameObject.SetActive(true);
                QuestScrollMiddle.GetComponent<RectTransform>().sizeDelta = new Vector2(QuestScrollMiddle.GetComponent<RectTransform>().sizeDelta.x, (offset + 1) * 30);
                QuestScrollBottom.GetComponent<RectTransform>().anchoredPosition += new Vector2(0, -30);
                offset++;
                break;
            }
        }
    }
}
