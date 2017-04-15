using System.Linq;
using Microsoft.SqlServer.Server;
using RimWorld;
using UnityEngine;
using Verse;

namespace MoreMechanoids
{
    public class MainTabWindow_Mechanoids : MainTabWindow_PawnList
    {
        private const float TopAreaHeight = 65f;
        private const float StayHomeWidth = 110f;
        private const float FullRepairWidth = 110f;
        private const float WorkTypeWidth = 110f;
        private const float AreaAllowedWidth = 350f;

        public override Vector2 RequestedTabSize => new Vector2(1010f, TopAreaHeight + this.PawnsCount * 30f + TopAreaHeight);
        protected override void BuildPawnList() => this.pawns = (from p in Find.World.worldPawns.AllPawnsAliveOrDead.Where((Pawn p) => p.Faction.Equals(Faction.OfPlayer)).OfType<PawnConverted>()
                                                                 orderby p.WorkType, p.NameStringShort
                                                                 select (Pawn)p).ToList<Pawn>();

        public override void DoWindowContents(Rect fillRect)
        {
            base.DoWindowContents(fillRect);
            Rect position = new Rect(0f, 0f, fillRect.width, TopAreaHeight);
            GUI.BeginGroup(position);
            float num = 175f;

            //Text.Font = GameFont.Tiny;
            //Text.Anchor = TextAnchor.LowerLeft;
            //Rect rect5 = new Rect(num, 0f, WorkTypeWidth, position.height + 3f);
            //Widgets.Label(rect5, "WorkTypeHeading".Translate());
            num += StayHomeWidth;

            if (this.pawns.OfType<PawnConverted>().Any())
            {
                Rect rect6 = new Rect(num, position.height/2f + 6, StayHomeWidth, position.height/2f);
                Rect rect7 = rect6.ContractedBy(2f);
                Text.Font = GameFont.Small;
                bool stayHomeAllOld = this.pawns.OfType<PawnConverted>().All(p => p.stayHome);
                bool stayHomeAll = stayHomeAllOld;
                Widgets.CheckboxLabeled(rect7, string.Empty, ref stayHomeAll);
                if (stayHomeAll != stayHomeAllOld)
                {
                    foreach (PawnConverted pawn in this.pawns.OfType<PawnConverted>())
                    {
                        pawn.SetStayHome(stayHomeAll);
                    }
                }
            }
            //Widgets.Label(rect, "Stay At Home".Translate());
            num += StayHomeWidth;

            if (this.pawns.OfType<PawnConverted>().Any())
            {
                Rect rect4 = new Rect(num, position.height/2f + 6, StayHomeWidth, position.height/2f);
                Rect rect5 = rect4.ContractedBy(2f);
                Text.Font = GameFont.Small;
                bool fullRepairAllOld = this.pawns.OfType<PawnConverted>().All(p => p.fullRepair);
                bool fullRepairAll = fullRepairAllOld;
                Widgets.CheckboxLabeled(rect5, string.Empty, ref fullRepairAll);
                if (fullRepairAll != fullRepairAllOld)
                {
                    foreach (PawnConverted pawn in this.pawns.OfType<PawnConverted>())
                    {
                        pawn.SetFullRepair(fullRepairAll);
                    }
                }
            }

            //Widgets.Label(rect4, "Full Repair".Translate());
            num += FullRepairWidth;

            Rect rect2 = new Rect(num, 0f, AreaAllowedWidth, Mathf.Round(position.height/2f));
            Text.Font = GameFont.Small;
            if (Widgets.ButtonText(rect2, "ManageAreas".Translate()))
            {
                Find.WindowStack.Add(new Dialog_ManageAreas(Find.VisibleMap));
            }

            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.LowerCenter;
            Rect rect3 = new Rect(num, 0f, AreaAllowedWidth, position.height + 3f);
            Widgets.Label(rect3, "AllowedArea".Translate());
            num += AreaAllowedWidth;

            GUI.EndGroup();
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            Rect outRect = new Rect(0f, position.height, fillRect.width, fillRect.height - position.height);
            DrawRows(outRect);
        }

        protected override void DrawPawnRow(Rect rect, Pawn p)
        {
            PawnConverted pawn = (PawnConverted) p;
            GUI.BeginGroup(rect);
            float num = 175f;

            Rect rect7 = new Rect(num, 0f, WorkTypeWidth, rect.height);
            Rect rect8 = rect7.ContractedBy(2f);
            Text.Font = GameFont.Small;
            WorkTypeDef workType = DefDatabase<WorkTypeDef>.GetNamed(pawn.WorkType);
            //if (workType!=null)
            {
                Widgets.Label(rect8, workType.pawnLabel);
            }
            num += WorkTypeWidth;

            Rect rect2 = new Rect(num, 0f, StayHomeWidth, rect.height);
            Rect rect3 = rect2.ContractedBy(2f);
            Text.Font = GameFont.Small;
            Widgets.CheckboxLabeled(rect3, "StayHomeCheck".Translate(), ref pawn.stayHome, pawn.Crashed);
            num += StayHomeWidth;

            Rect rect4 = new Rect(num, 0f, FullRepairWidth, rect.height);
            Rect rect5 = rect4.ContractedBy(2f);
            Text.Font = GameFont.Small;
            Widgets.CheckboxLabeled(rect5, "FullRepairCheck".Translate(), ref pawn.fullRepair, pawn.Crashed);
            num += FullRepairWidth;
            
            Rect rect6 = new Rect(num, 0f, AreaAllowedWidth, rect.height);
            AreaAllowedGUI.DoAllowedAreaSelectors(rect6, p, AllowedAreaMode.Humanlike);
            num += AreaAllowedWidth;
            
            GUI.EndGroup();
        }
    }
}
