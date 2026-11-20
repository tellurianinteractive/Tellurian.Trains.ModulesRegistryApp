using Microsoft.Extensions.Validation;
using ModulesRegistry.Data;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.StationCustomers;

/// <summary>
/// The print settings of one of a freight customer's waybills (Migration step 3.6). The waybills themselves are generated from the
/// cargo flows; only these fields are edited. The ids come from the page's address. The display names and the messages are
/// resource keys; the attributes are also checked in the browser.
/// </summary>
[ValidatableType]
public class StationCustomerWaybillFormData
{
    [Display(Name = "Days")]
    [Range(1, int.MaxValue, ErrorMessage = "MustBeSelected_Error")]
    public int OperatingDayId { get; set; }

    [Display(Name = "PrintCount")]
    [Range(0, 100)]
    public int PrintCount { get; set; }

    [Display(Name = "PrintPerOperatingDay")]
    public bool PrintPerOperatingDay { get; set; }

    [Display(Name = "HasEmptyReturn")]
    public bool HasEmptyReturn { get; set; }

    [Display(Name = "HasSameCargoReturn")]
    public bool HasSameCargoReturn { get; set; }
}

public static class StationCustomerWaybillFormExtensions
{
    extension(StationCustomerWaybill entity)
    {
        /// <summary>Entity → form data (GET: populate the form).</summary>
        public StationCustomerWaybillFormData AsFormData() => new()
        {
            OperatingDayId = entity.OperatingDayId,
            PrintCount = entity.PrintCount,
            PrintPerOperatingDay = entity.PrintPerOperatingDay,
            HasEmptyReturn = entity.HasEmptyReturn,
            HasSameCargoReturn = entity.HasSameCargoReturn,
        };
    }

    extension(StationCustomerWaybillFormData form)
    {
        /// <summary>
        /// Form data → the existing waybill: only the print settings change; the customer, cargo flows, region and the other flags
        /// stay as loaded. Call it only with validated form data.
        /// </summary>
        public StationCustomerWaybill AsEntity(StationCustomerWaybill existing)
        {
            existing.OperatingDayId = form.OperatingDayId;
            existing.PrintCount = form.PrintCount;
            existing.PrintPerOperatingDay = form.PrintPerOperatingDay;
            existing.HasEmptyReturn = form.HasEmptyReturn;
            existing.HasSameCargoReturn = form.HasSameCargoReturn;
            return existing;
        }
    }
}