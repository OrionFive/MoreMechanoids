using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MoreMechanoids
{
    public class Building_PAL_Connectable : Building
    {
        public Building_PAL_Core Pal { get; protected internal set; }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            if (Pal != null && !(this is Building_PAL_Core)) Pal.RecheckConnectables();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (this is Building_PAL_Core) yield break;

            if (Pal == null)
            {
                var gizmo = Building_PAL_Core.GetCommandNotConnected(232626335);
                yield return gizmo;
            }
            else
            {
                foreach (var gizmo in Pal.GetGizmosPAL())
                {
                    yield return gizmo;
                }
            }
        }
    }
}
