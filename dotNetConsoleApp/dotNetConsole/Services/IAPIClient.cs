using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace dotNetConsole.Services
{
    public interface IAPIClient
    {
        Task GetUsers(AuthenticationResult result, CancellationTokenSource _cancellationTokenSource, Action<JObject> processResult);
        Task GetUsers(AuthenticationResult result, CancellationTokenSource _cancellationTokenSource);
    }
}