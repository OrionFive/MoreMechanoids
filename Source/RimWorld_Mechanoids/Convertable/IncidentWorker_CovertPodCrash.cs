using System.Linq;
using RimWorld;
using Verse;

namespace MoreMechanoids
{
    public class IncidentWorker_CovertPodCrash : IncidentWorker
    {
        protected override bool StorytellerCanUseNowSub()
        {
            return Find.ListerPawns.FreeColonistsCount > 4
                   && Find.ListerThings.ThingsOfDef(ThingDef.Named("MechanoidCovert")).Count <= Find.ListerPawns.FreeColonistsCount/4;
        }

        public override bool TryExecute(IncidentParms parms)
        {
            IntVec3 intVec = DropCellFinder.RandomDropSpot();
            Find.LetterStack.ReceiveLetter("LetterLabelRefugeePodCrash".Translate(), "RefugeePodCrash".Translate(),
                LetterType.BadNonUrgent, intVec);
            Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Spacer);
            Pawn pawn = PawnGenerator.GeneratePawn(Verse.PawnKindDef.Named("MechanoidCovert"), faction);
            pawn.gender = Gender.Female;
            FixBackstory(pawn);
            FixHair(pawn);

            pawn.Name = NameGenerator.GenerateName(pawn);
            HealthUtility.GiveInjuriesToForceDowned(pawn);
            DropPodUtility.MakeDropPodAt(intVec,
                new DropPodInfo {SingleContainedThing = pawn, openDelay = 180, leaveSlag = true});
            return true;
        }

        private static void FixBackstory(Pawn pawn)
        {
            int tries = 100;

            while (pawn.story.BodyType != BodyType.Female && pawn.story.BodyType != BodyType.Thin)
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