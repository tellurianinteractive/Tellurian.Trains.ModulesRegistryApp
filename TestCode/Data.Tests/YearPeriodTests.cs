namespace ModulesRegistry.Data.Tests;

/// <summary>
/// Year periods (decided 2026-11-19): both years optional, upto inclusive, valid when from is not after upto. The overlap must
/// give the same result as the SQL views and procedures that select waybills by years.
/// </summary>
[TestClass]
public sealed class YearPeriodTests
{
    [TestMethod]
    [DataRow(null, null, true)]
    [DataRow(1950, null, true)]
    [DataRow(null, 1965, true)]
    [DataRow(1950, 1965, true)]
    [DataRow(1950, 1950, true)]
    [DataRow(1966, 1965, false)]
    public void IsValid(int? from, int? upto, bool expected) => Assert.AreEqual(expected, new YearPeriod(from, upto).IsValid);

    [TestMethod]
    [DataRow(1950, 1965, 1949, false)]
    [DataRow(1950, 1965, 1950, true)]
    [DataRow(1950, 1965, 1965, true)]
    [DataRow(1950, 1965, 1966, false)]
    [DataRow(null, 1965, 1800, true)]
    [DataRow(1950, null, 2100, true)]
    [DataRow(null, null, 1, true)]
    public void ContainsWithUptoInclusive(int? from, int? upto, int year, bool expected) => Assert.AreEqual(expected, new YearPeriod(from, upto).Contains(year));

    [TestMethod]
    [DataRow(1950, 1965, 1965, 1980, true, DisplayName = "Upto inclusive: the boundary year is shared")]
    [DataRow(1950, 1965, 1966, 1980, false)]
    [DataRow(1966, 1980, 1950, 1965, false)]
    [DataRow(1950, 1980, 1960, 1970, true)]
    [DataRow(null, 1965, 1965, null, true)]
    [DataRow(null, 1965, 1966, null, false)]
    [DataRow(1950, null, null, 1949, false)]
    [DataRow(null, null, 1960, 1970, true)]
    [DataRow(1960, 1970, null, null, true)]
    public void Overlaps(int? from, int? upto, int? otherFrom, int? otherUpto, bool expected)
    {
        var period = new YearPeriod(from, upto);
        var other = new YearPeriod(otherFrom, otherUpto);
        Assert.AreEqual(expected, period.Overlaps(other));
        Assert.AreEqual(expected, other.Overlaps(period), "Symmetric.");
        Assert.AreEqual(expected, SqlOverlaps(period, other), "As in the SQL.");
    }

    // The test in GetLayoutWaybills: (C.FromYear IS NULL OR C.FromYear <= ISNULL(L.LastYear, 9999)) AND (C.UptoYear IS NULL OR C.UptoYear >= ISNULL(L.FirstYear, 0)).
    private static bool SqlOverlaps(YearPeriod c, YearPeriod l) =>
        (c.From is null || c.From <= (l.Upto ?? 9999)) && (c.Upto is null || c.Upto >= (l.From ?? 0));

    [TestMethod]
    [DataRow(0, -1, null, null)]
    [DataRow(1965, 1950, 1950, 1965)]
    [DataRow(0, 1965, null, 1965)]
    [DataRow(1950, 1965, 1950, 1965)]
    public void Corrected(int? from, int? upto, int? expectedFrom, int? expectedUpto) =>
        Assert.AreEqual(new YearPeriod(expectedFrom, expectedUpto), new YearPeriod(from, upto).AsCorrected());

    [TestMethod]
    public void Text()
    {
        Assert.AreEqual("1950–1965", new YearPeriod(1950, 1965).ToString());
        Assert.AreEqual("1950–", new YearPeriod(1950, null).ToString());
        Assert.AreEqual("–1965", new YearPeriod(null, 1965).ToString());
        Assert.AreEqual(string.Empty, new YearPeriod((int?)null, null).ToString());
    }

    [TestMethod]
    public void FromShortYears() => Assert.AreEqual(new YearPeriod(1950, 1965), new YearPeriod((short?)1950, (short?)1965));
}