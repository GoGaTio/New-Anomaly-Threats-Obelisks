using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.IO;
using RimWorld.Planet;
using RimWorld.QuestGen;
using RimWorld.SketchGen;
using RimWorld.Utility;
using LudeonTK;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Grammar;
using Verse.Noise;
using Verse.Profile;
using Verse.Sound;
using Verse.Steam;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace NAT
{
	public class Hediff_Calamity : HediffWithComps
	{
		public override void PostAdd(DamageInfo? dinfo)
		{
			base.PostAdd(dinfo);
			CalamityUtility.activeHediffs.Add(this);
		}

		public override void PreRemoved()
		{
			CalamityUtility.activeHediffs.Remove(this);
			base.PreRemoved();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			if(Scribe.mode == LoadSaveMode.LoadingVars)
			{
				CalamityUtility.activeHediffs.Clear();
			}
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				CalamityUtility.activeHediffs.Add(this);
			}
		}
    }
}