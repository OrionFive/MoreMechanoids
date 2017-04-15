using RimWorld;
using Verse;

namespace MoreMechanoids
{
    public class ThinkNode_ConditionalStayHome : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            PawnConverted converted = pawn as PawnConverted;
            return converted != null && converted.stayHome;
        }
    }
}