using System.Linq;
using RimWorld;
using Verse;

namespace MoreMechanoids
{
    public class PAL_TalkTracker : Pawn_TalkTracker
    {
        private Building_PAL_Core PAL;

        public const int MinPreferredTalkInterval = 100;
        private int lastBeChattyTick = -1;
        private int lastTalkTime = -9999;
        public new const int ChattyModeDuration = 60;

        public PAL_TalkTracker(Building_PAL_Core core) : base(null)
        {
            PAL = core;
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
            if (!PAL.IsHashIntervalTick(60) || !Rand.MTBEventOccurs(!ChattyNow ? 5000f : 500f, 1f, 60f)) return;
            TryDoSocialChat();
        }

        public new bool TryDoSocialChat()
        {
            if (PAL == null) Log.ErrorOnce("PAL is already null...", 927594);
            if (Find.TickManager.TicksGame < lastTalkTime + MinPreferredTalkInterval) return false;
            if (PAL == null || PAL.Destroyed || !PAL.HasPower) return false;

            var source = Find.ListerPawns.AllPawns.Where(TalkUtility.CanTalk).ToArray();
            foreach (Pawn current in source.InRandomOrder())
            {
                if (current == null || !current.SpawnedInWorld) continue;

                SpeechMessage speech = new SpeechMessage( // TODO: hook up trait for hating mechanoids
                    false ? ThoughtDefOf.AbrasiveTalk : ThoughtDefOf.SocialTalk);

                if (TryTalkTo(speech, current))
                {
                    return true;
                }
            }
            return false;
        }

        private bool CanTalkTo(Pawn target)
        {
            if (target == null || target.talker == null) return false;
            return (PAL.Position - target.Position).LengthHorizontalSquared <= MaxTalkRange*MaxTalkRange
                   && GenSight.LineOfSight(PAL.Position, target.Position, true) && TalkUtility.CanTalk(target)
                   && !InStandby(target);
        }

        public new bool TryTalkTo(SpeechMessage speech, Pawn talkee)
        {
            if (!CanTalkTo(talkee)) return false;
            if (speech.ThoughtDef != null && talkee.needs != null && talkee.needs.mood != null
                && talkee.needs.mood.thoughts != null)
            {
                Thought_Memory thoughtMemory = (Thought_Memory) ThoughtMaker.MakeThought(speech.ThoughtDef);
                thoughtMemory.powerFactor = 1f;
                talkee.needs.mood.thoughts.TryGainThought(thoughtMemory);
            }

            MakeSpeechOverlay(PAL);
            lastTalkTime = Find.TickManager.TicksGame;
            return true;
        }

        public static void MakeSpeechOverlay(Thing talker)
        {
            MoteAttached moteAttached =
                (MoteAttached) ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("Mote_Speech"));
            moteAttached.ScaleUniform = 1.25f;
            moteAttached.AttachTo(talker);
            GenSpawn.Spawn(moteAttached, talker.Position);
        }

        public new void BeChatty()
        {
            lastBeChattyTick = Find.TickManager.TicksGame;
        }
    }
}
