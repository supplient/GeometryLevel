using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantSpeed : MonoBehaviour
{
    public Vector2 m_initVelocity = new Vector2(1.0f, 0);
    public float m_constantSpeed = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        var rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.velocity = m_initVelocity;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.velocity = m_constantSpeed * rigidbody.velocity.normalized;
    }
}
