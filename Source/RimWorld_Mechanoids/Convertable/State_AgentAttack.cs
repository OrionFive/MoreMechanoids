using System.Linq;
using RimWorld;
using RimWorld.SquadAI;
using Verse;
using Verse.AI;

namespace MoreMechanoids
{
    public class State_AgentAttack : State
    {
        public override void Init()
        {
            base.Init();
            ConceptDecider.TeachOpportunity(ConceptDefOf.Drafting, OpportunityType.Critical);
        }

        public override void UpdateAllDuties()
        {
            Log.Message("Pawns: "+brain.ownedPawns.Count);
            foreach (Pawn p in brain.ownedPawns)
            {
                Log.Message(p.Name.ToStringFull);
                p.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("AgentAttack"));
            }
        }

        public override void StateTick()
        {
            base.StateTick();
        }
    }
}
