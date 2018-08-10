using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RedRunner.Characters;

namespace RedRunner.Enemies
{

	public class Flower : Enemy
	{

		[SerializeField]
		private Collider2D m_Collider2D;
        [SerializeField]
        private ParticleSystem m_explosionParticlePrefab;

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
			target.Die (true);

            ParticleSystem particle = Instantiate<ParticleSystem> (m_explosionParticlePrefab, this.transform.position, Quaternion.identity);
            particle.Play();
			Destroy (particle.gameObject, particle.main.duration);
            Destroy(this.gameObject);
			AudioManager.Singleton.PlayExplosionSound (transform.position);
		}

	}

}