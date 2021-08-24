using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Encryption;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnvoyMedSys
{
    public class AutoEncryptHelper
    {
        private static readonly string __sampleNameValue = "Takeshi Kovacs";
        private static readonly int __sampleSsnValue = 213238414;

        private static readonly BsonDocument __sampleDocFields =
            new BsonDocument
            {
                { "name", __sampleNameValue },
                { "ssn", __sampleSsnValue },
                { "bloodType", "AB-" },
                {
                    "medicalRecords",
                    new BsonArray(new []
                    {
                        new BsonDocument("weight", 180),
                        new BsonDocument("bloodPressure", "120/80")
                    })
                },
                {
                    "insurance",
                    new BsonDocument
                    {
                        { "policyNumber", 211241 },
                        { "provider", "EnvoyHealth" }
                    }
                }
            };

        private readonly string _connectionString;
        private readonly CollectionNamespace _keyVaultNamespace;
        private readonly CollectionNamespace _medicalRecordsNamespace;

        public AutoEncryptHelper(string connectionString, CollectionNamespace keyVaultNamespace)
        {
            _connectionString = connectionString;
            _keyVaultNamespace = keyVaultNamespace;
            _medicalRecordsNamespace = CollectionNamespace.FromFullName("medicalRecords.patientData");
        }

        public async Task EncryptedWriteAndReadAsync(string keyIdBase64, KmsKeyLocation kmsKeyLocation)
        {
            // Construct a JSON Schema
            var schema = JsonSchemaCreator.CreateJsonSchema(keyIdBase64);

            // Construct an auto-encrypting client
            var autoEncryptingClient = CreateAutoEncryptingClient(
                kmsKeyLocation,
                _keyVaultNamespace,
                schema);

            // Set our working database and collection to medicalRecords.patients
            var collection = autoEncryptingClient
                .GetDatabase(_medicalRecordsNamespace.DatabaseNamespace.DatabaseName)
                .GetCollection<BsonDocument>(_medicalRecordsNamespace.CollectionName);

            var ssnQuery = Builders<BsonDocument>.Filter.Eq("ssn", __sampleSsnValue);

            // Upsert (update document if found, otherwise create it) a document into the collection
            var medicalRecordUpdateResult = await collection
                .UpdateOneAsync(ssnQuery, new BsonDocument("$set", __sampleDocFields), new UpdateOptions() { IsUpsert = true });

            if (!medicalRecordUpdateResult.UpsertedId.IsBsonNull)
            {
                Console.WriteLine("Successfully upserted the sample document!");
            }

            // Query by SSN field with auto-encrypting client
            var result = await collection.Find(ssnQuery).SingleAsync();

            Console.WriteLine($"Encrypted client query by the SSN (deterministically-encrypted) field:\n {result}\n");
        } 

        private IMongoClient CreateAutoEncryptingClient(
            KmsKeyLocation kmsKeyLocation,
            CollectionNamespace keyVaultNamespace,
            BsonDocument schema)
        {
            var kmsProviders = new Dictionary<string, IReadOnlyDictionary<string, object>>();

            // Specify Azure Key Vault settings
            if (kmsKeyLocation == KmsKeyLocation.Azure)
            {
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
            }

            var schemaMap = new Dictionary<string, BsonDocument>
                {
                    { _medicalRecordsNamespace.ToString(), schema },
                };

            // Specify location of mongocryptd binary, if necessary
            var extraOptions = new Dictionary<string, object>()
            {
                // uncomment the following line if you are running mongocryptd manually
                // { "mongocryptdBypassSpawn", true }
            };

            // Create CSFLE-enabled MongoClient
            // The addition of the automatic encryption options are what 
            // change this from a standard MongoClient to a CSFLE-enabled one
            var clientSettings = MongoClientSettings.FromConnectionString(_connectionString);
            var autoEncryptionOptions = new AutoEncryptionOptions(
                keyVaultNamespace: keyVaultNamespace,
                kmsProviders: kmsProviders,
                schemaMap: schemaMap,
                extraOptions: extraOptions);

            clientSettings.AutoEncryptionOptions = autoEncryptionOptions;

            return new MongoClient(clientSettings);
        }
    }
}