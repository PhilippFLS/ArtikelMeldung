namespace FLS.CodeBeispiel.FunctionApp.Abstractions;

/// <summary>
/// Basisinterface für zu erstellende Mails
/// Dieses wird von dem weiterverarbeitenden Prozess für Mails verlangt
/// </summary>
public interface IMailTemplate
{
    /// <summary>
    /// Liste der Empfänger
    /// </summary>
    public IEnumerable<string> Recipients { get; set; }

    /// <summary>
    /// Liste der CC-Empfänger
    /// </summary>
    public IEnumerable<string> CCRecipients { get; set; }

    /// <summary>
    /// Gewünschter Mailbetreff
    /// </summary>
    public string Subject { get; set; }

    /// <summary>
    /// Body der Mail
    /// HTML-Tags können hier beinhaltet sein
    /// </summary>
    public string MailBody { get; set; }
}
