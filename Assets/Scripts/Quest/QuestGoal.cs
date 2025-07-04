using UnityEngine;

// ������������ ��� ����� �����
public enum GoalType
{
    Gather, // ������� ������� (��� ����� � ���������)
    Buy,    // ������ ���-��
    Earn,   // ���������� �����
    Use,    // ������������ �������/�������� ���-�� (���� �� ������������, �� �������)
    FeedAnimal
}

[System.Serializable]
public class QuestGoal
{
    public GoalType goalType;
    [Tooltip("ID ����. Gather/Buy: ItemData.name. FeedAnimal: AnimalData.speciesName.")]
    public string targetID;
    public int requiredAmount;

    [HideInInspector] public int currentAmount;

    public bool IsReached()
    {
        return (currentAmount >= requiredAmount);
    }

    public void UpdateProgress(int amount)
    {
        currentAmount = Mathf.Clamp(currentAmount + amount, 0, requiredAmount);
    }
}