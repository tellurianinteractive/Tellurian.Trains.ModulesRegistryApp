-- One-off (2026-11-25): gives modules and layouts the theme of their standard, after the columns Module.PrimaryThemeId and
-- Layout.ThemeId are added with the default 1 (Europe). Today the theme is the standard's MainThemeId (1 Europe, 2 US); a standard
-- without a theme (0) counts as Europe, as the waybill views do. The secondary theme is set by the owners, never here.
-- Only rows that differ are changed, so it is safe to run again.
UPDATE M
SET M.PrimaryThemeId = CASE WHEN MS.MainThemeId > 0 THEN MS.MainThemeId ELSE 1 END
FROM dbo.Module M
INNER JOIN dbo.ModuleStandard MS ON MS.Id = M.StandardId
WHERE M.PrimaryThemeId <> CASE WHEN MS.MainThemeId > 0 THEN MS.MainThemeId ELSE 1 END;

UPDATE L
SET L.ThemeId = CASE WHEN MS.MainThemeId > 0 THEN MS.MainThemeId ELSE 1 END
FROM dbo.Layout L
INNER JOIN dbo.ModuleStandard MS ON MS.Id = L.PrimaryModuleStandardId
WHERE L.ThemeId <> CASE WHEN MS.MainThemeId > 0 THEN MS.MainThemeId ELSE 1 END;