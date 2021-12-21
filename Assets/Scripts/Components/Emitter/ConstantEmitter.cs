using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace geo_level
{
	public class ConstantEmitter : MonoBehaviour
	{
		public GameObject m_projectilePrefab;

		[Tooltip("Emit interval in seconds.")]
		public float m_interval;

		[Tooltip("Offset to the center of the Emitter, when generating the projectile.")]
		public Vector3 m_offset = new Vector3(0.0f, 0.0f, 0.0f);

		/// <summary>
		/// How much time has passed since the last emitting.
		/// </summary>
		private float m_timeDelta = 0.0f;

		private void FixedUpdate()
		{
			m_timeDelta += Time.fixedDeltaTime;
			while(m_timeDelta >= m_interval)
			{
				m_timeDelta -= m_interval;
				Emit();
			}
		}

		private void Emit()
		{
			var projectile = Instantiate(m_projectilePrefab);
			var trans = projectile.transform;
			trans.position = transform.position + m_offset;
		}
	}
}
