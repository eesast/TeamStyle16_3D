﻿#region

using System.Collections;
using System.Linq;
using UnityEngine;

#endregion

public class Destroyer : Ship
{
	private static readonly Material[][] materials = new Material[1][];
	private Transform barrel;
	private Transform bomb;
	private Component[] idleFXs;
	private Transform swivel;

	protected override IEnumerator AimAtPosition(Vector3 targetPosition)
	{
		foreach (var idleFX in idleFXs.Cast<IIdleFX>())
			idleFX.Disable();
		var targetRotation = Quaternion.identity;
		do
		{
			yield return null;
			if (Data.GamePaused)
				continue;
			targetRotation = Quaternion.LookRotation(swivel.TransformDirection(Vector3.Scale(swivel.InverseTransformPoint(targetPosition), new Vector3(1, 0, 1))), swivel.up);
			swivel.rotation = Quaternion.RotateTowards(swivel.rotation, targetRotation, Settings.SteeringRate.DestroyerSwivel * Time.deltaTime);
		}
		while (Data.GamePaused || Quaternion.Angle(swivel.rotation, targetRotation) > Settings.AngularTolerance);
		targetRotation = Quaternion.LookRotation(targetPosition - barrel.position, barrel.up);
		while (Data.GamePaused || Quaternion.Angle(barrel.rotation = Quaternion.RotateTowards(barrel.rotation, targetRotation, Settings.SteeringRate.DestroyerBarrel * Time.deltaTime), targetRotation) > Settings.AngularTolerance)
			yield return null;
	}

	protected override void Awake()
	{
		base.Awake();
		swivel = transform.Find("Hull/Swivel");
		barrel = swivel.Find("Barrel");
		bomb = barrel.Find("SP");
		idleFXs = swivel.GetComponentsInChildren(typeof(IIdleFX));
	}

	public override Vector3 Center() { return new Vector3(0.00f, 0.43f, 0.00f); }

	protected override Vector3 Dimensions() { return new Vector3(0.92f, 1.39f, 2.17f); }

	protected override IEnumerator FireAtPosition(Vector3 targetPosition)
	{
		++explosionsLeft;
		(Instantiate(Resources.Load("Bomb"), bomb.position, bomb.rotation) as GameObject).GetComponent<BombManager>().Initialize(this, targetPosition, BombManager.Level.Medium);
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
		(Instantiate(Resources.Load("Bomb"), bomb.position, bomb.rotation) as GameObject).GetComponent<BombManager>().Initialize(this, targetUnitBase, BombManager.Level.Medium);
		isAttacking = false;
		while (explosionsLeft > 0)
			yield return null;
		foreach (var idleFX in idleFXs.Cast<IIdleFX>())
			idleFX.Enable();
		--Data.Replay.AttacksLeft;
	}

	protected override int Kind() { return 5; }

	public static void LoadMaterial()
	{
		string[] name = { "D" };
		for (var id = 0; id < 1; id++)
		{
			materials[id] = new Material[3];
			for (var team = 0; team < 3; team++)
				materials[id][team] = Resources.Load<Material>("Destroyer/Materials/" + name[id] + "_" + team);
		}
	}

	public static void RefreshMaterialColor()
	{
		for (var id = 0; id < 1; id++)
			for (var team = 0; team < 3; team++)
				materials[id][team].SetColor("_Color", Data.TeamColor.Current[team]);
	}

	protected override void Start()
	{
		base.Start();
		foreach (var meshRenderer in GetComponentsInChildren<MeshRenderer>())
			meshRenderer.material = materials[0][team];
	}
}