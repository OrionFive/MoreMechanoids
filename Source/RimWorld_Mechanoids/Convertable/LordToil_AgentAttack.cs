using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace MoreMechanoids
{
    public class LordToil_AgentAttack : LordToil
    {
        public override void Init()
        {
            base.Init();
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.Drafting, OpportunityType.Critical);
        }

        public override void UpdateAllDuties()
        {
            Log.Message("Pawns: "+ this.lord.ownedPawns.Count);
            foreach (Pawn p in this.lord.ownedPawns)
            {
                Log.Message(p.Name.ToStringFull);
                p.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("AgentAttack"));
            }
        }

        public override void LordToilTick() => base.LordToilTick();
    }
}