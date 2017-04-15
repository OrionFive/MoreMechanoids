using System.Linq;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace MoreMechanoids
{
    public class IncidentWorker_CovertPodCrash : IncidentWorker
    {
        protected override bool CanFireNowSub(IIncidentTarget target) => ((Map)target).mapPawns.FreeColonistsCount > 4
                   && ((Map)target).listerThings.ThingsOfDef(ThingDef.Named("MechanoidCovert")).Count <= ((Map)target).mapPawns.FreeColonistsCount / 4;

        public override bool TryExecute(IncidentParms parms)
        {
            IntVec3 intVec = DropCellFinder.RandomDropSpot((Map)parms.target);
            Find.LetterStack.ReceiveLetter("LetterLabelRefugeePodCrash".Translate(), "RefugeePodCrash".Translate(),
                LetterType.BadNonUrgent, new GlobalTargetInfo(intVec, (Map)parms.target));
            Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Spacer);
            Pawn pawn = PawnGenerator.GeneratePawn(Verse.PawnKindDef.Named("MechanoidCovert"), faction);
            pawn.gender = Gender.Female;
            FixBackstory(pawn);
            FixHair(pawn);

            pawn.Name = PawnBioAndNameGenerator.GeneratePawnName(pawn);
            HealthUtility.GiveInjuriesToForceDowned(pawn);
            DropPodUtility.MakeDropPodAt(intVec, (Map)parms.target, new ActiveDropPodInfo {SingleContainedThing = pawn, openDelay = 180, leaveSlag = true});
            return true;
        }

        private static void FixBackstory(Pawn pawn)
        {
            int tries = 100;

            while (pawn.story.bodyType != BodyType.Female && pawn.story.bodyType != BodyType.Thin)
            {
                //pawn.story.childhood = BackstoryDatabase.RandomBackstory(BackstorySlot.Childhood);
                pawn.story.adulthood = BackstoryDatabase.RandomBackstory(BackstorySlot.Adulthood);
                tries--;
                if (tries <= 0)
                {
                    Log.Error("Couldn't get backstory.");
                    break;
                }
            }
        }

        private static void FixHair(Pawn pawn)
        {
            int tries = 100;

            while (!pawn.story.hairDef.hairTags.Any(h => h == "Rural" || h == "Urban") || pawn.story.hairDef.hairGender!=HairGender.Female)
            {
                pawn.story.hairDef = PawnHairChooser.RandomHairDefFor(pawn, FactionDef.Named("Spacer"));
                tries--;
                if (tries <= 0)
                {
                    Log.Error("Couldn't get backstory.");
                    break;
                }
            }
        }
    }
}