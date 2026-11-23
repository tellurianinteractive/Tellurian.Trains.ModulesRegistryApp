-- Run once in SQL Server Management Studio, after publishing the generation procedures that store the sending flow's days
-- (release 1.17.4). Gives the existing generated waybills the sending flow's days, as the procedures now do;
-- before, a waybill between two module stations had the receiving flow's days. Waybills added by hand are left as they are.

-- Another module station: the sending flow is the own flow if it sends, otherwise the other station's flow.
UPDATE W SET OperatingDayId = CASE WHEN OWN_DIRECTION.IsSupply <> 0 THEN OWN.OperatingDayId ELSE OTHER.OperatingDayId END
FROM StationCustomerWaybill AS W
    INNER JOIN StationCustomerCargo AS OWN ON OWN.Id = W.StationCustomerCargoId
    INNER JOIN CargoDirection AS OWN_DIRECTION ON OWN_DIRECTION.Id = OWN.DirectionId
    INNER JOIN StationCustomerCargo AS OTHER ON OTHER.Id = W.OtherStationCustomerCargoId
WHERE W.IsManuallyCreated = 0
    AND W.OperatingDayId <> CASE WHEN OWN_DIRECTION.IsSupply <> 0 THEN OWN.OperatingDayId ELSE OTHER.OperatingDayId END;

-- An external station: the sending flow is the own flow if it sends, otherwise the external station's flow.
UPDATE W SET OperatingDayId = CASE WHEN OWN_DIRECTION.IsSupply <> 0 THEN OWN.OperatingDayId ELSE OTHER.OperatingDayId END
FROM StationCustomerWaybill AS W
    INNER JOIN StationCustomerCargo AS OWN ON OWN.Id = W.StationCustomerCargoId
    INNER JOIN CargoDirection AS OWN_DIRECTION ON OWN_DIRECTION.Id = OWN.DirectionId
    INNER JOIN ExternalStationCustomerCargo AS OTHER ON OTHER.Id = W.OtherExternalCustomerCargoId
WHERE W.IsManuallyCreated = 0
    AND W.OperatingDayId <> CASE WHEN OWN_DIRECTION.IsSupply <> 0 THEN OWN.OperatingDayId ELSE OTHER.OperatingDayId END;

-- Shadow-yard waybills already have the own flow's days, which are also the shadow yard's.