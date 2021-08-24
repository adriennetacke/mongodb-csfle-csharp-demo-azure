# MongoDB Client-Side Field Level Encryption and Azure Key Vault
This sample application demonstrates how to integrate Azure Key Vault with MongoDB's Client Side Field Level Encryption in a .NET Core application.

## Dependencies

**MongoDB Dependencies**
* A [MongoDB Atlas](https://www.mongodb.com/cloud/atlas>) cluster running MongoDB 4.2 (or later) OR [MongoDB 4.2 Enterprise Server](https://www.mongodb.com/try/download/enterprise>) (or later). Required for automatic encryption.
* [MongoDB .NET Driver 2.13.0](https://www.nuget.org/packages/MongoDB.Driver/2.13.0) (or later)
* [Mongocryptd](https://docs.mongodb.com/manual/reference/security-client-side-encryption-appendix/#installation)

**Azure Dependencies**

* An [Azure Account](https://azure.microsoft.com/en-us/free/) with an active subscription and the same permissions as those found in any of these Azure AD roles (only one is needed):
  * [Application administrator](https://docs.microsoft.com/en-us/azure/active-directory/roles/permissions-reference#application-administrator)
  * [Application developer](https://docs.microsoft.com/en-us/azure/active-directory/roles/permissions-reference#application-developer)
  * [Cloud application administrator](https://docs.microsoft.com/en-us/azure/active-directory/roles/permissions-reference#cloud-application-administrator)
* An [Azure AD tenant](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-create-new-tenant#create-a-new-azure-ad-tenant) (you can use an existing one, assuming you have appropriate permissions)
* [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)


## Running the application

1. Clone this repository:

   ```
     git clone https://github.com/adriennetacke/mongodb-csfle-csharp-demo-azure.git
   ```

    💡 If you decide to share this repo at all, immediately add the `launchSettings.json` file to your `.gitignore` file so that you don't inadvertently expose your variables to the world! *Well, why is there a `launchSettings.json` file in your repo, Adrienne?* I've deliberately left this file in to make development/learning a bit easier for you. :)  

2. Navigate to the cloned repo's directory and open the application in Visual Studio:

    ```
      cd mongodb-csfle-csharp-demo-azure
      EnvoyMedSys.sln
    ```

3. Go to `Properties` > `launchSettings.json` and update all of the placeholder variables: 
    * `MDB_ATLAS_URI`: The connection string to your MongoDB Atlas cluster. This enables the storage of our data encryption key, encrypted by Azure Key Vault. Be sure to update the `<USERNAME>`, `<PASSWORD>`, and `<CLUSTER_NAME>` portions of the URI with your own credentials!
    * `AZURE_TENANT_ID`: Identifies the organization of the Azure account.
    * `AZURE_CLIENT_ID`: Identifies the clientId to authenticate your registered application.
    * `AZURE_CLIENT_SECRET`: Used to authenticate your registered application.
    * `AZURE_KEY_NAME`: Name of the Customer Master Key stored in Azure Key Vault.
    * `AZURE_KEYVAULT_ENDPOINT`: URL of the Key Vault. e.g. yourVaultName.vault.azure.net


## More MongoDB Tutorials

Check out these other tutorials from Adrienne:

- [Create a Multi-Cloud Cluster with MongoDB Atlas](https://developer.mongodb.com/how-to/setup-multi-cloud-cluster-mongodb-atlas/)
- [Using MongoDB Atlas on Heroku](https://developer.mongodb.com/how-to/use-atlas-on-heroku/)
- [How to Use the Union All Aggregation Pipeline Stage in MongoDB 4.4](https://developer.mongodb.com/how-to/use-union-all-aggregation-pipeline-stage/)

