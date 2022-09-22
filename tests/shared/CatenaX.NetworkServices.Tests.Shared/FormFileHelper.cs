using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace CatenaX.NetworkServices.Tests.Shared;

public static class FormFileHelper
{
    public static IFormFile GetFormFile(string content, string fileName, string contentType)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(content);
        writer.Flush();
        stream.Position = 0;

        var headers = new HeaderDictionary {{HeaderNames.ContentType, contentType}};
        return new FormFile(stream, 0, stream.Length, "id_from_form", fileName)
        {
            Headers = headers 
        };
    }
}