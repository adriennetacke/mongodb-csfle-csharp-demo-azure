using System;
using MongoDB.Driver;

namespace EnvoyMedSys
{
    public enum KmsKeyLocation
    {
        Azure,
    }

    class Program
    {
        public static void Main(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("MDB_ATLAS_URI");
            var keyVaultNamespace = CollectionNamespace.FromFullName("encryption.__keyVaultTemp");

            var kmsKeyHelper = new KmsKeyHelper(
                connectionString: connectionString,
                keyVaultNamespace: keyVaultNamespace);
            var autoEncryptHelper = new AutoEncryptHelper(
                connectionString: connectionString,
                keyVaultNamespace: keyVaultNamespace);

            var kmsKeyIdBase64 = kmsKeyHelper.CreateKeyWithAzureKmsProvider().GetAwaiter().GetResult();

            autoEncryptHelper.EncryptedWriteAndReadAsync(kmsKeyIdBase64, KmsKeyLocation.Azure).GetAwaiter().GetResult();

            Console.ReadKey();
        }
    }
}
