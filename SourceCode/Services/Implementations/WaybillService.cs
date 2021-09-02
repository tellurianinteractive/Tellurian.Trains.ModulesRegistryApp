using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Security.Claims;

namespace ModulesRegistry.Services.Implementations
{
    public class WaybillService
    {
        private readonly IDbContextFactory<ModulesDbContext> Factory;
        public WaybillService(IDbContextFactory<ModulesDbContext> factory)
        {
            Factory = factory;
        }

        public IEnumerable<Waybill>? GetWaybills(ClaimsPrincipal? principal, int layoutId, int? stationId, bool matchShadowYards = false, bool sending = true, bool receiving = true)
        {
            List<Waybill>? waybills = new List<Waybill>(200);
            if (principal.IsAuthenticated())
            {
                var dbContext = Factory.CreateDbContext();
                using var connection = dbContext.Database.GetDbConnection() as SqlConnection;
                if (connection is not null)
                {
                    var command = new SqlCommand("GetWaybills", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    command.Parameters.AddWithValue("@LayoutId", layoutId);
                    if (stationId.HasValue) command.Parameters.AddWithValue("@StationId", stationId);
                    command.Parameters.AddWithValue("@MatchShadowYard", matchShadowYards);
                    try
                    {
                        connection.Open();
                        var reader = command.ExecuteReader();
                        var resourceManager = new ResourceManager(typeof(Resources.Strings));
                        while (reader.Read())
                        {
                            waybills.Add(MapWaybill(reader, resourceManager));
                        }
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }

                }
            }
            return waybills;
        }

        private static Waybill MapWaybill(IDataRecord record, ResourceManager resourceManager)
        {
            string originLanguage = record.GetString("OriginLanguages", "EN").FirstItem("EN");
            string destinationLanguage = record.GetString("DestinationLanguages", "EN").FirstItem("EN");
            var originPackageUnits = EnumExtensions.CargoPackageUnitListboxItems(originLanguage);
            var destinationPackageUnits = EnumExtensions.CargoPackageUnitListboxItems(destinationLanguage);
            var wagonClass = record.GetString("DefaultClasses");
            var specialWagonClass = record.GetString("SpecificWagonClass");
            return new()
            {
                Origin = new CargoCustomer
                {
                    Name = record.GetString("Sender"),
                    StationName = record.GetString("OriginStationName"),
                    Language = originLanguage,
                    DomainSuffix = record.GetString("OriginDomainSuffix"),
                    ForeColor = record.GetString("OriginForeColor"),
                    BackColor = record.GetString("OriginBackColor"),
                    CargoName = record.GetString(originLanguage),
                    PackageUnitName = PackageUnitName(originPackageUnits, record.GetInt("OriginPackageUnitId")),
                    QuantityUnitName = record.GetStringResourceForLanguage("QuanityUnitResourceName", resourceManager, originLanguage)
                },
                Destination = new CargoCustomer
                {
                    Name = record.GetString("Receiver"),
                    StationName = record.GetString("DesinationStationName"),
                    Language = destinationLanguage,
                    DomainSuffix = record.GetString("DestinationDomainSuffix"),
                    ForeColor = record.GetString("DestinationForeColor"),
                    BackColor = record.GetString("DestinationBackColor"),
                    CargoName = record.GetString(destinationLanguage),
                    PackageUnitName = PackageUnitName(destinationPackageUnits, record.GetInt("DestinationPackageUnitId")),
                    QuantityUnitName = record.GetStringResourceForLanguage("QuanityUnitResourceName", resourceManager, destinationLanguage)
                },
                Quantity = record.GetInt("Quantity"),
                OperatorName = string.Empty, // To be supported
                WagonClass = string.IsNullOrWhiteSpace(specialWagonClass) ? wagonClass : specialWagonClass
            };
        }

        static string PackageUnitName(IEnumerable<ListboxItem>? items, int id) =>
            items is null || id == 0 ? string.Empty :
            items.SingleOrDefault(i => i.Id == id)?.Description ?? string.Empty;
    }

    public static class IDataRecordExtensions
    {
        public static string GetString(this IDataRecord me, string columnName, string defaultValue = "")
        {
            var i = me.GetColumIndex(columnName, false);
            if (i < 0 || me.IsDBNull(i)) return defaultValue;
            var s = me.GetString(me.GetOrdinal(columnName));
            return (string.IsNullOrWhiteSpace(s)) ? defaultValue : s;
        }

        //public static string GetStringResource(this IDataRecord me, string columnName, ResourceManager resourceManager, string defaultValue = "")
        //{
        //    var resourceKey = me.GetString(columnName, defaultValue);
        //    if (resourceKey.HasValue())
        //    {
        //        var resourceValue = resourceManager.GetString(resourceKey, CultureInfo.CurrentCulture);
        //        if (resourceValue.HasValue()) return resourceValue;
        //        return resourceKey;
        //    }
        //    return defaultValue;
        //}
        public static string GetStringResourceForLanguage(this IDataRecord me, string columnName, ResourceManager resourceManager, string language, string defaultValue = "")
        {
            var resourceKey = me.GetString(columnName, defaultValue);
            var culture = new CultureInfo(language);
            if (resourceKey.HasValue())
            {
                var resourceValue = resourceManager.GetString(resourceKey, culture);
                if (resourceValue.HasValue()) return resourceValue;
                return resourceKey;
            }
            return defaultValue;
        }

        public static byte GetByte(this IDataRecord me, string columnName)
        {
            var i = me.GetColumIndex(columnName);
            if (me.IsDBNull(i)) return 0;
            var value = me.GetValue(i);
            if (value is byte a) return a;
            if (value is double b) return (byte)b;
            throw new InvalidOperationException(columnName);
        }

        public static int GetInt(this IDataRecord me, string columnName, short defaultValue = 0)
        {
            var i = me.GetColumIndex(columnName, false);
            if (i < 0) return defaultValue;
            if (me.IsDBNull(i)) return defaultValue;
            var value = me.GetValue(i);
            if (value is int b) return b;
            if (value is short a) return a;
            throw new InvalidOperationException(columnName);
        }

        public static double GetDouble(this IDataRecord me, string columnName, double defaultValue = 0)
        {
            var i = me.GetColumIndex(columnName);
            if (me.IsDBNull(i)) return defaultValue;
            var value = me.GetValue(i);
            if (value is double b) return b;
            if (value is float a) return a;
            throw new InvalidOperationException(columnName);
        }

        public static string GetTime(this IDataRecord me, string columnName, string defaultValue = "")
        {
            var i = me.GetColumIndex(columnName);
            if (me.IsDBNull(i)) return defaultValue;
            return me.GetDateTime(i).ToString("HH:mm", CultureInfo.InvariantCulture);
        }

        public static TimeSpan GetTimeAsTimespan(this IDataRecord me, string columnName)
        {
            var i = me.GetColumIndex(columnName);
            if (me.IsDBNull(i)) return TimeSpan.MinValue;
            var value = me.GetValue(i);
            if (value is DateTime d) return new TimeSpan(d.Hour, d.Minute, 0);
            throw new InvalidOperationException(columnName);
        }

        public static double GetTimeAsDouble(this IDataRecord me, string columnName)
        {
            var t = me.GetTimeAsTimespan(columnName);
            return t.TotalMinutes;
        }

        public static bool GetBool(this IDataRecord me, string columnName)
        {
            var i = me.GetColumIndex(columnName);
            if (me.IsDBNull(i)) return false;
            var value = me.GetValue(i);
            if (value is bool a) return a;
            if (value is Int16 b) return b != 0;
            if (value is double c) return c != 0;
            throw new InvalidOperationException(columnName);
        }

        public static bool IsDBNull(this IDataRecord me, string columnName)
        {
            var i = me.GetOrdinal(columnName);
            return me.IsDBNull(i);
        }

        private static int GetColumIndex(this IDataRecord me, string columnName, bool throwOnNotFound = true)
        {
            var i = -1;
            try { i = me.GetOrdinal(columnName); }
            catch (IndexOutOfRangeException)
            {
                if (throwOnNotFound) throw new InvalidOperationException(columnName);
            }
            return i;
        }
    }
}
