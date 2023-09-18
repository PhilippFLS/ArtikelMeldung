namespace FLS.CodeBeispiel.FunctionApp.Abstractions;

public interface IMailSender
{
    /// <summary>
    /// Hiermit wird eine erzeugte Mail verschickt
    /// Achtung: Im momentanen Prozess gibt es keine Möglichkeit unsererseits, Mails direkt aus einer FunctionApp zu versenden!
    /// Der Workaround ist hier, eine Datei in einem BlobStorage abzulegen, woraufhin ein anderer Prozess diese weiterverschickt.
    /// </summary>
    /// <param name="mail">Die zu verschickende Mail</param>
    /// <param name="dateiName">Der Name der abgelegten Datei</param>
    Task LadeMailInDenBlobStorageHoch(IMailTemplate mail, string dateiName);
}
