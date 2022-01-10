using System;

#nullable disable
namespace ModulesRegistry.Data
{
    public class LayoutModule
    {
        public LayoutModule()
        {
        }
        public int Id { get; set; }
        public int LayoutParticipantId { get; set; }
        public int ModuleId { get; set; }
        public int? LayoutStationId { get; set; }
        public DateTimeOffset RegisteredTime { get; set; }
        public int RegistrationStatus { get; set; }
        public int? LayoutLineId { get; set; }
        public byte LayoutLinePosition { get; set; }
        public bool BringAnyway { get; set; }
        public string Note { get; set; }
        public virtual LayoutParticipant LayoutParticipant { get; set; }
        public virtual Module Module { get; set; }
        public virtual LayoutLine LayoutLine { get; set; }
        public virtual LayoutStation LayoutStation { get; set; }
    }
}
