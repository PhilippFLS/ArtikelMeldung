namespace FLS.CodeBeispiel.CrmService.Models.Infrastructure;

/// <summary>
/// Basisklasse für alle Antworten des CRM
/// Alle Antworten kommen grundsätzlich als Array eines Typs zurück
/// </summary>
/// <typeparam name="T">Typ, in den die CRM-Antwort deserialisiert wird</typeparam>
public record CrmBaseResponse<T>
{
    public T[] Value { get; set; } = Array.Empty<T>();
}
