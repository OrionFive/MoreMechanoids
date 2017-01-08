using System.Linq;
using Verse;

namespace MoreMechanoids
{
    public class PlaceWorker_OnLink : PlaceWorker
    {
        private static readonly string txtPlaceOnLink = "PlaceOnLink".Translate();
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot)
        {
            //IntVec3 c = loc + rot.FacingSquare * -1;
            //if (!c.InBounds())
            //{
            //    return false;
            //}
            var link = loc.GetThingList().FirstOrDefault(t => t.def == Defs.LinkDef);
            if (link == null)
            {
                return txtPlaceOnLink;
            }
            return true;
        }
    }
}
