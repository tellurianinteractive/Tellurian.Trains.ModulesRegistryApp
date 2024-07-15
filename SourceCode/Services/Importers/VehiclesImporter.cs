namespace ModulesRegistry.Services.Importers;

using System.Diagnostics;
using Column = (string Name, int Index);

/// <summary>
/// Imports loco data from CSV into my vehicles.
/// </summary>
public sealed class VehiclesImporter(IDbContextFactory<ModulesDbContext> factory)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;

    public async Task<int> ImportLocos(string csvFilePathname, CancellationToken cancellationToken)
    {
        var vehicles = await ImportFromCsv(csvFilePathname, cancellationToken);
        if (vehicles.Any()) return await SaveVehiclesAsync(vehicles, cancellationToken);
        return 0;
    }

    public async Task<IEnumerable<Vehicle>> ImportFromCsv(string csvFilePathname, CancellationToken cancellationToken)
    {
        if (Path.Exists(csvFilePathname))
        {
            var lines = await File.ReadAllLinesAsync(csvFilePathname, cancellationToken);
            if (lines.Length > 1)
            {
                using var db = await Factory.CreateDbContextAsync(cancellationToken);
                var vehicleFeatures = await db.VehicleFeatures.ToReadOnlyListAsync();

                var vehicles = new List<Vehicle>(lines.Length - 1);
                var columns = IndexColumns(lines[0]);
                foreach (var line in lines[1..])
                {
                    var fields = SplitLineOfData(line);
                    vehicles.Add(CreateVehicle(fields, columns, vehicleFeatures));
                }
                return vehicles;
            }
        }
        return [];
    }

    private async Task<int> SaveVehiclesAsync(IEnumerable<Vehicle> vehicles, CancellationToken cancellationToken)
    {
        using var db = await Factory.CreateDbContextAsync(cancellationToken);
        foreach (var vehicle in vehicles) { db.Vehicles.Add(vehicle); }
        return await db.SaveChangesAsync(cancellationToken);
    }


    private static Vehicle CreateVehicle(string[] fields, Column[] columns, List<VehicleFeature> features)
    {
        var result = new Vehicle
        {
            OwningPersonId = 19
        };
        foreach (var column in columns)
        {
            switch (column.Name)
            {
                case "Id":
                    result.InventoryNumber =int.Parse(fields[column.Index]);
                    break;
                case "Operator":
                    result.KeeperSignature = fields[column.Index];
                    break;
                case "Class":
                    result.VehicleClass = fields[column.Index];
                    break;
                case "VehicleNumber":
                    result.VehicleNumber = fields[column.Index];
                    break;
                case "PrototypeManufacturer":
                    result.PrototypeManufacturerName = fields[column.Index];
                    break;
                case "Traction":
                    result.TractionFeatureId = features.SingleOrDefault(f => f.Description.Equals(fields[column.Index], StringComparison.OrdinalIgnoreCase))?.Id;
                    break;
                case "Theme":
                    result.Theme = fields[column.Index];
                    break;
                case "FromYear":
                    if (short.TryParse(fields[column.Index], out short fromYear)) result.ThisEmbodiementFromYear = fromYear;
                    break;
                case "UptoYear":
                    if (short.TryParse(fields[column.Index], out short uptoYear)) result.ThisEmbodiementFromYear = uptoYear;
                    break;
                case "ScaleId":
                    result.ScaleId = int.Parse(fields[column.Index]);
                    break;
                case "ModelManufacturer":
                    result.ModelManufacturerName = fields[column.Index];
                    break;
                case "CatalogueNumber":
                    result.ModelNumber = fields[column.Index];
                    break;
                case "Couplings":
                    result.CouplingFeatureId = features.SingleOrDefault(f => f.Description.Equals(fields[column.Index], StringComparison.OrdinalIgnoreCase))?.Id;
                    break;
                case "Wheels":
                    result.CouplingFeatureId = features.SingleOrDefault(f => f.Description.Equals(fields[column.Index], StringComparison.OrdinalIgnoreCase))?.Id;
                    break;
                case "Decoder":
                    result.DecoderType = fields[column.Index];
                    break;
                case "Address":
                    if (short.TryParse(fields[column.Index], out short address)) result.DccAddress = address;
                    break;
                case "Sound":
                    result.HasSound = fields[column.Index] != "0";
                    break;
                case "RemoteCouplings":
                    result.HasRemoteCouplings = fields[column.Index] != "0";
                    break;
                case "IsWeathered":
                    result.IsWeathered = fields[column.Index] != "0";
                    break;
                case "Note":
                    result.Note = fields[column.Index];
                    break;
            }
        }
        return result;
    }

    Column[] IndexColumns(string columnHeader)
    {
        var items = SplitLineOfData(columnHeader);
        var columns = new List<Column>();
        for (var i = 0; i < items.Length; i++)
        {
            if (ColumnNames.Contains(items[i])) columns.Add(new Column(items[i], i));
        }
        return [.. columns];
    }

    string[] SplitLineOfData(string line)
    {
        var items = line.Split(';');
        var i = 0;
        try
        {
            for (i = 0; i < items.Length; i++)
            {
                if (items[i].Length == 0) continue;
                if (items[i][0] == '"')
                {
                    if (items[i].Length <= 2)
                    {
                        items[i] = string.Empty;
                    }
                    else
                    {
                        items[i] = items[i][1..^1];
                    }
                }

            }
        }
        catch (Exception ex)
        {
            Debugger.Break();
            throw;
        }
        return items;
    }

    private static string[] ColumnNames = ["Id", "Operator", "Class", "VehicleNumber", "PrototypeManufacturer", "Traction", "Theme", "FromYear", "UptoYear", "ScaleId", "ModelManufacturer", "CatalogueNumber", "Couplings", "Wheel", "Decoder", "Address", "Sound", "RemoteCouplings", "IsWeathered", "Note"];
}
