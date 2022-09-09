using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MoreMechanoidsDoorsExpanded
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        static HarmonyInit()
        {
            var harmony = new Harmony("Harmony_MoreMechanoidsDoorsExpanded");
            try
            {
                harmony.PatchAll(Assembly.GetExecutingAssembly());

            }
            catch (Exception e)
            {
                Log.Error($"Harmony: MoreMechanoidsDoorsExpanded patches failed {e}");
            }
        }
    }
}
