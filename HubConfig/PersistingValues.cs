
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

public static class PersistingValues
{
    static PersistingValues() {
        IdsOfConnectedClients = new List<string>();
        Obstacles = new List<Obstacle>();
        EmptySpaces = new List<Coordinate>();
        SoilTiles = new List<SoilInfo>();
        StartFruitInfoSentThisCycleResetTimer();
        FruitInfoSentThisCycle = false;
        StartFruitInfoTimer();

        //Start this timer half a second later than the resetting timer. This way, FruitInfoSentThisCycle should always be false for the first attempt of FruitInfoTimer.Elapsed event.
        Task.Delay(500).ContinueWith(t => EnableFruitInfoTimer());
    }

    public static Timer FruitInfoTimer { get; set; }
    public static Timer FruitInfoSentThisCycleResetTimer { get; set; }
    public static bool FruitInfoSentThisCycle { get; set; }
    public static List<Obstacle> Obstacles { get; set; }
    public static List<Coordinate> EmptySpaces { get; set; }
    public static List<SoilInfo> SoilTiles { get; set; }
    public static List<string> IdsOfConnectedClients { get; set; }


    //Set up loops
    private static void StartFruitInfoTimer()
    {
        FruitInfoTimer = new Timer(Constants.FRUIT_GROWTH_INTERVAL_MS);
        FruitInfoTimer.AutoReset = true;
        FruitInfoTimer.Enabled = false;
    }
    private static void EnableFruitInfoTimer()
    {
        FruitInfoTimer.Enabled = true;
    }
    private static void StartFruitInfoSentThisCycleResetTimer()
    {
        FruitInfoSentThisCycleResetTimer = new Timer(Constants.FRUIT_GROWTH_INTERVAL_MS);
        FruitInfoSentThisCycleResetTimer.AutoReset = true;
        FruitInfoSentThisCycleResetTimer.Elapsed += OnFruitInfoSentThisCycleResetTimerEvent;
        FruitInfoSentThisCycleResetTimer.Enabled = true;
    }
    private static void OnFruitInfoSentThisCycleResetTimerEvent(Object source, ElapsedEventArgs e)
    {
        FruitInfoSentThisCycle = false;
    }
}


