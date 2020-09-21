using dotNetConsole.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Threading.Tasks;

namespace dotNetConsole.Services
{
    public interface IGraphConnectDemo
    {
        Task Run(AuthenticationResult result);
    }
}
