using System.Globalization;

namespace MedTechSupplies.Web.Services;

public static class Format
{
    private static readonly CultureInfo ZA = CultureInfo.GetCultureInfo("en-ZA");

    /// <summary>Formats an amount as South African Rand, e.g. R1,299.00</summary>
    public static string Money(decimal amount) => "R" + amount.ToString("N2", ZA);

    public static string Money0(decimal amount) => "R" + amount.ToString("N0", ZA);
}
