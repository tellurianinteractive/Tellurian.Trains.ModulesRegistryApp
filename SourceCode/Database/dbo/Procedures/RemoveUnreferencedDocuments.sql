-- Maintenance note!
-- Add select statements for every document reference in any table.
CREATE PROCEDURE [dbo].[RemoveUnreferencedDocuments]
AS
	DELETE FROM Document WHERE Id IN 
	(
		SELECT Document.Id AS Id   
		FROM Document LEFT JOIN 
		(
			SELECT DwgDrawingId AS Id FROM Module UNION
			SELECT SkpDrawingId FROM Module UNION
			SELECT PdfDocumentationId FROM Module UNION
			SELECT PdfInstructionId FROM Station
			-- Add additional SELECT of document references here
		) 
		AS DocumentRef ON Document.Id = DocumentRef.Id
		WHERE DocumentRef.Id IS NULL
	)
RETURN 0
