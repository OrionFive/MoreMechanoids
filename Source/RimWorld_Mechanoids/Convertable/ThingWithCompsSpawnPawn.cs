using RimWorld;
using Verse;

namespace MoreMechanoids
{
    public class ThingWithCompsSpawnPawn : ThingWithComps
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            ThingSpawnPawnDef thingDef = (ThingSpawnPawnDef) def;
            PawnConverted newPawn = (PawnConverted) PawnGenerator.GeneratePawn(thingDef.spawnPawnDef, Faction.OfPlayer);
            IntVec3 pos = CellFinder.RandomClosewalkCellNear(Position, map, 2);
            newPawn.workTypes = thingDef.workTypes;
            GenSpawn.Spawn(newPawn, pos, map);

            TaleRecorder.RecordTale(TaleDef.Named("CreatedMechanoid"), newPawn);

            Destroy();
        }
    }
}
