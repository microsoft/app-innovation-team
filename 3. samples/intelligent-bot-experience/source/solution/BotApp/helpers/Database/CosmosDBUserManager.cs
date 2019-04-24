using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Linq;

namespace BotApp
{
    public class CosmosDBUserManager
    {
        public async static void DeleteUserSession(string id)
        {
            DocumentClient client = new DocumentClient(new Uri(Settings.BotConversationStorageConnectionString), Settings.BotConversationStorageKey);
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            IQueryable<Document> docQueryInSql = client.CreateDocumentQuery<Document>(
                    UriFactory.CreateDocumentCollectionUri(Settings.BotConversationStorageDatabaseId, Settings.BotConversationStorageUserCollection),
                    $"SELECT * FROM c WHERE ENDSWITH(c.id, '{id}')",
                    queryOptions);

            foreach (Document doc in docQueryInSql)
            {
                await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(Settings.BotConversationStorageDatabaseId, Settings.BotConversationStorageUserCollection, doc.Id));
            }
        }
    }
}