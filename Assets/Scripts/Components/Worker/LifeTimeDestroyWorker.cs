using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeTimeDestroyWorker : MonoBehaviour
{
    [Tooltip("Lifetime from the creation of the object to its destruction.")]
    public float m_lifeTime = 30.0f;

    private float m_startTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        m_startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time - m_startTime >= m_lifeTime)
		{
            Debug.Log(string.Format("{0} is destroyed for expired lifetime.", gameObject.name), gameObject);
            gameObject.SetActive(false);
            Destroy(gameObject, 0.1f);
		}
    }
}
