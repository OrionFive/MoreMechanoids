using RimWorld;
using Verse;

namespace MoreMechanoids
{
    public class PawnColumnWorker_StayHome : PawnColumnWorker_Checkbox
    {
        protected override string GetTip(Pawn pawn)
        {
            return "StayHomeCheck".Translate();
        }

        protected override bool HasCheckbox(Pawn pawn)
        {
            return pawn is PawnConverted && pawn.Faction == Faction.OfPlayer && pawn.SpawnedOrAnyParentSpawned;
        }

        protected override bool GetValue(Pawn pawn)
        {
            var converted = pawn as PawnConverted;
            return converted != null && converted.stayHome;
        }

        protected override void SetValue(Pawn pawn, bool value)
        {
            if (value == GetValue(pawn))
            {
                return;
            }
            var converted = pawn as PawnConverted;
            if (converted == null) return;
            converted.SetStayHome(value);
        }
    }
}