using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class ClearAfterCinematic : MonoBehaviour
{
    public PlayableDirector director;
    public List<GameObject> gameObjects;

    void Start()
    {
        director.stopped += OnCinematicEnd;
    }

    void OnCinematicEnd(PlayableDirector d)
    {
        for (int i = 0; i < gameObjects.Count; i++)
        {
            Destroy(gameObjects[i]);
        }
    }
}