using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace MoreMechanoids
{
    public class ResearchProjectDef : Verse.ResearchProjectDef
    {
        public List<string> unlockedRecipeDefs;
        public string targetTableDef;

        public void UnlockRecipes()
        {
            if (unlockedRecipeDefs != null && !string.IsNullOrEmpty(targetTableDef))
            {
                foreach (var recipeDef in unlockedRecipeDefs)
                {
                    UnlockResearch.UnlockRecipe(targetTableDef, recipeDef);
                }
            }
        }
    }

    public class UnlockResearch : MapComponent
    {
        public UnlockResearch(Map map) : base(map)
        {
            LockAllRecipes();
        }

        private static void LockAllRecipes()
        {
            foreach (RecipeDef recipe in DefDatabase<RecipeDef>.AllDefs.Where(def => def.defName.StartsWith("MM_")))
            {
                LockRecipe(recipe);
            }
            Log.Message("All mechanoid recipes locked.");
        }

        private static void LockRecipe(RecipeDef def)
        {
            def.recipeUsers = new List<ThingDef>();
        }

        public static void UnlockRecipe(string tableDefName, string defName)
        {
            Log.Message("Unlocking recipe " + defName + " at " + tableDefName);
            ThingDef tableDef = DefDatabase<ThingDef>.GetNamed(tableDefName);

            RecipeDef recipeDef = DefDatabase<RecipeDef>.GetNamed(defName);
            recipeDef.recipeUsers = new List<ThingDef> {tableDef};

            // Clear cache to update existing objects
            typeof(ThingDef).GetField("allRecipesCached", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(tableDef, null);
        }
    }

    public class Unlock_CreateMechanoidChip : ResearchMod
    {
        public override void Apply()
        {
            UnlockResearch.UnlockRecipe("TableMachining", "MM_CreateMechanoidChip");
        }
    }

    public class Unlock_ChipCrawler : ResearchMod
    {
        public override void Apply()
        {
            UnlockResearch.UnlockRecipe("TableMachining", "MM_ChipCrawlerCleaning");
            UnlockResearch.UnlockRecipe("TableMachining", "MM_ChipCrawlerHauling");
        }
    }

    public class Unlock_ChipSkullywag : ResearchMod
    {
        public override void Apply()
        {
            UnlockResearch.UnlockRecipe("TableMachining", "MM_ChipSkullywagCooking");
            UnlockResearch.UnlockRecipe("TableMachining", "MM_ChipSkullywagDoctor");
        }
    }

    public class Unlock_ChipScyther : ResearchMod
    {
        public override void Apply()
        {
            UnlockResearch.UnlockRecipe("TableMachining", "MM_ChipScytherCutting");
            UnlockResearch.UnlockRecipe("TableMachining", "MM_ChipScytherTailoring");
        }
    }

    public class Unlock_ChipCentipede : ResearchMod
    {
        public override void Apply()
        {
            UnlockResearch.UnlockRecipe("TableMachining", "MM_ChipCentipedeGrowing");
            UnlockResearch.UnlockRecipe("TableMachining", "MM_ChipCentipedeMining");
        }
    }
}
