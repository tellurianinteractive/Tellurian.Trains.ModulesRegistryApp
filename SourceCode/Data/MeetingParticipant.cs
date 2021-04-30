using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable disable

namespace ModulesRegistry.Data
{
    public class MeetingParticipant
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int MeetingId { get; set; }
        public DateTimeOffset RegistrationTime { get; set; }
        public DateTimeOffset? CancellationTime { get; set; }

        public virtual Person Person { get; set; }
        public virtual Meeting Meeting { get; set; }
    }
}
