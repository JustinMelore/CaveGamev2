using UnityEngine;

/// <summary>
/// Enum to represent sound levels
/// </summary>
public enum SoundLevel
{
    QUIET = 0,
    MODERATE = 1,
    LOUD = 2
}

/// <summary>
/// Simple class for coupling SoundLevel and position
/// </summary>
public class Sound
{
    public SoundLevel Volume { get; private set; }
    public Vector3 Position { get; private set; }

    public Sound(SoundLevel volume, Vector3 position)
    {
        Volume = volume;
        Position = position;
    }
}
