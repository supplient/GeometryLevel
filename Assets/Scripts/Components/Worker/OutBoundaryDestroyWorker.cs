using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace geo_level
{
	public class OutBoundaryDestroyWorker : MonoBehaviour
	{
		[Tooltip(@"The interval between each checking whether should be destroyed, in seconds.
		Note, this is a minimal value, which means the actual interval will always >= this value."
		)]
		public float m_checkInterval = 0.1f;

		[Tooltip(@"Margin for the boundary, in units.
		e.g. If the object's x is in [leftBottom.x - margin, rightTop.x + margin], it will not be destroyed."
		)]
		public float m_margin = 2.0f;
		[Tooltip("The left bottom of the boundary. This will automatically be filled by the camera's viewport.")]
		public Vector2 m_leftBottom;
		[Tooltip("The right top of the boundary. This will automatically be filled by the camera's viewport.")]
		public Vector2 m_rightTop;

		private void Start()
		{
			var camera = Camera.main;
			var cx = camera.transform.position.x;
			var cy = camera.transform.position.y;
			var cheight = camera.orthographicSize;
			var cwidth = cheight * camera.aspect;

			var left	= cx - cwidth	;
			var right	= cx + cwidth	;
			var bottom	= cy - cheight	;
			var top		= cy + cheight	;

			m_leftBottom.x	= left		;
			m_leftBottom.y	= bottom	;
			m_rightTop.x	= right		;
			m_rightTop.y	= top		;

			StartCoroutine(Check());
		}

		private IEnumerator Check()
		{
			while(true)
			{
				float x = transform.position.x;
				float y = transform.position.y;
				if(
					x < m_leftBottom.x - m_margin || x > m_rightTop.x + m_margin ||
					y < m_leftBottom.y - m_margin || y > m_rightTop.y + m_margin
					)
				{
					DoDestroy();
					break;
				}

				yield return new WaitForSeconds(m_checkInterval);
			}
		}

		private void DoDestroy()
		{
			Debug.Log(string.Format("{0} is destroyed for out of boundary.", gameObject.name), gameObject);
			Destroy(gameObject, 1.0f);
		}
	}
}
