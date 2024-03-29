﻿#region

using System.Collections;
using UnityEngine;

#endregion

public class BombManager : MonoBehaviour
{
	public enum Level
	{
		Small,
		Medium,
		Large
	}

	private UnitBase attacker;
	private bool exploded;
	private Level level;
	private bool shallResumeAudio;
	private Vector3 targetPosition;
	private UnitBase targetUnitBase;

	private void Awake()
	{
		Delegates.GameStateChanged += OnGameStateChanged;
		foreach (var childCollider in GetComponentsInChildren<Collider>())
			childCollider.gameObject.layer = LayerMask.NameToLayer("Bomb");
		audio.maxDistance = Settings.Audio.MaxAudioDistance;
		audio.volume = Settings.Audio.Volume.Bomb;
	}

	private static Vector3 Dimensions() { return new Vector3(0.12f, 0.12f, 0.74f); }

	private void Explode()
	{
		audio.clip = Resources.Load<AudioClip>("Sounds/Impact_" + level);
		if (Data.GamePaused)
			shallResumeAudio = true;
		else
			audio.Play();
		(Instantiate(Resources.Load("Detonator_" + level), transform.position, Quaternion.identity) as GameObject).GetComponent<Detonator>().size = ((float)level + 1) / 2 * Settings.DimensionScaleFactor;
		--attacker.explosionsLeft;
		StartCoroutine(FadeOut());
		exploded = true;
	}

	private IEnumerator FadeOut()
	{
		GetComponent<MeshRenderer>().enabled = false;
		var trail = transform.Find("Trail").particleSystem;
		while ((trail.emissionRate *= Settings.FastAttenuation) > 3 || audio.isPlaying)
			yield return new WaitForSeconds(Settings.DeltaTime);
		Destroy(gameObject);
	}

	public void Initialize(UnitBase attacker, Vector3 targetPosition, Level bombLevel)
	{
		this.attacker = attacker;
		this.targetPosition = targetPosition;
		level = bombLevel;
	}

	public void Initialize(UnitBase attacker, UnitBase targetUnitBase, Level bombLevel)
	{
		this.attacker = attacker;
		this.targetUnitBase = targetUnitBase;
		targetPosition = targetUnitBase.transform.WorldCenterOfElement();
		level = bombLevel;
	}

	private void OnDestroy() { Delegates.GameStateChanged -= OnGameStateChanged; }

	private void OnGameStateChanged()
	{
		if (Data.GamePaused)
		{
			if (!audio.isPlaying)
				return;
			audio.Pause();
			shallResumeAudio = true;
		}
		else if (shallResumeAudio)
		{
			audio.Play();
			shallResumeAudio = false;
		}
	}

	private void OnTriggerEnter(Component other)
	{
		if (exploded)
			return;
		var unitBase = other.GetComponentInParent(typeof(UnitBase));
		if (!unitBase || unitBase != targetUnitBase)
			return;
		Explode();
	}

	private IEnumerator Start()
	{
		audio.clip = Resources.Load<AudioClip>("Sounds/Launcher_" + level);
		if (Data.GamePaused)
			shallResumeAudio = true;
		else
			audio.Play();
		transform.localScale = Vector3.one * ((int)level + 1) * 0.1f * Settings.DimensionScaleFactor / ((Dimensions().x + Dimensions().z));
		while (Data.GamePaused || !exploded && (targetPosition - transform.position).magnitude > Settings.DimensionalTolerancePerUnitSpeed * Settings.Bomb.Speed)
		{
			if (!Data.GamePaused)
			{
				transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position), Time.deltaTime * Settings.Bomb.AngularCorrectionRate);
				transform.Translate((Vector3.forward + Random.insideUnitSphere * Settings.Bomb.Noise / ((float)level + 2)) * Settings.Bomb.Speed * Time.deltaTime);
			}
			yield return null;
		}
		if (!exploded)
			Explode();
	}
}