# Azure Data Lake Gen2 -- REST API Examples in C#
This project provides an example of leveraging the [REST APIs](https://docs.microsoft.com/en-us/rest/api/storageservices/datalakestoragegen2/filesystem) for Azure Data Lake Gen2 to create file systems, folder paths and creating and uploading files. 

## Getting started
If you don't have one already, you will need to create an Azure Data Lake Gen2 account by following the instuctions here [https://docs.microsoft.com/en-us/azure/storage/blobs/data-lake-storage-quickstart-create-account](https://docs.microsoft.com/en-us/azure/storage/blobs/data-lake-storage-quickstart-create-account)


Next, you will need to create an Azure Active Directory (AAD) application that will provide the authentication mechanism for the REST calls. The instructions for this are here: 
[https://docs.microsoft.com/en-us/previous-versions/azure/storage/data-lake-storage-rest-api-guide#register-with-azure-active-directory-tenant](https://docs.microsoft.com/en-us/previous-versions/azure/storage/data-lake-storage-rest-api-guide#register-with-azure-active-directory-tenant)

Once you have the AAD application, you need to gather the Application (client) ID, Directory (tenant) ID and Client secret for that application and add them to the web.config appSettings applicationId, tenantID and secretKey respectively.

You should then be able to compile and run the application to test our the APIs!

**NOTE:** Code provided for example purposes only.  