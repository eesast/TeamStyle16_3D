﻿#region

using System.Collections;
using System.Linq;
using UnityEngine;

#endregion

public class Carrier : Ship
{
	private static readonly Material[][] materials = new Material[2][];
	private Interceptor[] interceptors;
	private bool interruptInterceptorReturns;
	public int movingInterceptorsLeft;

	protected override IEnumerator AimAtPosition(Vector3 targetPosition)
	{
		foreach (var interceptor in interceptors)
			interceptor.StartCoroutine(interceptor.AimAtPosition(targetPosition));
		while (movingInterceptorsLeft > 0)
			yield return null;
	}

	protected override void Awake()
	{
		base.Awake();
		interceptors = GetComponentsInChildren<Interceptor>();
	}

	public override Vector3 Center() { return new Vector3(-0.02f, 0.26f, 0.15f); }

	public void DestroyInterceptors()
	{
		foreach (var interceptor in interceptors.Where(interceptor => interceptor))
			Destroy(interceptor.gameObject);
	}

	protected override Vector3 Dimensions() { return new Vector3(1.14f, 1.82f, 3.01f); }

	protected override IEnumerator FireAtPosition(Vector3 targetPosition)
	{
		explosionsLeft += interceptors.Length * 2;
		foreach (var interceptor in interceptors)
		{
			interceptor.FireAtPosition(targetPosition);
			interceptor.StartCoroutine(interceptor.returnTrip = interceptor.Return());
		}
		isAttacking = false;
		while (explosionsLeft > 0)
			yield return null;
		StartCoroutine(MonitorInterceptorReturns());
	}

	protected override IEnumerator FireAtUnitBase(UnitBase targetUnitBase)
	{
		explosionsLeft += interceptors.Length * 2;
		foreach (var interceptor in interceptors)
		{
			interceptor.FireAtUnitBase(targetUnitBase);
			interceptor.StartCoroutine(interceptor.returnTrip = interceptor.Return());
		}
		isAttacking = false;
		while (explosionsLeft > 0)
			yield return null;
		StartCoroutine(MonitorInterceptorReturns());
	}

	public void ForceDestructReturningInterceptors()
	{
		interruptInterceptorReturns = true;
		foreach (var interceptor in interceptors.Where(interceptor => !interceptor.transform.parent))
			interceptor.ForceDestruct();
		--Data.Replay.AttacksLeft;
	}

	protected override int Kind() { return 6; }

	public static void LoadMaterial()
	{
		string[] name = { "C", "I" };
		for (var id = 0; id < 2; id++)
		{
			materials[id] = new Material[3];
			for (var team = 0; team < 3; team++)
				materials[id][team] = Resources.Load<Material>("Carrier/Materials/" + name[id] + "_" + team);
		}
	}

	private IEnumerator MonitorInterceptorReturns()
	{
		while (movingInterceptorsLeft > 0)
		{
			if (interruptInterceptorReturns)
				yield break;
			yield return null;
		}
		--Data.Replay.AttacksLeft;
	}

	public static void RefreshMaterialColor()
	{
		for (var id = 0; id < 2; id++)
			for (var team = 0; team < 3; team++)
				materials[id][team].SetColor("_Color", Data.TeamColor.Current[team]);
	}

	protected override void Start()
	{
		base.Start();
		transform.Find("Hull").GetComponent<MeshRenderer>().material = materials[0][team];
		transform.Find("Hull/Radar").GetComponent<MeshRenderer>().material = materials[0][team];
		foreach (var interceptor in interceptors)
			interceptor.GetComponentInChildren<MeshRenderer>().material = materials[1][team];
	}
}