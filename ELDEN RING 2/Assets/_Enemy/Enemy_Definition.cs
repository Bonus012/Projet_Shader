using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType { Cac, Ranged, Magic }

[System.Serializable]
public struct Enemy
{
    public GameObject _Enemy;
    public Animator _EnemyAnimator;
    public EnemyType _EnemyType;
}

public class Enemy_Definition : MonoBehaviour
{
    [SerializeField] int Max_Life, Life;
    [SerializeField] float Speed;
    [SerializeField] float ViewRange;
    [SerializeField] List<Enemy> Enemy;
    [SerializeField] int Enemy_Id;
    [SerializeField] GameObject _Preview;

    private void Start()
    {
        _Preview.SetActive(false);
        Enemy[Enemy_Id]._Enemy.SetActive(true);
    }
}
