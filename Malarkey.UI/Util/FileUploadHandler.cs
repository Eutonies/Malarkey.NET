using Malarkey.Abstractions.Util;
using Microsoft.AspNetCore.Components.Forms;

namespace Malarkey.UI.Util;

public static class FileUploadHandler
{

    private const long MaxNumberOfBytes = 3 * 1024 * 1024;

    private static readonly IReadOnlyDictionary<string, string> MimeTypeMap = new List<(string, string)>
    {
        ("png", "image/png"),
        ("jpg", "image/jpg"),
        ("webp", "image/webp")


    }.ToDictionarySafe(_ => _.Item1, _ => _.Item2);

    public static async Task<FileUploadResult> ParseFileChange(InputFileChangeEventArgs ev)
    {
        var name = ev.File.Name;
        var lastDot = name.LastIndexOf('.');
        if(lastDot > 0)
        {
            var fileEnding = name.Substring(lastDot + 1).ToLower();
            if(MimeTypeMap.ContainsKey(fileEnding))
            {
                var mimeType = MimeTypeMap[fileEnding];
                var fileStream = ev.File.OpenReadStream(MaxNumberOfBytes);
                var bytes = await fileStream.ToByteArray();
                return new FileUploadSuccess(FileName: name, FileEnding: fileEnding, mimeType, bytes);
            }
            return new FileUploadFailure($"Cannot handle files of file type: {fileEnding}");
        }
        return new FileUploadFailure($"LOLs.... Your file no contain a dot... Take the L!");
    }

    public abstract record FileUploadResult();
    public record FileUploadSuccess(string FileName, string FileEnding, string MimeType, byte[] Bytes) : FileUploadResult;
    public record FileUploadFailure(string FileError) : FileUploadResult;

}
