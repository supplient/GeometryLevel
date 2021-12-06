using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsController : MonoBehaviour
{
	[Tooltip("How much angular velocity will change if you push the button for one second (in radian).")]
	public float m_torquePerSecond = 1.0f;
	[Tooltip("If angular velocity(radian/s) is larger than maxAngularVelocity, torque will not be added.")]
	public float m_maxAngularVelocity = 360.0f;

	private void FixedUpdate()
	{
		// Get Input
		float input = 0.0f;
		if(Input.GetKey(KeyCode.A))
			input = -1.0f;
		else if(Input.GetKey(KeyCode.D))
			input = 1.0f;
		else
			return;

		// TODO: seems need to be negative
		input = -input;

		// Cal Torque
		float torque = Time.fixedDeltaTime * input * m_torquePerSecond;

		// Get Rigidbody
		Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
		if(!rigidbody)
		{
			Debug.LogWarning("No Rigidbody2D.");
			return;
		}

		// Truncate for Max Angular Velocity
		float allowedAngVelDelta = Mathf.Max(0.0f, m_maxAngularVelocity - Mathf.Abs(rigidbody.angularVelocity));
		float allowedTorque = allowedAngVelDelta * rigidbody.inertia;
		torque = Mathf.Sign(torque) * Mathf.Min(Mathf.Abs(torque), allowedTorque);

		// Apply Torque
		rigidbody.AddTorque(torque);
	}
}
