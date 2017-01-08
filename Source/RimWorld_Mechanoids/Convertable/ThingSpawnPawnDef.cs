using System.Collections.Generic;
using Verse;

namespace MoreMechanoids
{
    public class ThingSpawnPawnDef : ThingDef
    {
        /// <summary>
        /// What to spawn.
        /// </summary>
        public Verse.PawnKindDef spawnPawnDef;
        /// <summary>
        /// What work types the pawn can perform.
        /// </summary>
        public List<WorkTypeDef> workTypes;
    }
}