using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace geo_level
{
	public class PhysicsController : MonoBehaviour
	{
		public static PhysicsController instance {
			get
			{
				var gameObject = GameObject.FindGameObjectWithTag("Player");
				return gameObject.GetComponent<PhysicsController>();
			}
		}

		[Tooltip("How much angular velocity will change if you push the button for one second (in radian).")]
		public float m_torquePerSecond = 1.0f;
		[Tooltip("If angular velocity(radian/s) is larger than maxAngularVelocity, torque will not be added.")]
		public float m_maxAngularVelocity = 360.0f;

		public float input
		{
			set
			{
				// Check if Input exists
				if(value == 0.0f)
					return;

				// TODO: seems need to be negative
				value = -value;

				// Cal Torque
				float torque = Time.fixedDeltaTime * value * m_torquePerSecond;

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
	}
}
