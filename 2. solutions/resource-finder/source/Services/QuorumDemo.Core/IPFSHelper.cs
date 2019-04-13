using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
namespace QuorumDemo.Core
{
    public class IPFSHelper
    {
        public IPFSHelper()
        {
        }

        public async Task<string> GetIPFSFileHashAsync()
        {
            //TODO: 
            // This code should allow a file to be uploaded to a given IPFS node and 
            // Return back its content Hash. 

            return "";
        }

        public async Task<string> ResolveIPFSHashAsync(string fileHash)
        {
            //TODO: 
            // This code should resolve a given content hash at a node and
            // Return back its content

            return "";
        }

    }
}
