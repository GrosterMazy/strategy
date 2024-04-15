using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToughResources : ObjectOnGrid // Ресурсы, которые нужно добывать
{
    public string Name;
    public float WeightOfOneItem;
    public float actionsToGetPiece; // Стандартное Кол-во действий, которые должен потратить рабочий, чтобы получить некоторое кол-во ресурса
    public float actionsToBreak; // Кол-во действий, которые должен потратить рабочий, чтобы срубить дерево, или чтобы рудник истощился и тд
    public int piece;
    public int awardForBreak;
    private PlacementManager _placementManager => FindObjectOfType<PlacementManager>();
    private float _remainingActionsToGetPiece; // Оставшееся кол-во действий, которые должен потратить рабочий, чтобы получить некоторое кол-во ресурса

    private void Start()
    {
        _remainingActionsToGetPiece = actionsToGetPiece;
    }
    private void Update()
    {
        if (actionsToBreak <= 0)
        {
            Destroy(gameObject);
        }
    }
    public int ApplyDamage(float workerActions)
    {
        int countOfPiece = 0;
        _remainingActionsToGetPiece -= workerActions;
        while (_remainingActionsToGetPiece < 0)
        {
            _remainingActionsToGetPiece += actionsToGetPiece;
            if ((countOfPiece + 1) * actionsToGetPiece > actionsToBreak) break;
            countOfPiece++;
        }
        actionsToBreak -= workerActions;
        if (actionsToBreak <= 0)
        {
            return awardForBreak + countOfPiece * piece;
        }
        return countOfPiece * piece;
    }
}
