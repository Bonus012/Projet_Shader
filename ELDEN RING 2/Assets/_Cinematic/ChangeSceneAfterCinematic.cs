using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class ChangeSceneAfterCinematic : MonoBehaviour
{
    public PlayableDirector director;
    public string sceneName;

    void Start()
    {
        director.stopped += OnCinematicEnd;
    }

    void OnCinematicEnd(PlayableDirector d)
    {
        SceneManager.LoadScene(sceneName);
    }
}