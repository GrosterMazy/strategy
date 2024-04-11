using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class TurnManager : MonoBehaviour
{
    [NonSerialized] public static Action onTurnChanged;
    public int turn;
    public float timeToTurn;
    public float remainingTime;
    public int dayDuration; // На самом деле, продолжительность дня на 1 больше данного значения
    public int nightDuration;
    public int teamCount;
    public int currentTeam; // Номер команды, которая сейчас ходит

    [SerializeField] private TextMeshProUGUI _turnText;
    [SerializeField] private TextMeshProUGUI _currentTeamText;
    [SerializeField] private TextMeshProUGUI _remainingTimeText;
    [SerializeField] private TextMeshProUGUI _timeOfDayText;

    void Start()
    {
        turn = 0; // На экран будем выводить: turn + 1
        _timeOfDayText.SetText("Day");
        currentTeam = 1;
        _currentTeamText.SetText(currentTeam.ToString());
        remainingTime = timeToTurn;
        _turnText.SetText(turn+1.ToString());

    }

    void Update()
    {
        remainingTime -= Time.deltaTime;
        if (remainingTime < 0) NextTeam();
        _remainingTimeText.SetText(Mathf.Round(remainingTime).ToString());
    }

    private void NextTeam()
    {
        currentTeam += 1;
        _currentTeamText.SetText(currentTeam.ToString());
        remainingTime = timeToTurn;
        if (currentTeam > teamCount)
        {
            NextTurn();
        }
    }
    private void NextTurn()
    {
        currentTeam = 1;
        turn += 1;
        _currentTeamText.SetText(currentTeam.ToString());
        _turnText.SetText(turn+1.ToString());
        if (turn % (dayDuration + nightDuration + 1) - dayDuration > 0)
        {
            StartNight();
        }
        else if (turn % (dayDuration + nightDuration + 1) - dayDuration <= 0)
        {
            StartDay();
        }
    }
    private void StartNight()
    {
        _timeOfDayText.SetText("Night");
    }
    private void StartDay()
    {
        _timeOfDayText.SetText("Day");
    }
}
