using RimWorld;
using Verse;

namespace MoreMechanoids
{
    public class ThingWithCompsSpawnPawn : ThingWithComps
    {
        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);

            ThingSpawnPawnDef thingDef = (ThingSpawnPawnDef)this.def;
            PawnConverted newPawn = (PawnConverted) PawnGenerator.GeneratePawn(thingDef.spawnPawnDef, Faction.OfPlayer);
            IntVec3 pos = CellFinder.RandomClosewalkCellNear(this.Position, map, 2);
            newPawn.workTypes = thingDef.workTypes;
            GenSpawn.Spawn(newPawn, pos, map);

            TaleRecorder.RecordTale(TaleDef.Named("CreatedMechanoid"), new object[] {newPawn});

            Destroy();
        }
    }
}
