using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace MoreMechanoids.Harmony;

/// <summary>
///     Don't spawn weak mechanoids in ancient danger site
/// </summary>
public class SymbolResolver_RandomMechanoidGroup_Patch
{
    [HarmonyPatch(typeof(SymbolResolver_RandomMechanoidGroup), nameof(SymbolResolver_RandomMechanoidGroup.Resolve))]
    public class Resolve
    {
        private static FloatRange combatPower = new(150, 500);

        [HarmonyPostfix]
        internal static void Postfix()
        {
            // We go through the stack to check all the Mechanoids that have been pushed
            CheckStackItemRecursive();
        }

        private static void CheckStackItemRecursive()
        {
            // Done?
            if (BaseGen.symbolStack.Empty)
            {
                return;
            }

            // Get top item
            var symbol = BaseGen.symbolStack.Pop();

            // Is it a mechanoid?
            if (symbol.symbol == "pawn" && symbol.resolveParams.faction == Faction.OfMechanoids)
            {
                if (!combatPower.Includes(symbol.resolveParams.singlePawnKindDef.combatPower))
                {
                    symbol.resolveParams.singlePawnKindDef = DefDatabase<PawnKindDef>.AllDefsListForReading
                        .Where(kind => kind.RaceProps.IsMechanoid && combatPower.Includes(kind.combatPower))
                        .RandomElementByWeight(kind => 1f / kind.combatPower);
                }

                // Check the next item
                CheckStackItemRecursive();
            }

            // Put the item back on the stack
            BaseGen.symbolStack.Push(symbol.symbol, symbol.resolveParams, symbol.symbolPath);
        }
    }
}