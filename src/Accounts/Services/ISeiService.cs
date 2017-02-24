namespace Accounts.Services
{
    using Accounts.Models;
    using Microsoft.AspNetCore.Http;

    public interface ISeiService
    {
        void AddDocument(string protocol, string title, IFormFile document);

        void AddTextDocument(string protocol, string title, string content);

        void CreateProtocol(Person person);

        void ReopenProtocol(string protocol);
    }
}