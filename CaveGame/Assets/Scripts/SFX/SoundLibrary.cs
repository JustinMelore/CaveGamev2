using UnityEngine;

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Scriptable Objects/SoundLibrary")]
public class SoundLibrary : ScriptableObject
{
    public SoundEffect[] soundList;
}
