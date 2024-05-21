﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class TurnManager : MonoBehaviour
{
    [NonSerialized] public static Action onTurnChanged;
    [NonSerialized] public static Action DayStarts;
    [NonSerialized] public static Action NightStarts;

    public bool isDay;
    public int turn;
    public float timeToTurn;
    public float remainingTime;
    public int dayDuration;
    public int nightDuration;
    public int dayDurationChange;
    public int nightDurationChange;
    public int teamCount;
    public int currentTeam; // Номер команды, которая сейчас ходит
    public Dictionary<int, string> teamsDict = new Dictionary<int, string>(); // Словарь, ключи в котором - это номера команд, а значения - их названия

    [SerializeField] private TextMeshProUGUI _turnText;
    [SerializeField] private TextMeshProUGUI _currentTeamText;
    [SerializeField] private TextMeshProUGUI _remainingTimeText;
    [SerializeField] private TextMeshProUGUI _timeOfDayText;

    private int _turnToChangeDayAndNight;

    private MouseSelection _mouseSelection;
    private SelectedObjectInformationEnableController _selectedObjectInformationEnableController;

    private void Awake()
    {
        InitComponentLinks();
    }
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

    public void NextTeam()
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
        currentTeam = 1;
        turn += 1;
        _currentTeamText.SetText("Current Team: " + teamsDict[currentTeam]);
        _turnText.SetText("Turn: " + (turn).ToString());
        if (turn == _turnToChangeDayAndNight)
        {
            if (isDay)
            {
                if (nightDuration + nightDurationChange > 0) StartNight();
                else nightDuration = 0;
            }
            else if (!isDay) 
            {
                if (dayDuration + dayDurationChange > 0) StartDay();
                else dayDuration = 0;
            }
        }
        onTurnChanged?.Invoke();
        SetChangesInOtherClassesOnTurnChanged();
    }
    private void StartNight()
    {
        isDay = false;
        nightDuration += nightDurationChange;
        _timeOfDayText.SetText("Night");
        _turnToChangeDayAndNight = turn + nightDuration;

        _turnText.color = Color.white;
        _currentTeamText.color = Color.white;
        _remainingTimeText.color = Color.white;
        _timeOfDayText.color = Color.white;

        NightStarts?.Invoke();
    }
    private void StartDay()
    {
        isDay = true;
        dayDuration += dayDurationChange;
        _timeOfDayText.SetText("Day");
        _turnToChangeDayAndNight = turn + dayDuration;

        _turnText.color = Color.black;
        _currentTeamText.color = Color.black;
        _remainingTimeText.color = Color.black;
        _timeOfDayText.color = Color.black;

        DayStarts?.Invoke();
    }
    private void FirstTurnStart()
    {
        turn = 1;
        currentTeam = 1;
        TeamsNameInitialization();
        _currentTeamText.SetText("Current Team: " + teamsDict[currentTeam]);
        remainingTime = timeToTurn;
        StartDay();
        dayDuration -= dayDurationChange;
        _turnToChangeDayAndNight = turn + dayDuration;
        _turnText.SetText("Turn: " + (turn).ToString());
    }
    private void TeamsNameInitialization() // В будущем будет какая-нибудь менюшка, в которую игрок будет вводить название своей команды, но пока её нет, будет эта затычка
    {
        teamsDict[1] = "First Team";
        teamsDict[2] = "Second Team";
        teamsDict[3] = "Third Team";
        teamsDict[4] = "Fourth Team";
        teamsDict[5] = "Fifth Team";
    }

    private void SetChangesInOtherClassesOnTurnChanged()
    {
        foreach (EffectsDescription effect in FindObjectsOfType<EffectsDescription>()) // Можно потимизировать, создав тут список, в который эффекты будут записывать себя при появлении
        {
            effect.ReduceRemainingLifeTimeOnTurnChanged();
        }
        _selectedObjectInformationEnableController.SelectedObjectInformationSetActive(_mouseSelection.selected);
    }

    private void InitComponentLinks()
    {
        _mouseSelection = FindObjectOfType<MouseSelection>();
        _selectedObjectInformationEnableController = FindObjectOfType<SelectedObjectInformationEnableController>();
    }
}
