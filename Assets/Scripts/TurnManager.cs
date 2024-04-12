﻿using System.Collections;
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
    public int dayDuration;
    public int nightDuration;
    public int teamCount;
    public int currentTeam; // Номер команды, которая сейчас ходит
    public Dictionary<int, string> teamsDict = new Dictionary<int, string>(); // Словарь, ключи в котором - это номера команд, а значения - их названия

    [SerializeField] private TextMeshProUGUI _turnText;
    [SerializeField] private TextMeshProUGUI _currentTeamText;
    [SerializeField] private TextMeshProUGUI _remainingTimeText;
    [SerializeField] private TextMeshProUGUI _timeOfDayText;



    void Start()
    {
        FirstTurnStart();
    }

    void Update()
    {
        remainingTime -= Time.deltaTime;
        if (remainingTime < 0) NextTeam();
        _remainingTimeText.SetText("Remaining Time: " + Mathf.FloorToInt(remainingTime).ToString());
    }

    private void NextTeam()
    {
        currentTeam += 1;
        _currentTeamText.SetText("Current Team: " + teamsDict[currentTeam]);
        remainingTime = timeToTurn;
        if (currentTeam > teamCount)
        {
            NextTurn();
        }
    }
    private void NextTurn()
    {
        onTurnChanged?.Invoke();
        currentTeam = 1;
        turn += 1;
        _currentTeamText.SetText("Current Team: " + teamsDict[currentTeam]);
        _turnText.SetText("Turn: " + (turn +1).ToString());
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
    private void FirstTurnStart()
    {
        dayDuration -= 1; // Чтобы рассчёты работали нормально, на самом деле продолжительность дня будет той, которую мы ввели
        turn = 0; // На экран будем выводить: turn + 1
        _timeOfDayText.SetText("Day");
        currentTeam = 1;
        TeamsNameInitialization();
        _currentTeamText.SetText("Current Team: " + teamsDict[currentTeam]);
        remainingTime = timeToTurn;
        _turnText.SetText("Turn: " + (turn + 1).ToString());
    }
    private void TeamsNameInitialization() // В будущем будет какая-нибудь менюшка, в которую игрок будет вводить название своей команды, но пока её нет, будет эта затычка
    {
        teamsDict[1] = "First Team";
        teamsDict[2] = "Second Team";
        teamsDict[3] = "Third Team";
        teamsDict[4] = "Fourth Team";
        teamsDict[5] = "Fifth Team";
    }
}
