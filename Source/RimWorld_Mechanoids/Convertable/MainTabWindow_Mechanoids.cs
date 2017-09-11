using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreMechanoids
{
    public class MainTabWindow_Mechanoids : MainTabWindow_PawnTable
    {
        protected override PawnTableDef PawnTableDef
        { get { return DefDatabase<PawnTableDef>.GetNamed("Mechanoids"); } }

        protected override IEnumerable<Pawn> Pawns
        { get { return (from p in Find.VisibleMap.mapPawns.AllPawnsSpawned.OfType<PawnConverted>() where p.Faction.Equals(Faction.OfPlayer) orderby p.WorkType select (Pawn) p).ToList(); } }

        public override void PostOpen()
		{
			base.PostOpen();
			Find.World.renderer.wantedMode = WorldRenderMode.None;
		}
    }
}
