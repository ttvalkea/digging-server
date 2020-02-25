
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

public static class PersistingValues
{
    static PersistingValues() {
        IdsOfConnectedClients = new List<string>();
        Obstacles = new List<Obstacle>();
        TagPlayerId = "";
        StartNewTagInfoSentThisCycleResetTimer();
        NewTagInfoSentThisCycle = false;
        StartNewTagItemTimer();

        //Start this timer half a second later than the resetting timer. This way, NewTagInfoSentThisCycle should always be false for the first attempt of NewTagItemTimer.Elapsed event.
        Task.Delay(500).ContinueWith(t => EnableNewTagItemTimer());
    }

    public static Timer NewTagItemTimer { get; set; }
    public static Timer NewTagInfoSentThisCycleResetTimer { get; set; }
    public static bool NewTagInfoSentThisCycle { get; set; }
    public static NewTagItem TagItem { get; set; }
    public static string TagPlayerId { get; set; }
    public static List<Obstacle> Obstacles { get; set; }
    public static List<string> IdsOfConnectedClients { get; set; }


    //Set up loops
    private static void StartNewTagItemTimer()
    {
        NewTagItemTimer = new Timer(Constants.NEW_TAG_ITEM_SPAWN_INTERVAL_MS);
        NewTagItemTimer.AutoReset = true;
        NewTagItemTimer.Enabled = false;
    }
    private static void EnableNewTagItemTimer()
    {
        NewTagItemTimer.Enabled = true;
    }
    private static void StartNewTagInfoSentThisCycleResetTimer()
    {
        NewTagInfoSentThisCycleResetTimer = new Timer(Constants.NEW_TAG_ITEM_SPAWN_INTERVAL_MS);
        NewTagInfoSentThisCycleResetTimer.AutoReset = true;
        NewTagInfoSentThisCycleResetTimer.Elapsed += OnNewTagInfoSentThisCycleResetTimerEvent;
        NewTagInfoSentThisCycleResetTimer.Enabled = true;
    }
    private static void OnNewTagInfoSentThisCycleResetTimerEvent(Object source, ElapsedEventArgs e)
    {
        NewTagInfoSentThisCycle = false;
    }
}


