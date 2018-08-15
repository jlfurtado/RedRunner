using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour
{
	public float MoveSpeed = 10f;
	// How quickly the camera should move from point A to B.
	public float SnapDistance = 0.25f;

	public bool IsShaking { get; private set; }

    public Vector3 ShakeOffset { get; private set; }

	private Vector3 _newPosition;
	private float _currentMoveSpeed;

	// For shaking camera
	private int _shakeCount;
	private float _shakeIntensity, _shakeSpeed;
	private Vector3 _nextShakePosition;

	
	void Update ()
	{
		if (IsShaking) {
            // Move toward the previously determined next shake position
            ShakeOffset = Vector3.MoveTowards (ShakeOffset, _nextShakePosition, Time.deltaTime * _shakeSpeed);

			// Determine if we are there or not
			if (Vector2.Distance (ShakeOffset, _nextShakePosition) < _shakeIntensity / 5f) {
				//Decrement shake counter
				_shakeCount--;

				// If we are done shaking, turn this off if we're not longer moving
				if (_shakeCount <= 0) {
					IsShaking = false;
					ShakeOffset = Vector3.zero;
				}
                // If there is only 1 shake left, return back to base
                else if (_shakeCount <= 1) {
                    _nextShakePosition = Vector3.zero;
				}
                // If we are not done or nearing done, determine the next position to travel to
                else {
					DetermineNextShakePosition ();
				}
			}
		}
	}

	/// <summary>
	/// Shakes the camera. Essentially places some random points around the camera and lerps it to them.
	/// </summary>
	/// <param name="intensity">Max distance from the center point the camera will travel.</param>
	/// <param name="shakes">Total number of random points the camera will travel to.</param>
	/// <param name="speed">How quickly the camera moves from point to point.</param>
	public void Shake (float intensity, int shakes, float speed)
	{
		enabled = true;

        IsShaking = true;
		_shakeCount = shakes;
		_shakeIntensity = intensity;
		_shakeSpeed = speed;

		DetermineNextShakePosition ();
	}


	private void DetermineNextShakePosition ()
	{
		_nextShakePosition = new Vector3 (Random.Range (-_shakeIntensity, _shakeIntensity), Random.Range (0, _shakeIntensity), 0);
	}
}