using RimWorld;
using Verse;

namespace MoreMechanoids
{
    public class ThinkNode_ConditionalAIEnabled : ThinkNode_Conditional
    {
        public float threshold;

        protected override bool Satisfied(Pawn pawn)
        {
            var converted = pawn as PawnConverted;
            return converted == null || (!converted.Crashed &&!converted.fullRepair);
        }
    }
}
