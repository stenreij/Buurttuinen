using System;
using System.Collections.Generic;

[System.Serializable]
public class CommunityGoal
{
    public CommunityGoalType goalType;
    public string name;
    public string goalText;
    public Func<List<Player>, int> calculateScore;
    public int goalValue;

    public CommunityGoal(CommunityGoalType goalType, string name, string goalText, Func<List<Player>, int> calculateScore, int goalValue)
    {
        this.goalType = goalType;
        this.name = name;
        this.goalText = goalText;
        this.calculateScore = calculateScore;
        this.goalValue = goalValue;
    }

    public int CalculateScore(List<Player> players)
    {
        return calculateScore(players);
    }
}