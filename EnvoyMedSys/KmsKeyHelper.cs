using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Encryption;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EnvoyMedSys
{
    public class KmsKeyHelper
    {
        private readonly string _mdbConnectionString;
        private readonly CollectionNamespace _keyVaultNamespace;

        public KmsKeyHelper(
            string connectionString,
            CollectionNamespace keyVaultNamespace)
        {
            _mdbConnectionString = connectionString;
            _keyVaultNamespace = keyVaultNamespace;
        }

        public async Task<string> CreateKeyWithAzureKmsProvider()
        {
            var kmsProviders = new Dictionary<string, IReadOnlyDictionary<string, object>>();

            var azureTenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            var azureClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var azureClientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            var azureIdentityPlatformEndpoint = Environment.GetEnvironmentVariable("AZURE_IDENTIFY_PLATFORM_ENPDOINT"); // Optional, only needed if user is using a non-commercial Azure instance

            var azureKmsOptions = new Dictionary<string, object>
            {
                { "tenantId", azureTenantId },
                { "clientId", azureClientId },
                { "clientSecret", azureClientSecret },
            };

            if (azureIdentityPlatformEndpoint != null)
            {
                azureKmsOptions.Add("identityPlatformEndpoint", azureIdentityPlatformEndpoint);
            }

            kmsProviders.Add("azure", azureKmsOptions);

            var clientEncryption = GetClientEncryption(kmsProviders);
            var azureKeyName = Environment.GetEnvironmentVariable("AZURE_KEY_NAME");
            var azureKeyVaultEndpoint = Environment.GetEnvironmentVariable("AZURE_KEYVAULT_ENDPOINT"); // typically <azureKeyName>.vault.azure.net
            var azureKeyVersion = Environment.GetEnvironmentVariable("AZURE_KEY_VERSION"); // Optional
            var dataKeyOptions = new DataKeyOptions(
                masterKey: new BsonDocument
                {
                    { "keyName", azureKeyName },
                    { "keyVaultEndpoint", azureKeyVaultEndpoint },
                    { "keyVersion", () => azureKeyVersion, azureKeyVersion != null }
                });

            var dataKeyId = clientEncryption.CreateDataKey("azure", dataKeyOptions, CancellationToken.None);
            Console.WriteLine($"Azure DataKeyId [UUID]: {dataKeyId}");

            var dataKeyIdBase64 = Convert.ToBase64String(GuidConverter.ToBytes(dataKeyId, GuidRepresentation.Standard));
            Console.WriteLine($"Azure DataKeyId [base64]: {dataKeyIdBase64}");

            await ValidateKeyAsync(dataKeyId);

            return dataKeyIdBase64;
        }

        private ClientEncryption GetClientEncryption(
            Dictionary<string, IReadOnlyDictionary<string, object>> kmsProviders)
        {
            var keyVaultClient = new MongoClient(_mdbConnectionString);
            var clientEncryptionOptions = new ClientEncryptionOptions(
                keyVaultClient: keyVaultClient,
                keyVaultNamespace: _keyVaultNamespace,
                kmsProviders: kmsProviders);

            return new ClientEncryption(clientEncryptionOptions);
        }

        private async Task ValidateKeyAsync(Guid dataKeyId)
        {
            var client = new MongoClient(_mdbConnectionString);
            var collection = client
                .GetDatabase(_keyVaultNamespace.DatabaseNamespace.DatabaseName)
#pragma warning disable CS0618 // Type or member is obsolete
                        .GetCollection<BsonDocument>(_keyVaultNamespace.CollectionName, new MongoCollectionSettings { GuidRepresentation = GuidRepresentation.Standard });
#pragma warning restore CS0618 // Type or member is obsolete

            var query = Builders<BsonDocument>.Filter.Eq("_id", new BsonBinaryData(dataKeyId, GuidRepresentation.Standard));
            var keyDocument = await collection
                .Find(query)
                .SingleAsync();

            Console.WriteLine(keyDocument);
        }
    }
}
