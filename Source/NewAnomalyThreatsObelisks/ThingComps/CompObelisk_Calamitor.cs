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
	public class CompObelisk_Calamitor : CompObelisk_ExplodingSpawner
	{
		public List<Pawn> targets = new List<Pawn>();

		public int actionsCount;

		private static readonly SimpleCurve ActionsFromPoints = new SimpleCurve
		{
			new CurvePoint(0f, 3f),
			new CurvePoint(1000f, 5f),
			new CurvePoint(8000f, 9f),
			new CurvePoint(10000f, 10f)
		};

		public override void TriggerInteractionEffect(Pawn interactor, bool triggeredByPlayer = false)
		{
			
			string text = triggeredByPlayer ? "NAT_InducerTriggeredPlayer_Desc".Translate() : "NAT_InducerTriggered_Desc".Translate(interactor.Named("PAWN"));
			Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter("NAT_InducerTriggered_Label".Translate(), text, triggeredByPlayer ? LetterDefOf.NeutralEvent : LetterDefOf.ThreatSmall));
			
		}

		public override void OnActivityActivated()
		{
			base.OnActivityActivated();
			Find.LetterStack.ReceiveLetter("NAT_InducerObeliskLetterLabel".Translate(), "NAT_InducerObeliskLetter".Translate(), LetterDefOf.ThreatBig, parent);
			actionsCount = Mathf.FloorToInt(ActionsFromPoints.Evaluate(pointsRemaining));
		}

		public override void CompTick()
		{
			base.CompTick();
			if (activated && !base.ActivityComp.Deactivated && explodeTick <= 0 && Find.TickManager.TicksGame >= nextSpawnTick && warmupComplete)
			{
				
				nextSpawnTick = Find.TickManager.TicksGame + SpawnIntervalTicks.RandomInRange;
				if (actionsCount <= 0f)
				{
					PrepareExplosion();
				}
			}
		}

		public void AddCalamity(Pawn pawn, float power)
        {
			Hediff h = pawn.health.GetOrAddHediff(HediffDefOf.DarkPsychicShock);
			h.TryGetComp<HediffComp_Disappears>().ticksToDisappear = 60000;
		}
	}
}