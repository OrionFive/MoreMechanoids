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

        private bool Resting => Find.TickManager.TicksGame < this.lastRestTick + 2;

        public Need_Rest_Mechanoid(Pawn pawn)
            : base(pawn)
        {
            this.CurLevel = 0.5f;
        }

        public override void NeedInterval() => this.CurLevel = 0.5f;

        public void TickResting() => this.lastRestTick = Find.TickManager.TicksGame;

        public override string GetTipString() => this.txtTooltip;
    }
}

public static class NeedsHelper
{
    public static void AddNeed(this Pawn_NeedsTracker needs, NeedDef def)
    {
        needs.GetType()
            .GetMethod("AddNeed", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(needs, new object[] {def});
        foreach (Need need in needs.AllNeeds)
        {
            Log.Message(need.LabelCap);
        }
        Log.Message("---");
    }
}