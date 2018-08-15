using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RedRunner.Characters;

namespace RedRunner.Enemies
{

	public class Flower : Enemy
	{
        private const float FLOWER_HEIGHT = 1.28f;
		[SerializeField]
		private Collider2D m_Collider2D;
        [SerializeField]
        private ParticleSystem m_explosionParticlePrefab;
        [SerializeField]
        private float m_explosionMagnitude;

		public override Collider2D Collider2D {
			get {
				return m_Collider2D;
			}
		}

		void OnTriggerEnter2D (Collider2D other)
		{
			Character character = other.GetComponent<Character> ();
			if (character != null) {
				Kill (character);
			}
		}

		public override void Kill (Character target)
		{
            Vector2 direction = (target.transform.position - (transform.position - Vector3.up * FLOWER_HEIGHT)).normalized;
            target.GetComponent<Rigidbody2D>().AddForce(direction * m_explosionMagnitude, ForceMode2D.Impulse);
            target.Die (true);
            Camera.main.GetComponent<CameraControl>().Shake(1.5f, 5, 50);

            ParticleSystem particle = Instantiate<ParticleSystem> (m_explosionParticlePrefab, this.transform.position, Quaternion.identity);
            particle.Play();
			Destroy (particle.gameObject, particle.main.duration);
            Destroy(this.gameObject);
			AudioManager.Singleton.PlayExplosionSound (transform.position);
		}

	}

}