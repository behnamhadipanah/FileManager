namespace FileManager.Domain.Enumerations;

public enum StorageProvider
{
    RustFs = 1,
    MinIo = 2,
    AwsS3 = 3,
    AzureBlob = 4,
    Local = 5
}