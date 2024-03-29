﻿#region

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JSON;
using UnityEngine;

#endregion

public class Fort : Building
{
	private static readonly Material[][] materials = new Material[3][];
	public readonly List<int> targetTeams = new List<int>();
	private Transform bomb;
	private Transform cannon;
	private Component[] idleFXs;
	public int life;
	public int rebornsLeft;
	protected override Transform Beamer { get { return cannon; } }

	protected override IEnumerator AimAtPosition(Vector3 targetPosition)
	{
		foreach (var idleFX in idleFXs.Cast<IIdleFX>())
			idleFX.Disable();
		var targetRotation = Quaternion.LookRotation(targetPosition - cannon.position);
		while (Data.GamePaused || Quaternion.Angle(cannon.rotation = Quaternion.RotateTowards(cannon.rotation, targetRotation, Settings.SteeringRate.FortCannon * Time.deltaTime), targetRotation) > Settings.AngularTolerance)
			yield return null;
	}

	protected override void Awake()
	{
		base.Awake();
		cannon = transform.Find("Cannon");
		bomb = cannon.Find("SP");
		idleFXs = cannon.GetComponents(typeof(IIdleFX));
		audio.volume = Settings.Audio.Volume.FortScore;
		audio.clip = Resources.Load<AudioClip>("Sounds/Fort_Score");
	}

	public override Vector3 Center() { return new Vector3(0.05f, 0.60f, 0.05f); }

	protected override Vector3 Dimensions() { return new Vector3(2.47f, 1.87f, 2.47f); }

	protected override IEnumerator FireAtPosition(Vector3 targetPosition)
	{
		++explosionsLeft;
		(Instantiate(Resources.Load("Bomb"), bomb.position, bomb.rotation) as GameObject).GetComponent<BombManager>().Initialize(this, targetPosition, BombManager.Level.Large);
		isAttacking = false;
		while (explosionsLeft > 0)
			yield return null;
		foreach (var idleFX in idleFXs.Cast<IIdleFX>())
			idleFX.Enable();
		--Data.Replay.AttacksLeft;
	}

	protected override IEnumerator FireAtUnitBase(UnitBase targetUnitBase)
	{
		++explosionsLeft;
		(Instantiate(Resources.Load("Bomb"), bomb.position, bomb.rotation) as GameObject).GetComponent<BombManager>().Initialize(this, targetUnitBase, BombManager.Level.Large);
		isAttacking = false;
		while (explosionsLeft > 0)
			yield return null;
		foreach (var idleFX in idleFXs.Cast<IIdleFX>())
			idleFX.Enable();
		--Data.Replay.AttacksLeft;
	}

	public override void Initialize(JSONObject info)
	{
		base.Initialize(info);
		if (team < 2)
			Data.Replay.Forts[team].Add(this);
	}

	protected override int Kind() { return 1; }

	protected override void LoadMark() { markRect = (Instantiate(Resources.Load("Marks/Fort")) as GameObject).GetComponent<RectTransform>(); }

	public static void LoadMaterial()
	{
		string[] name = { "B", "C", "R" };
		for (var id = 0; id < 3; id++)
		{
			materials[id] = new Material[3];
			for (var team = 0; team < 3; team++)
				materials[id][team] = Resources.Load<Material>("Fort/Materials/" + name[id] + "_" + team);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (team < 2)
			Data.Replay.Forts[team].Remove(this);
	}

	public IEnumerator Reborn(Vector3 internalPosition, int index, List<int> targetTeams, int fuel, int ammo, int metal, int life)
	{
		team = targetTeams[0];
		targetTeams.RemoveAt(0);
		targetTeams.ForEach(targetTeam => this.targetTeams.Add(targetTeam));
		Data.Replay.Elements.Add(this.index = index, this);
		Data.Replay.Forts[team].Add(this);
		++Data.Replay.UnitNums[team];
		Data.Replay.TargetScores[team] += Constants.Score.PerFortCapture;
		targetHP = MaxHP();
		targetFuel = fuel;
		targetAmmo = ammo;
		targetMetal = metal;
		transform.position = internalPosition - Vector3.up * RelativeSize * Settings.DimensionScaleFactor;
		while (Data.GamePaused || (internalPosition - transform.position).y > Settings.DimensionalTolerance)
		{
			if (!Data.GamePaused)
				transform.position = Vector3.Lerp(transform.position, internalPosition, Settings.TransitionRate * Time.deltaTime);
			yield return null;
		}
		this.life = life;
		--Data.Replay.AttacksLeft;
	}

	public static void RefreshMaterialColor()
	{
		for (var id = 0; id < 3; id++)
			for (var team = 0; team < 3; team++)
				materials[id][team].SetColor("_Color", Data.TeamColor.Current[team]);
	}

	public static void RefreshTextureOffset()
	{
		for (var team = 0; team < 3; team++)
		{
			var offset = materials[2][team].mainTextureOffset;
			offset.y = (offset.y + Time.deltaTime) % 1;
			materials[2][team].mainTextureOffset = offset;
		}
	}

	protected override void Start()
	{
		base.Start();
		transform.Find("Accessory").GetComponent<MeshRenderer>().material = materials[1][team];
		transform.Find("Base").GetComponent<MeshRenderer>().material = materials[0][team];
		cannon.GetComponent<MeshRenderer>().materials = new[] { materials[1][team], materials[2][team] };
	}
}