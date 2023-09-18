using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FLS.CodeBeispiel.FunctionApp.Abstractions;
using System.Text.Json;

namespace FLS.CodeBeispiel.FunctionApp.Logic;

/// <summary>
/// Implementierung von <see cref="IMailSender"/>
/// </summary>
public class MailSender : IMailSender
{
    private readonly BlobContainerClient blobContainerClient;

    public MailSender(BlobServiceClient blobServiceClient)
    {
        blobContainerClient = blobServiceClient.GetBlobContainerClient(Constants.BlobContainerName);
        blobContainerClient.CreateIfNotExistsAsync();
    }

    /// <inheritdoc/>
    public async virtual Task LadeMailInDenBlobStorageHoch(IMailTemplate mail, string dateiName)
    {
        var blobClient = blobContainerClient.GetBlobClient(dateiName);

        var mailAlsJson = JsonSerializer.Serialize(mail);

        await blobClient.UploadAsync(new BinaryData(mailAlsJson), new BlobUploadOptions() { HttpHeaders = new BlobHttpHeaders() { ContentType = Constants.BlobContentType } });
    }
}
