#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using UnityEngine.Audio;

/// <summary>
/// Custom editor for the SoundLibrary scriptable object
/// </summary>
[CustomEditor(typeof(SoundLibrary))]
public class SoundsSOEditor : Editor
{
    private void OnEnable()
    {
        ref SoundEffect[] soundList = ref ((SoundLibrary)target).soundList;

        if (soundList == null)
            return;

        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref soundList, names.Length);
        for (int i = 0; i < soundList.Length; i++)
        {
            string currentName = names[i];
            soundList[i].name = currentName;
        }
    }
}
#endif
