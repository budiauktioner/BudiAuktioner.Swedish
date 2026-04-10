namespace Buildi.Primitives.Finance;

/// <summary>
/// Provides exchange rates between <see cref="Currency"/> pairs.
/// </summary>
public interface IExchangeRates
{
    /// <summary>
    /// Returns the exchange rate from <paramref name="from"/> to <paramref name="to"/>.
    /// Returns <c>1</c> when both currencies are the same.
    /// </summary>
    /// <exception cref="InvalidOperationException">No rate is available for the requested pair.</exception>
    decimal GetRate(Currency from, Currency to);

    /// <summary>
    /// Attempts to get the exchange rate from <paramref name="from"/> to <paramref name="to"/>.
    /// Returns <see langword="true"/> and sets <paramref name="rate"/> to <c>1</c> when both currencies are the same.
    /// </summary>
    bool TryGetRate(Currency from, Currency to, out decimal rate);
}

/// <summary>
/// A dictionary-backed collection of exchange rates. Automatically derives inverse rates
/// unless an explicit rate is provided in both directions.
/// </summary>
/// <example>
/// <code>
/// var rates = new ExchangeRates()
///     .AddRate(Currency.Parse("SEK"), Currency.Parse("USD"), 0.095m)
///     .AddRate(Currency.Parse("SEK"), Currency.Parse("EUR"), 0.087m);
///
/// var rate = rates.GetRate(Currency.Parse("SEK"), Currency.Parse("USD")); // 0.095
/// var inverse = rates.GetRate(Currency.Parse("USD"), Currency.Parse("SEK")); // ~10.5263…
/// </code>
/// </example>
public sealed class ExchangeRates : IExchangeRates
{
    private readonly Dictionary<(string From, string To), decimal> _rates = new();

    /// <summary>
    /// Adds an exchange rate from <paramref name="from"/> to <paramref name="to"/>.
    /// A rate of <c>0.095</c> from SEK to USD means 1 SEK = 0.095 USD.
    /// The inverse rate is automatically derived unless it has been explicitly set.
    /// </summary>
    /// <returns>The same <see cref="ExchangeRates"/> instance for fluent chaining.</returns>
    public ExchangeRates AddRate(Currency from, Currency to, decimal rate)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);
        if (rate <= 0)
            throw new ArgumentOutOfRangeException(nameof(rate), "Exchange rate must be positive.");

        _rates[(from.Code, to.Code)] = rate;

        if (!_rates.ContainsKey((to.Code, from.Code)))
            _rates[(to.Code, from.Code)] = 1m / rate;

        return this;
    }

    /// <inheritdoc />
    public decimal GetRate(Currency from, Currency to)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        if (from.Code == to.Code)
            return 1m;

        if (_rates.TryGetValue((from.Code, to.Code), out var rate))
            return rate;

        throw new InvalidOperationException($"No exchange rate available from {from.Code} to {to.Code}.");
    }

    /// <inheritdoc />
    public bool TryGetRate(Currency from, Currency to, out decimal rate)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        if (from.Code == to.Code)
        {
            rate = 1m;
            return true;
        }

        return _rates.TryGetValue((from.Code, to.Code), out rate);
    }
}
