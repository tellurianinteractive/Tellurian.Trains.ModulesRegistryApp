using System;
using System.Collections.Generic;

#nullable disable

namespace ModulesRegistry.Data
{
    public class MeetingParticipant
    {
        public MeetingParticipant()
        {
            LayoutModules = new HashSet<LayoutModule>();
        }
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int MeetingId { get; set; }
        public DateTimeOffset RegistrationTime { get; set; }
        public DateTimeOffset? CancellationTime { get; set; }

        public virtual Person Person { get; set; }
        public virtual Meeting Meeting { get; set; }
        public virtual ICollection<LayoutModule> LayoutModules { get; set; }
    }
}
