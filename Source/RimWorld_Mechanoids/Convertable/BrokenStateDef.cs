using System.Collections.Generic;

namespace MoreMechanoids
{
    /// <summary>
    /// Hack to get rid of stupid warning "no letter bla"
    /// </summary>
    public class BrokenStateDef : Verse.MentalStateDef
    {
        public override IEnumerable<string> ConfigErrors()
        {
            yield break;
        }
    }
}