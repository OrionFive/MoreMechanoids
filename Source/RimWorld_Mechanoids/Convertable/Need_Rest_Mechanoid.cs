using System.Reflection;
using RimWorld;
using Verse;

namespace MoreMechanoids
{
    public class Need_Rest_Mechanoid : Need_Rest
    {
        private readonly string txtTooltip = "BotTooltipRest".Translate();
        private readonly string txtLabel = "BotLabelRest".Translate();
        private int lastRestTick;

        private bool Resting
        {
            get
            {
                return Find.TickManager.TicksGame < lastRestTick + 2;
            }
        }

        public Need_Rest_Mechanoid(Pawn pawn)
            : base(pawn)
        {
            CurLevel = 0.5f;
        }

        public override void NeedInterval()
        {

            CurLevel = 0.5f;
            /*
            var bot = pawn as PawnConverted;
            if (bot.AIEnabled) return;

            if (Resting)
            {
                Need_Rest needRest = this;
                double num = needRest.CurLevel + 0.0114285722374916 * 150.0;
                needRest.CurLevel = (float)num;
                Log.Message(pawn.Name + ": rested " + (0.0114285722374916 * 150.0)+" = "+needRest.CurLevel);
            }
            else
            {
                Need_Rest needRest = this;
                double num = needRest.CurLevel - RestFallPerTick * 450.0;
                needRest.CurLevel = (float)num;
            }*/
        }

        public void TickResting()
        {
            lastRestTick = Find.TickManager.TicksGame;
        }

        public override string GetTipString()
        {
            return txtTooltip;
        }
    }
}

public static class NeedsHelper
{
    public static void AddNeed(this Pawn_NeedsTracker needs, NeedDef def)
    {
        needs.GetType()
            .GetMethod("AddNeed", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(needs, new object[] {def});
        foreach (var need in needs.AllNeeds)
        {
            Log.Message(need.LabelCap);
        }
        Log.Message("---");
    }
}