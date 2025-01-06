BEGIN TRANSACTION;
GO

ALTER TABLE [Supplies] ADD [DateEdited] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
GO

UPDATE master.dbo.[Supplies] 
SET master.dbo.Supplies.DateEdited = master.dbo.Supplies.Date
GO

ALTER TABLE [Orders] ADD [DateEdited] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
GO
UPDATE master.dbo.[Orders]
SET master.dbo.Orders.DateEdited = master.dbo.Orders.Date

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20241215151513_addDateEditedFields', N'9.0.0-preview.3.24172.4');
GO

COMMIT;
GO

