CREATE VIEW dbo.ReportStationCustomer
AS
SELECT dbo.CargoCustomer.Id, dbo.Station.FullName AS StationName, dbo.Station.Signature AS StationSignature, dbo.StationCustomer.CustomerName, dbo.Cargo.sv AS CargoName, COALESCE (dbo.CargoCustomer.TrackOrArea, 
                  dbo.StationCustomer.TrackOrArea, '') AS TrackOrArea, dbo.CargoCustomer.SpecialCargoName, dbo.CargoDirection.FullName AS CargoDirection, dbo.OperatingDay.FullName AS OperatingDay, dbo.CargoCustomer.Quantity, 
                  dbo.CargoReadyTime.FullName AS CargoReadyTime, COALESCE (dbo.CargoCustomer.FromYear, dbo.StationCustomer.OpenedYear, 0) AS FromYear, COALESCE (dbo.CargoCustomer.UptoYear, dbo.StationCustomer.ClosedYear, 9999) 
                  AS UptoYear, dbo.Cargo.DefaultClasses AS WagonClasses
FROM     dbo.StationCustomer INNER JOIN
                  dbo.CargoCustomer ON dbo.StationCustomer.Id = dbo.CargoCustomer.StationCustomerId INNER JOIN
                  dbo.Station ON dbo.StationCustomer.StationId = dbo.Station.Id INNER JOIN
                  dbo.CargoDirection ON dbo.CargoCustomer.DirectionId = dbo.CargoDirection.Id INNER JOIN
                  dbo.OperatingDay ON dbo.CargoCustomer.OperatingDayId = dbo.OperatingDay.Id INNER JOIN
                  dbo.CargoReadyTime ON dbo.CargoCustomer.ReadyTimeId = dbo.CargoReadyTime.Id INNER JOIN
                  dbo.Cargo ON dbo.CargoCustomer.CargoId = dbo.Cargo.Id

GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPane1', @value = N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[24] 2[16] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "StationCustomer"
            Begin Extent = 
               Top = 46
               Left = 654
               Bottom = 340
               Right = 852
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "CargoCustomer"
            Begin Extent = 
               Top = 70
               Left = 1043
               Bottom = 429
               Right = 1314
            End
            DisplayFlags = 280
            TopColumn = 5
         End
         Begin Table = "Station"
            Begin Extent = 
               Top = 41
               Left = 380
               Bottom = 280
               Right = 570
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "CargoDirection"
            Begin Extent = 
               Top = 174
               Left = 1513
               Bottom = 337
               Right = 1707
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "OperatingDay"
            Begin Extent = 
               Top = 358
               Left = 1642
               Bottom = 521
               Right = 1836
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "CargoReadyTime"
            Begin Extent = 
               Top = 447
               Left = 1388
               Bottom = 610
               Right = 1610
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Cargo"
            Begin Extent = 
               Top = 16
               Left = 1729
               Bottom = 276
               Right = 2071
    ', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'ReportStationCustomer';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPane2', @value = N'        End
            DisplayFlags = 280
            TopColumn = 1
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 13
         Width = 284
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 2040
         Width = 1200
         Width = 1200
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 3084
         Table = 3120
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1356
         SortOrder = 1416
         GroupBy = 1350
         Filter = 1356
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'ReportStationCustomer';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPaneCount', @value = 2, @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'ReportStationCustomer';

