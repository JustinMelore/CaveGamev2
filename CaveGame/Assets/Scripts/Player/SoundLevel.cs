using UnityEngine;

/// <summary>
/// Enum to represent sound levels
/// </summary>
public enum SoundLevel
{
    NONE = 0,
    QUIET = 1,
    MODERATE = 2,
    LOUD = 3
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
