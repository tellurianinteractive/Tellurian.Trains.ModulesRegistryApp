-- One-off, after publishing the NickName column (release 1.15.15, 2026-11-19): a nickname written in quotation marks as the
-- whole middle name moves to NickName. The app does the same, and more, whenever a person is saved (PersonExtensions.CorrectNames).
-- Run it once; it changes nothing the second time. On 2026-11-19 one person in production had this.
UPDATE dbo.Person
SET NickName = LTRIM(RTRIM(SUBSTRING(MiddleName, 2, LEN(MiddleName) - 2))),
    MiddleName = NULL
WHERE NickName IS NULL
  AND LEN(MiddleName) > 2
  AND LEFT(MiddleName, 1) = '"' AND RIGHT(MiddleName, 1) = '"'
  AND CHARINDEX('"', SUBSTRING(MiddleName, 2, LEN(MiddleName) - 2)) = 0;