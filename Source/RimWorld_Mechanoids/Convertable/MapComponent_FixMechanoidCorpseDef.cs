using System.Linq;
using Verse;

namespace MoreMechanoids
{
    public class MapComponent_FixMechanoidCorpseDef : MapComponent
    {
        public static readonly CompProperties CompCorpse = new CompProperties(typeof(MechanoidAgentCorpseComp));

        public MapComponent_FixMechanoidCorpseDef()
        {
            // Hack to process converted pawn stuff

            // Special corpse, to avoid _Converted_Corpse
            FixConvertedCorpse();

            // Special corpse, to resurect killed covert agents
            FixAgentCorpse();
        }

        public static void FixAgentCorpse()
        {
            var def = ThingDef.Named("MechanoidCovert");
            Log.Message(def.race.corpseDef.thingClass.Name);
            if(!def.race.corpseDef.comps.Contains(CompCorpse))
            def.race.corpseDef.comps.Add(CompCorpse);
            def.race.corpseDef.tickerType = TickerType.Normal;
            //def.race.corpseDef.selectable = false;
            //def.race.corpseDef.category = ThingCategory.None;
            //def.race.corpseDef.size = new IntVec2(1, 1);
        }

        public static void FixConvertedCorpse()
        {
            var defs =
                DefDatabase<ThingDef>.AllDefs.Where(
                    def => def.defName.StartsWith("Mechanoid_") && def.defName.EndsWith("_Converted"));

            foreach (var def in defs)
            {
                def.race.corpseDef = ThingDef.Named(def.defName.Replace("_Converted", "_Corpse"));

                var oldDef = ThingDef.Named(def.defName + "_Corpse");
                oldDef.menuHidden = true;
            }
        }
    }
}