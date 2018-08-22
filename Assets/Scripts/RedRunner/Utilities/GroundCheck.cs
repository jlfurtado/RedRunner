using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedRunner.Utilities
{

	public class GroundCheck : MonoBehaviour
	{

		public delegate void GroundedHandler ();

		public event GroundedHandler OnGrounded;

		public const string GROUND_TAG = "Ground";
        public const string SNOW_TAG = "Snow";
		public const string GROUND_LAYER_NAME = "Ground";

		[SerializeField]
		private Collider2D m_Collider2D;

		[SerializeField]
		private float m_RayDistance = 0.5f;

        public bool IsGrounded { get { return m_IsGrounded; } }
        public bool IsSnow { get { return m_IsSnow; } }

        private bool m_IsGrounded = false;
        private bool m_IsSnow = false;

		void Awake ()
		{
			m_IsGrounded = false;
		}

		void Update ()
		{
			Vector2 left = new Vector2 (m_Collider2D.bounds.max.x, m_Collider2D.bounds.center.y);
			Vector2 center = new Vector2 (m_Collider2D.bounds.center.x, m_Collider2D.bounds.center.y);
			Vector2 right = new Vector2 (m_Collider2D.bounds.min.x, m_Collider2D.bounds.center.y);
		
			RaycastHit2D hit1 = Physics2D.Raycast (left, new Vector2 (0f, -1f), m_RayDistance, LayerMask.GetMask (GROUND_LAYER_NAME));
			Debug.DrawRay (left, new Vector2 (0f, -m_RayDistance));

			RaycastHit2D hit2 = Physics2D.Raycast (center, new Vector2 (0f, -1f), m_RayDistance, LayerMask.GetMask (GROUND_LAYER_NAME));
			Debug.DrawRay (center, new Vector2 (0f, -m_RayDistance));
		
			RaycastHit2D hit3 = Physics2D.Raycast (right, new Vector2 (0f, -1f), m_RayDistance, LayerMask.GetMask (GROUND_LAYER_NAME));
			Debug.DrawRay (right, new Vector2 (0f, -m_RayDistance));

			bool grounded = IsGround(hit1.collider) || IsGround(hit2.collider) || IsGround(hit3.collider);

			if (grounded && !m_IsGrounded) {
				if (OnGrounded != null) {
					OnGrounded ();
				}
			}

			m_IsGrounded = grounded;
            m_IsSnow = IsSnowGround(hit1.collider) || IsSnowGround(hit2.collider) || IsSnowGround(hit3.collider);
        }

        private bool IsGround(Collider2D collider)
        {
            return (collider != null) && (collider.CompareTag(GROUND_TAG) || collider.CompareTag(SNOW_TAG));
        }

        private bool IsSnowGround(Collider2D collider)
        {
            return (collider != null) && collider.CompareTag(SNOW_TAG);
        }

	}

}