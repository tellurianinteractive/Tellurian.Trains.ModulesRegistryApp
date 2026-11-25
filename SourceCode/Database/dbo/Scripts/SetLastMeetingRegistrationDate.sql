-- One-off (2026-11-25): sets Module.LastMeetingRegistrationDate from the meetings the modules are registered to, before the app
-- sets it on each registration. The value is the start date of the latest meeting, at midnight with offset zero, the same value
-- the app writes (ModuleMeetingRegistrationExtensions). Canceled meetings (Status 9) and canceled participants don't count.
-- A stored date is only replaced by a later one, so it is safe to run again; it changes nothing the second time.
UPDATE M
SET M.LastMeetingRegistrationDate = L.LastDate
FROM dbo.Module M
INNER JOIN (
    SELECT LM.ModuleId, TODATETIMEOFFSET(CAST(MAX(CAST(MT.StartDate AS DATE)) AS DATETIME2), '+00:00') AS LastDate
    FROM dbo.LayoutModule LM
    INNER JOIN dbo.LayoutParticipant LP ON LP.Id = LM.LayoutParticipantId
    INNER JOIN dbo.MeetingParticipant MP ON MP.Id = LP.MeetingParticipantId
    INNER JOIN dbo.Meeting MT ON MT.Id = MP.MeetingId
    WHERE MT.Status <> 9
      AND MP.CancellationTime IS NULL
    GROUP BY LM.ModuleId
) L ON L.ModuleId = M.Id
WHERE M.LastMeetingRegistrationDate IS NULL OR M.LastMeetingRegistrationDate < L.LastDate;