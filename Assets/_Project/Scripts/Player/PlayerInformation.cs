using Alteruna;

public static class PlayerInformation
{
    public static string Name { get; set; }
    public static bool IsCreatingNewRoom { get; set; }
    public static int SkinIndex { get; set; }
    public static int MicrophoneVolume { get; set; } = 100;
    public static float AudioVolume { get; set; } = 0.5f;
    public static bool IsMusicMute { get; set; }
    public static float Sensitivity { get; set; } = 3f;
    public static float Contrast { get; set; } = 0;
    public static Room Room { get; set; }
}
