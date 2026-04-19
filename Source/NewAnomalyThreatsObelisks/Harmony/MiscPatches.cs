using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using DelaunatorSharp;
using Gilzoide.ManagedJobs;
using Ionic.Crc;
using Ionic.Zlib;
using JetBrains.Annotations;
using KTrie;
using LudeonTK;
using NVorbis.NAudioSupport;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.IO;
using RimWorld.Planet;
using RimWorld.QuestGen;
using RimWorld.SketchGen;
using RimWorld.Utility;
using RuntimeAudioClipLoader;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Grammar;
using Verse.Noise;
using Verse.Profile;
using Verse.Sound;
using Verse.Steam;
using HarmonyLib;

namespace NAT
{
	/*[HarmonyPatch(typeof(PsychicRitualToil_GatherForInvocation), "InvokerGatherPhaseToils")]
	public static class Patch_InvokerGatherPhaseToils
	{
		[HarmonyPostfix]
		public static IEnumerable<PsychicRitualToil> Postfix(IEnumerable<PsychicRitualToil> __result, PsychicRitualDef_InvocationCircle def)
		{
			if (def is PsychicRitualDef_AdditionalOfferings ritual)
			{
				yield return new PsychicRitualToil_BringAdditionalOfferings(ritual);
			}
			foreach (PsychicRitualToil toil in __result)
			{
				yield return toil;
			}
		}
	}*/

	[HarmonyPatch(typeof(ArmorUtility), nameof(ArmorUtility.GetPostArmorDamage))]
	public class Patch_GetPostArmorDamage
	{
		[HarmonyPrefix]
		public static bool Prefix(Pawn pawn, float amount, ref float __result)
		{
			if (pawn.AffectedByCalamity(out var power) && Rand.Chance(power))
			{
				__result = amount;
				Log.Message("Calamity: no armor");
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch]
	public static class CombatExtended_ArmorUtilityCE_GetAfterArmorDamage
	{
		public static MethodBase TargetMethod()
		{
			return AccessTools.Method("CombatExtended.ArmorUtilityCE:GetAfterArmorDamage");
		}

		public static bool Prepare(MethodBase method)
		{
			return AccessTools.Method("CombatExtended.ArmorUtilityCE:GetAfterArmorDamage") != null;
		}

		[HarmonyPrefix]
		public static bool Prefix(DamageInfo originalDinfo, Pawn pawn, ref DamageInfo __result)
		{
			if (pawn.AffectedByCalamity(out var power) && Rand.Chance(power))
			{
				__result = originalDinfo;
				Log.Message("Calamity: no armor CE");
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(DamageWorker_AddInjury), "ChooseHitPart")]
	public class Patch_ChooseHitPart
	{
		[HarmonyPrefix]
		public static bool Prefix(DamageInfo dinfo, Pawn pawn, ref BodyPartRecord __result)
		{
			if (pawn.AffectedByCalamity(out var power) && Rand.Chance(power))
			{
				__result = pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, BodyPartDepth.Inside);
				Log.Message("Calamity: " + __result);
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(Verb_LaunchProjectile), "TryCastShot")]
	public class Patch_TryCastShot_Test
	{
		[HarmonyPrefix]
		public static void Prefix()
		{
			Patch_HitReportFor.active = true;
		}

		[HarmonyPostfix]
		public static void Postfix()
		{
			Patch_HitReportFor.active = false;
		}
	}

	[HarmonyPatch(typeof(ShotReport), nameof(ShotReport.HitReportFor))]
	public class Patch_HitReportFor
	{
		public static bool active = false;

		public static FieldInfo offsetFromDarkness = AccessTools.Field(typeof(ShotReport), "offsetFromDarkness");

		public static FieldInfo coversOverallBlockChance = AccessTools.Field(typeof(ShotReport), "coversOverallBlockChance");

		[HarmonyPostfix]
		public static void Postfix(Thing caster, Verb verb, LocalTargetInfo target, ref ShotReport __result)
		{
			if (active)
			{
				string s = "Cast: ";
				if (caster is Pawn pawn && pawn.AffectedByCalamity(out var power) && Rand.Chance(power))
				{
					s += __result.AimOnTargetChance_IgnoringPosture + "/";
					AccessTools.StructFieldRefAccess<ShotReport, float>(ref __result, offsetFromDarkness) = -9999f;
					s += __result.AimOnTargetChance_IgnoringPosture;
					
				}
				else if (target.TryGetPawn(out var targetPawn) && targetPawn.AffectedByCalamity(out var targetPower) && Rand.Chance(targetPower))
				{
					s += __result.AimOnTargetChance_IgnoringPosture + "(" + __result.PassCoverChance + ")/";
					AccessTools.StructFieldRefAccess<ShotReport, float>(ref __result, offsetFromDarkness) = 9999f;
					AccessTools.StructFieldRefAccess<ShotReport, float>(ref __result, coversOverallBlockChance) = 0f;
					s += __result.AimOnTargetChance_IgnoringPosture + "(" + __result.PassCoverChance + ")";
				}
				Log.Message(s);
			}
		}
	}
}