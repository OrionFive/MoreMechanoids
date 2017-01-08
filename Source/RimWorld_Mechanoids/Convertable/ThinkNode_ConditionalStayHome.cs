using RimWorld;
using Verse;

namespace MoreMechanoids
{
    public class ThinkNode_ConditionalStayHome : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            var converted = pawn as PawnConverted;
            return converted != null && converted.stayHome;
        }
    }
}