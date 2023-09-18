using FLS.CodeBeispiel.CrmService.Enumerations;

namespace FLS.CodeBeispiel.CrmService.Models.Infrastructure;

/// <summary>
/// Erstellt einen Filter für die OData-Abfrage
/// </summary>
/// <typeparam name="T">Typ des Filterwertes</typeparam>
internal record ODataFilter<T>
{
    /// <summary>Feld, auf dass gefiltert werden soll</summary>
    internal string Key { get; init; } = string.Empty;

    /// <summary>Wert mit dem das Feld gefiltert werden soll</summary>
    internal T Value { get; init; } = default!;

    /// <summary>Verknüpfung zu bereits bestehenden Filtern, sofern bereits welche in der URI vorhanden sind</summary>
    internal bool UndVerknüpfung { get; init; } = true;

    /// <summary <see cref="ODataOperators"/>>Operator zum Vergleich des Feldes mit dem übergebenen Filterwert</summary>
    internal ODataOperators Operator { get; init; } = ODataOperators.eq;
}
