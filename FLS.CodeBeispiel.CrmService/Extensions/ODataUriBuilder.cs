using FLS.CodeBeispiel.CrmService.Models.Infrastructure;
using System.Text;
using System.Web;

namespace FLS.CodeBeispiel.CrmService.Extensions;

/// <summary>
/// Hilfsfunktionen zum Erstellen von ODataabfragen
/// Dabei werden die häufigsten Operationen abgebildet
/// </summary>
internal static class ODataUriBuilder
{
    /// <summary>
    /// Erstellt eine Basisurl für Abfragen an das CRM
    /// </summary>
    /// <param name="baseAdress"></param>
    /// <param name="entity" <see cref="CrmTableNames"/>>Name der abzufragenden Tabelle</param>
    /// <returns>UriBuilder mit initialer URI</returns>
    internal static UriBuilder SetBaseEntity(this UriBuilder uriBuilder, string entity)
    {
        uriBuilder.Path += $"api/data/v9.1/{entity}";
        return uriBuilder;
    }

    /// <summary>
    /// Fügt einen Stringfilter zu den Filtern der Uri für OData hinzu
    /// </summary>
    /// <param name="uriBuilder">UriBuilder auf dem der Filter ergänzt werden soll</param>
    /// <param name="filter" <see cref="ODataFilter{T}"/>>Wendet einen Stringfilter auf die Request an</param>
    /// <returns>Um einen Stringfilter aktualisierte Version des hereingegebenen UriBuilders</returns>
    internal static UriBuilder WithFilter(
        this UriBuilder uriBuilder,
        ODataFilter<string> filter)
    {
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        var link = filter.UndVerknüpfung ? "and" : "or";
        query["$filter"] += string.IsNullOrEmpty(query["$filter"]) ? $"{filter.Key} {filter.Operator} '{filter.Value}'"
                                                                   : $" {link} {filter.Key} {filter.Operator} '{filter.Value}'";
        uriBuilder.Query = query.ToString();
        return uriBuilder;
    }

    /// <summary>
    /// Fügt einen Guidfilter zu den Filtern der Uri für OData hinzu
    /// </summary>
    /// <param name="uriBuilder">UriBuilder auf dem der Filter ergänzt werden soll</param>
    /// <param name="filter" <see cref="ODataFilter{T}"/>>Wendet einen Guidfilter auf die Request an</param>
    /// <returns>Um einen Stringfilter aktualisierte Version des hereingegebenen UriBuilders</returns>
    internal static UriBuilder WithFilter(
        this UriBuilder uriBuilder,
        ODataFilter<Guid> filter)
    {
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        var link = filter.UndVerknüpfung ? "and" : "or";
        query["$filter"] += string.IsNullOrEmpty(query["$filter"]) ? $"{filter.Key} {filter.Operator} '{filter.Value}'"
                                                                   : $" {link} {filter.Key} {filter.Operator} '{filter.Value}'";
        uriBuilder.Query = query.ToString();
        return uriBuilder;
    }

    /// <summary>
    /// Fügt einen Zahlenfilter zu den Filtern der Uri für OData hinzu
    /// Bietet zusätzlich die Möglichkeit, Felder auf null zu prüfen
    /// </summary>
    /// <param name="uriBuilder">UriBuilder auf dem der Filter ergänzt werden soll</param>
    /// <param name="filter" <see cref="ODataFilter{T}"/>>Wendet einen Zahlenfilter auf die Request an</param>
    /// <returns>Um einen Zahlenfilter aktualisierte Version des hereingegebenen UriBuilders</returns>
    internal static UriBuilder WithFilter(
        this UriBuilder uriBuilder,
        ODataFilter<int?> filter)
    {
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        var link = filter.UndVerknüpfung ? "and" : "or";
        query["$filter"] += string.IsNullOrEmpty(query["$filter"]) ? $"{filter.Key} {filter.Operator} {filter.Value?.ToString() ?? "null"}"
                                                                   : $" {link} {filter.Key} {filter.Operator} {filter.Value?.ToString() ?? "null"}";
        uriBuilder.Query = query.ToString();
        return uriBuilder;
    }

    /// <summary>
    /// Fügt eine Liste von Filtern zu den Filtern der Uri für OData hinzu
    /// Die Syntax übernimmt dabei die des Stringfilters
    /// </summary>
    /// <param name="uriBuilder">UriBuilder auf dem der Filter ergänzt werden soll</param>
    /// <param name="filter" <see cref="ODataFilter{T}"/>>Wendet einen Listenfilter auf die Request an</param>
    /// <returns>Um einen Listenfilter aktualisierte Version des hereingegebenen UriBuilders</returns>
    internal static UriBuilder WithFilter(
        this UriBuilder uriBuilder,
        ODataFilter<IEnumerable<string>> filter)
    {
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        var link = filter.UndVerknüpfung ? "and" : "or";

        var sb = new StringBuilder();
        foreach (var value in filter.Value)
        {
            sb.Append(value != filter.Value.Last() ? $"{filter.Key} {filter.Operator} '{value}' {link} " : $"{filter.Key} {filter.Operator} '{value}'");
        }

        query["$filter"] += string.IsNullOrEmpty(query["$filter"]) ? $"{sb}"
                                                                   : $" {link} {sb}";
        uriBuilder.Query = query.ToString();
        return uriBuilder;
    }

    /// <summary>
    /// Fügt einen selbstgeschriebenen Filter zur ODataabfrage hinzu
    /// </summary>
    /// <param name="uriBuilder">UriBuilder auf dem der Filter ergänzt werden soll</param>
    /// <param name="filter">Selbstgeschriebener Filter, muss als Key-Feld übergeben werden</param>
    /// <returns>Um einen eigenen Filter aktualisierte Version des hereingegebenen UriBuilders</returns>
    internal static UriBuilder WithCustomFilter(
    this UriBuilder uriBuilder,
    ODataFilter<string> filter)
    {
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        var link = filter.UndVerknüpfung ? "and" : "or";
        query["$filter"] += string.IsNullOrEmpty(query["$filter"]) ? $"{filter.Key}"
                                                                   : $" {link} {filter.Key}";
        uriBuilder.Query = query.ToString();
        return uriBuilder;
    }

    /// <summary>
    /// Fügt einen Datumsfilter zu den Filtern der Uri für OData hinzu
    /// Das Datum wird hierbei intern in das richtige Format für das CRM gebracht
    /// </summary>
    /// <param name="uriBuilder">UriBuilder auf dem der Filter ergänzt werden soll</param>
    /// <param name="filter" <see cref="ODataFilter{T}"/>>Wendet einen Datumsfilter auf die Request an</param>
    /// <returns>Um einen Datumsfilter aktualisierte Version des hereingegebenen UriBuilders</returns>
    internal static UriBuilder WithFilter(
    this UriBuilder uriBuilder,
    ODataFilter<DateTime> filter)
    {
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        var link = filter.UndVerknüpfung ? "and" : "or";
        query["$filter"] += string.IsNullOrEmpty(query["$filter"]) ? $"{filter.Key} {filter.Operator} {filter.Value:yyyy-MM-dd}"
                                                                   : $" {link} {filter.Key} {filter.Operator} {filter.Value:yyyy-MM-dd}";
        uriBuilder.Query = query.ToString();
        return uriBuilder;
    }

    /// <summary>
    /// Fügt ein Feld als Ausgabe der ODataabfrage hinzu
    /// </summary>
    /// <param name="uriBuilder">UriBuilder auf dem das Select ergänzt werden soll</param>
    /// <param name="selectKey">Feld, welches abgefragt werden soll</param>
    /// <returns>Um ein Select aktualisierte Version des hereingegebenen UriBuilders</returns>
    internal static UriBuilder WithSelect(this UriBuilder uriBuilder, string selectKey)
    {
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["$select"] += string.IsNullOrEmpty(query["$select"]) ? $"{selectKey}"
                                                                   : $",{selectKey}";
        uriBuilder.Query = query.ToString();
        return uriBuilder;
    }

    /// <summary>
    /// Erweitert die abgefragte Entität mittels Referenz um Daten aus einer anderen Entität
    /// Achtung: Geht pro Abfrage nur einmal!
    /// </summary>
    /// <param name="uriBuilder">UriBuilder auf dem das Expand ergänzt werden soll</param>
    /// <param name="expandKey">Entität, um welche die angefragte Entität erweitert werden soll</param>
    /// <param name="expandedSelectKeys">Felder, die auf der erweiterten Entität ausgwählt werden sollen</param>
    /// <returns>Um ein Expand aktualisierte Version des hereingegebenen UriBuilders</returns>
    internal static UriBuilder WithExpand(this UriBuilder uriBuilder, string expandKey, string[] expandedSelectKeys)
    {
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        var joinedSelectKeys = string.Join(",", expandedSelectKeys);
        query["$expand"] = $"{expandKey}($select={joinedSelectKeys})";

        uriBuilder.Query = query.ToString();
        return uriBuilder;
    }

    /// <summary>
    /// Stellt die aktuell zusammengebaute Uri aus dem UriBuilder bereit
    /// </summary>
    /// <param name="uriBuilder">UriBuilder, aus welchem die Uri ausgegeben werden soll</param>
    /// <returns>Die fertige Uri aus dem UriBuilder</returns>
    internal static Uri BuildUri(this UriBuilder uriBuilder)
    {
        return uriBuilder.Uri;
    }
}
