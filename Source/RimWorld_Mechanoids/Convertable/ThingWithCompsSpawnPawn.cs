using RimWorld;
using Verse;

namespace MoreMechanoids
{
    public class ThingWithCompsSpawnPawn : ThingWithComps
    {
        public override void SpawnSetup()
        {
            base.SpawnSetup();

            var thingDef = (ThingSpawnPawnDef) def;
            var newPawn = (PawnConverted) PawnGenerator.GeneratePawn(thingDef.spawnPawnDef, Faction.OfColony);
            IntVec3 pos = CellFinder.RandomClosewalkCellNear(Position, 2);
            newPawn.workTypes = thingDef.workTypes;
            GenSpawn.Spawn(newPawn, pos);

            TaleRecorder.RecordTale(TaleDef.Named("CreatedMechanoid"), new object[] {newPawn});

            Destroy();
        }
    }
}
