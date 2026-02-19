using UnityEngine;

public class Destroyable : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particleSystem;

    public void BreakIt()
    {
        if ( _particleSystem != null )
        {
        }

        Destroy(gameObject);
    }
}
