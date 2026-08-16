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
    public Func<int> generateValue;

    public CommunityGoal(CommunityGoalType goalType, string name, string goalText, Func<List<Player>, int> calculateScore, Func<int> generateValue)
    {
        this.goalType = goalType;
        this.name = name;
        this.goalText = goalText;
        this.calculateScore = calculateScore;
        this.generateValue = generateValue;
        this.goalValue = 0;
    }

    public int CalculateScore(List<Player> players)
    {
        return calculateScore(players);
    }

    public void GenerateValue()
    {
        goalValue = generateValue();
    }
}