-- One-off (2026-11-19): soft-deletes the user accounts that belong to no person and never signed in. They are left over from
-- persons removed long ago (8 in production, registered in 2022), and two of them had the same sign-in address, which made the
-- sign-in lookup fail for that address before release 1.15.17. The app ignores such accounts since 1.15.17 (SignInAccounts).
-- Run it once; it changes nothing the second time.
UPDATE U
SET U.DeletedTimestamp = SYSDATETIMEOFFSET()
FROM dbo.[User] U
WHERE U.DeletedTimestamp IS NULL
  AND U.LastSignInTime IS NULL
  AND NOT EXISTS (SELECT 1 FROM dbo.Person P WHERE P.UserId = U.Id);