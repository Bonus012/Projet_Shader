using UnityEngine;

public class AttackZone : MonoBehaviour
{
    [SerializeField] public bool IsInShieldMode;
    [SerializeField] public float pushForce = 10f;

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.GetComponent<Destroyable>())
        {
            if (!IsInShieldMode)
            {
                other.GetComponent<Destroyable>().BreakIt();
            }
            else
            {
                Rigidbody rb = other.attachedRigidbody;
                if (rb != null)
                {
                    Vector3 direction = (other.transform.position - transform.position).normalized;
                    rb.AddForce(direction * pushForce, ForceMode.Impulse);
                }
            }

        }
    }
    

}
