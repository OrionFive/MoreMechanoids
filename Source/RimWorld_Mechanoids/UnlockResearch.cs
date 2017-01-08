using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace MoreMechanoids
{
    public class UnlockResearch : MapComponent
    {
        public UnlockResearch()
        {
            LockAllRecipes();
        }

        private static void LockAllRecipes()
        {
            foreach (var recipe in DefDatabase<RecipeDef>.AllDefs.Where(def => def.defName.StartsWith("MM_")))
            {
                LockRecipe(recipe);
            }
        }

        private static void LockRecipe(RecipeDef def)
        {
            def.recipeUsers = new List<ThingDef>();
        }

        private static void UnlockRecipe(string tableDefName, string defName)
        {
            var tableDef = DefDatabase<ThingDef>.GetNamed(tableDefName);

            var recipeDef = DefDatabase<RecipeDef>.GetNamed(defName);
            recipeDef.recipeUsers = new List<ThingDef> {tableDef};

            // Clear cache to update existing objects
            typeof (ThingDef).GetField("allRecipesCached", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(tableDef, null);
        }

        public static void CreateMechanoidChip()
        {
            UnlockRecipe("TableMachining", "MM_CreateMechanoidChip");
        }

        public static void ChipCrawler()
        {
            UnlockRecipe("TableMachining", "MM_ChipCrawlerCleaning");
            UnlockRecipe("TableMachining", "MM_ChipCrawlerHauling");
        }

        public static void ChipSkullywag()
        {
            UnlockRecipe("TableMachining", "MM_ChipSkullywagCooking");
            UnlockRecipe("TableMachining", "MM_ChipSkullywagDoctor");
        }

        public static void ChipScyther()
        {
            UnlockRecipe("TableMachining", "MM_ChipScytherCutting");
            UnlockRecipe("TableMachining", "MM_ChipScytherTailoring");
        }

        public static void ChipCentipede()
        {
            UnlockRecipe("TableMachining", "MM_ChipCentipedeGrowing");
            UnlockRecipe("TableMachining", "MM_ChipCentipedeMining");
        }
    }
}
