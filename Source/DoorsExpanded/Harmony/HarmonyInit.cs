using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace MoreMechanoidsDoorsExpanded;

[StaticConstructorOnStartup]
public static class HarmonyInit
{
    static HarmonyInit()
    {
        var harmony = new Harmony("Harmony_MoreMechanoidsDoorsExpanded");
        try
        {
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.Message("[More Mechanoids]: Patched Doors Expanded doors to allow forced opening");
        }
        catch (Exception e)
        {
            Log.Error($"Harmony: MoreMechanoidsDoorsExpanded patches failed {e}");
        }
    }
}