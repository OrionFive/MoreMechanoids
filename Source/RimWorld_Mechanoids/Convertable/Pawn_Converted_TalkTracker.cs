using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MoreMechanoids
{
    /*
    public class Pawn_Converted_TalkTracker : Pawn_InteractionsTracker
    {
        public const int MinPreferredTalkInterval = 700;
        private Pawn pawn;
        private int lastBeChattyTick = -1;
        private int lastTalkTime = -9999;
        public new const int ChattyModeDuration = 60;

        private List<Pawn> workingList = new List<Pawn>();

        public Pawn_Converted_TalkTracker(Pawn pawn) : base(pawn)
        {
            this.pawn = pawn;
        }

        private static bool InStandby(Pawn target)
        {
            var converted = target as PawnConverted;
            if (converted == null) return false;
            return converted.InStandby;
        }

        private bool ChattyNow { get { return Find.TickManager.TicksGame - lastBeChattyTick < ChattyModeDuration; } }

        public new void ExposeData()
        {
            Scribe_Values.LookValue(ref lastTalkTime, "lastTalkTime", -9999);
        }

        public new void TalkTrackerTick()
        {
            if (!pawn.IsHashIntervalTick(60) || !Rand.MTBEventOccurs(!ChattyNow ? 5000f : 500f, 1f, 60f)) return;
            TryDoSocialChat();
        }

        public new bool TryDoSocialChat()
        {
            if (pawn == null) Log.ErrorOnce("pawn is already null...", 3489643);
            if (Find.TickManager.TicksGame < lastTalkTime + MinPreferredTalkInterval || (pawn.Downed)
                || !TalkUtility.CanTalk(pawn)) return false;

            List<Pawn> list = Find.ListerPawns.PawnsInFaction(pawn.Faction);
            workingList.Clear();
            foreach (Pawn p in list)
            {
                workingList.Add(p);
            }
            workingList.Shuffle();
            foreach (Pawn p in workingList)
            {
                if (p != pawn && CanTalkTo(p))
                {
                    ThoughtDef thoughtDef = ThoughtDefOf.AbrasiveTalk;
                    if (!pawn.talker.TryTalkTo(new SpeechMessage(thoughtDef), p))
                    {
                        Log.Error(pawn + " failed to talk to " + p);
                    }
                    pawn.skills.Learn(SkillDefOf.Social, 4f);
                    pawn.needs.mood.thoughts.TryGainThought(ThoughtDefOf.SocialTalk);
                    return true;
                }
            }
            return false;
        }

        public bool CanTalkTo(Pawn talkee)
        {
            if (talkee == null || talkee.talker == null) return false;
            return talkee.SpawnedInWorld
                   && (pawn.Position - talkee.Position).LengthHorizontalSquared <= MaxTalkRange*MaxTalkRange
                   && TalkUtility.CanTalk(pawn) && GenSight.LineOfSight(pawn.Position, talkee.Position, true)
                   && TalkUtility.CanListen(talkee) && !InStandby(pawn) && !InStandby(talkee);
        }

        public new bool TryTalkTo(SpeechMessage speech, Pawn talkee)
        {
            if (!CanTalkTo(talkee)) return false;
            if (speech.ThoughtDef != null && talkee.needs.mood != null)
            {
                Thought_Memory thoughtMemory = (Thought_Memory) ThoughtMaker.MakeThought(speech.ThoughtDef);
                thoughtMemory.powerFactor = 1f;
                talkee.needs.mood.thoughts.TryGainThought(thoughtMemory);
            }
            MoteThrower.MakeSpeechOverlay(pawn);
            lastTalkTime = Find.TickManager.TicksGame;
            return true;
        }

        public new void BeChatty()
        {
            lastBeChattyTick = Find.TickManager.TicksGame;
        }
    }*/
}
