using HarmonyLib;
using RimWorld;
using Verse;

namespace MoreMechanoids.Harmony;

internal static class Pawn_MeleeVerbs_Patch
{
    [HarmonyPatch(typeof(Pawn_MeleeVerbs), "ChooseMeleeVerb")]
    public class ChooseMeleeVerb
    {
        [HarmonyPrefix]
        internal static bool Prefix(Pawn ___pawn, ref Pawn_MeleeVerbs __instance, Thing target)
        {
            if (___pawn.def.defName != "Mech_Skullywag")
            {
                return true;
            }

            Verb result = null;
            if (target is Pawn pawn && GenericUtility.ValidSkullywagTargetPawn(pawn))
            {
                result = ___pawn.verbTracker.AllVerbs.FirstOrDefault(verb => verb is Verb_ParalyzingPoke);
            }

            if (result == null && target.CanBeForcedOpen())
            {
                result = ___pawn.verbTracker.AllVerbs.FirstOrDefault(verb => verb is Verb_UnlockDoor);
            }

            if (result == null)
            {
                result = ___pawn.verbTracker.AllVerbs.RandomElement();
            }

            __instance.SetCurMeleeVerb(result, target);
            return false;
        }
    }
}