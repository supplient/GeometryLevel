using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : MonoBehaviour
{
    public float m_force;
    public float m_maxVel;

    // Start is called before the first frame update
    void Start()
    {
        var rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.velocity = new Vector2(1.0f, 0);
    }

    void FixedUpdate()
    {
        var rigidbody = GetComponent<Rigidbody2D>();
        if(rigidbody.velocity.sqrMagnitude >= m_maxVel)
            return;
        var addForce = m_force * rigidbody.velocity.normalized;
        rigidbody.AddForce(addForce);
    }
}
