using System;
using System.Collections.Generic;

#nullable disable
namespace ModulesRegistry.Data
{
    public class LayoutModule
    {
        public LayoutModule()
        {
        }
        public int Id { get; set; }
        public int LayoutId { get; set; }
        public int ModuleId { get; set; }
        public int ParticipantId { get; set; }
        public DateTimeOffset RegisteredTime { get; set; }
        public int RegistrationStatus { get; set; }
        public int? LayoutLineId { get; set; }
        public byte LayoutLinePosition { get; set; }
        public int? LayoutStationId { get; set; }
        public bool BringAnyway { get; set; }
        public string? Note { get; set; }
        public virtual Layout Layout { get; set; }
        public virtual Module Module { get; set; }
        public virtual MeetingParticipant Participant { get; set; }
        public virtual LayoutLine LayoutLine { get; set; }
        public virtual LayoutStation LayoutStation { get; set; }
    }
}
