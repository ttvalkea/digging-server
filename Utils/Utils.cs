
using System;

public static class Utils
{
    public static string GetId(string prefix = "")
    {
        var rng = new Random();
        return (prefix + DateTime.Now.Ticks + "" + rng.Next(0, 1000));
    }
}


