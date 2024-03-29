﻿#region

using UnityEngine;

#endregion

[RequireComponent(typeof(Detonator)), AddComponentMenu("Detonator/Force")]
public class DetonatorForce : DetonatorComponent
{
	private readonly float _basePower = 4000.0f;
	private readonly float _baseRadius = 50.0f;
	private Collider[] _colliders;
	private bool _delayedExplosionStarted;
	private float _explodeDelay;
	private Vector3 _explosionPosition;
	private float _scaledIntensity;
	private float _scaledRange;
	private GameObject _tempFireObject;
	public GameObject fireObject;
	public float fireObjectLife;
	public float power;
	public float radius;
	public float upwardsBiasForce = 10;

	public override void Explode()
	{
		if (!on)
			return;
		if (detailThreshold > detail)
			return;

		if (!_delayedExplosionStarted)
			_explodeDelay = explodeDelayMin + (Random.value * (explodeDelayMax - explodeDelayMin));
		if (_explodeDelay <= 0) //if the delayTime is zero
		{
			//tweak the position such that the explosion center is related to the explosion's direction
			_explosionPosition = transform.position; //- Vector3.Normalize(MyDetonator().direction);
			_colliders = Physics.OverlapSphere(_explosionPosition, radius * size);

			foreach (var hit in _colliders)
			{
				if (!hit)
					continue;

				if (hit.rigidbody)
				{
					//align the force along the object's rotation
					//this is wrong - need to attenuate the velocity according to distance from the explosion center			
					//offsetting the explosion force position by the negative of the explosion's direction may help
					hit.rigidbody.AddExplosionForce((power * size), _explosionPosition, radius * size, upwardsBiasForce * size);

					//fixed 6/15/2013 - didn't work before, was sending message to this script instead :)
					hit.gameObject.SendMessage("OnDetonatorForceHit", null, SendMessageOptions.DontRequireReceiver);

					//and light them on fire for Rune
					if (fireObject)
					{
						//check to see if the object already is on fire. being on fire twice is silly
						if (hit.transform.Find(fireObject.name + "(Clone)"))
							return;

						_tempFireObject = (Instantiate(fireObject, transform.position, transform.rotation)) as GameObject;
						_tempFireObject.transform.parent = hit.transform;
						_tempFireObject.transform.localPosition = new Vector3(0f, 0f, 0f);
						if (_tempFireObject.particleEmitter)
						{
							_tempFireObject.particleEmitter.emit = true;
							Destroy(_tempFireObject, fireObjectLife);
						}
					}
				}
			}
			_delayedExplosionStarted = false;
			_explodeDelay = 0f;
		}
		else
		//tell update to start reducing the start delay and call explode again when it's zero
			_delayedExplosionStarted = true;
	}

	public override void Init()
	{
		//unused
	}

	public void Reset()
	{
		radius = _baseRadius;
		power = _basePower;
	}

	private void Update()
	{
		if (_delayedExplosionStarted)
		{
			_explodeDelay = (_explodeDelay - Time.deltaTime);
			if (_explodeDelay <= 0f)
				Explode();
		}
	}
}