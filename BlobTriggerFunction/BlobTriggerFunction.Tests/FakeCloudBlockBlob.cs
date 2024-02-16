using Microsoft.Azure.Storage.Blob;

namespace BlobTriggerFunction.Tests;
public class FakeCloudBlockBlob : CloudBlockBlob
{
    public FakeCloudBlockBlob(Uri uri) : base(uri)
    {
        Metadata = new Dictionary<string, string>();
    }

    public new IDictionary<string, string> Metadata { get; }
}
