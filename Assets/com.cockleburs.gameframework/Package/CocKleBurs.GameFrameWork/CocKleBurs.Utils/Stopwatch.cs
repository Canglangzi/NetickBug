using UnityEngine;


namespace CockleBurs.GameFramework.Utility
{
public class Timer
{
    public string Handle { get; private set; }
    private float startTime;
    private float elapsedTime;
    private bool isRunning;
    private bool isPaused;
    private float pauseTime;

    public Timer(string handle)
    {
        Handle = handle;
        elapsedTime = 0f;
        isRunning = false;
        isPaused = false;
    }

    public void Start()
    {
        if (!isRunning)
        {
            startTime = Time.time;
            isRunning = true;
            isPaused = false;
        }
    }

    public void Stop()
    {
        if (isRunning)
        {
            elapsedTime += Time.time - startTime;
            isRunning = false;
        }
    }

    public void Reset()
    {
        elapsedTime = 0f;
        isRunning = false;
        isPaused = false;
    }

    public void Pause()
    {
        if (isRunning && !isPaused)
        {
            pauseTime = Time.time;
            isPaused = true;
        }
    }

    public void Resume()
    {
        if (isPaused)
        {
            elapsedTime += Time.time - pauseTime;
            isPaused = false;
            startTime = Time.time; // Reset start time for resumed calculation
        }
    }

    public float ElapsedTime()
    {
        if (isRunning)
        {
            return elapsedTime + (isPaused ? 0 : Time.time - startTime);
        }
        return elapsedTime;
    }

    public string ElapsedTimeFormatted()
    {
        float totalSeconds = ElapsedTime();
        int minutes = Mathf.FloorToInt(totalSeconds / 60);
        int seconds = Mathf.FloorToInt(totalSeconds % 60);
        int milliseconds = Mathf.FloorToInt((totalSeconds - Mathf.Floor(totalSeconds)) * 1000);
        return $"{minutes:D2}:{seconds:D2}:{milliseconds:D3}";
    }
}

}