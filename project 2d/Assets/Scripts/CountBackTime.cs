﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CountBackTime : MonoBehaviour
{
    int minutes = Web.timeInSec / 60;
    int seconds = Web.timeInSec % 60;
    [SerializeField]
    TextMeshProUGUI text;
    
    int total_sec;
    private float startTime;
    private bool running = true;
    private bool increase_font_size = false;
    private bool play_end = false;
    private bool clockTicking = false;

    // Start is called before the first frame update
    void Start()
    {
        if (RouteLevel.isCustom)
        {
            minutes = Areas.timeInSec / 60;
            seconds = Areas.timeInSec % 60;
        }
        else if (RouteLevel.isCustomChallenge)
        {
            minutes = Web.CustomtimeInSec / 60;
            seconds = Web.CustomtimeInSec % 60;
        }
        else if (calculateScore.isDemo)
        {
            minutes = 2;
            seconds = 0;
        }
        

        //initializing the class fields
        running = true;
        startTime = Time.time;
        total_sec = minutes * 60 + seconds;// casting the time to seconds
    }

    /*
     * Function that caculate The seconds that have passed since the start of the game and show the clock text 
     */
    private void Update()
    {
        float t = Time.time - startTime;
        int time_left = total_sec - (int)t;
        if (time_left >= 0 && running)
        {
            if (time_left <= 30 && time_left > 10)
            {
                text.faceColor = new Color32(255, 165, 0, 255);
                text.color = new Color32(255, 165 , 0, 255);
            }
            if (time_left <= 10 && !increase_font_size)
            {
                if (!clockTicking)
                {
                    SoundManagerScript.PlaySound("clockTicking");
                    clockTicking = true;
                }
                text.color = Color.red;
                text.fontSize += text.fontSize * 0.1f;
                increase_font_size = true;
            }
            string seconds = ((time_left) % 60).ToString("f0");
            string minutes = (time_left / 60).ToString();

            if (int.Parse(minutes) < 10) minutes = "0" + minutes;
            if (int.Parse(seconds) < 10) seconds = "0" + seconds;

            text.text = minutes + ":" + seconds;
        }
        // if the time is over - show the score to the player
        else
        {
            if (!play_end)
            {
                running = false;
                play_end = true;

            }
        }
    }

    /*
     * Function that returns if the game is running
     */
    public bool isRunning()
    {
        return running;
    }

    /*
     * Function that assign false to running field
     */
    public void setNoRunning()
    {
        running = false;
    }
}
