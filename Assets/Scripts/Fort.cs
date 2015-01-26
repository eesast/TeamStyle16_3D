﻿#region

using GameStatics;
using UnityEngine;

#endregion

public class Fort : EntityBehaviour
{
	private static readonly Vector3 dimensions = new Vector3(2.47f, 1.53f, 2.47f);
	private static readonly Material[][] materials = new Material[3][];
	private static readonly int maxHP = 100;

	protected override Vector3 Dimensions() { return dimensions; }

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

	protected override int MaxHP() { return maxHP; }

	public static void RefreshMaterialColor()
	{
		for (var id = 0; id < 3; id++)
			for (var team = 0; team < 3; team++)
				materials[id][team].SetColor("_Color", Data.TeamColor.Current[team]);
	}

	public static void RefreshRibbonTextureOffset()
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
		transform.FindChild("Accessory").GetComponent<MeshRenderer>().material = materials[1][team];
		transform.FindChild("Base").GetComponent<MeshRenderer>().material = materials[0][team];
		transform.FindChild("Cannon").GetComponent<MeshRenderer>().materials = new[] { materials[1][team], materials[2][team] };
	}

	protected override void UpdateInfo()
	{
		base.UpdateInfo();
		team = Mathf.RoundToInt(_info["team"].n);
	}
}