using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MoreMechanoids
{
    [StaticConstructorOnStartup]
    public class FixMechanoidCorpseDef
    {
        public static readonly CompProperties CompCorpse = new CompProperties(typeof(MechanoidAgentCorpseComp));

        static FixMechanoidCorpseDef()
        {
            // Special corpse, to avoid _Converted_Corpse
            FixConvertedCorpse();

            // Special corpse, to resurect killed covert agents
            FixAgentCorpse();
        }

        public static void FixAgentCorpse()
        {
            ThingDef def = ThingDef.Named("MechanoidCovert");
            if(!def.race.corpseDef.comps.Contains(CompCorpse))
                def.race.corpseDef.comps.Add(CompCorpse);
            def.race.corpseDef.tickerType = TickerType.Normal;
            //def.race.corpseDef.selectable = false;
            //def.race.corpseDef.category = ThingCategory.None;
            //def.race.corpseDef.size = new IntVec2(1, 1);
        }

        public static void FixConvertedCorpse()
        {
            IEnumerable<ThingDef> defs =
                DefDatabase<ThingDef>.AllDefs.Where(
                    def => def.defName.StartsWith("Mechanoid_") && def.defName.EndsWith("_Converted"));

            foreach (ThingDef def in defs)
            {
                def.race.corpseDef = ThingDef.Named(def.defName.Replace("_Converted", "_Corpse"));

                ThingDef oldDef = ThingDef.Named(def.defName + "_Corpse");
                oldDef.menuHidden = true;
            }
        }
    }
}