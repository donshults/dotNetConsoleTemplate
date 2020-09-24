using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace dotNetConsole.Services
{
    public class Customer : ICustomer
    {
        private readonly ILogger _logger;

        public Customer(ILogger<Customer> logger)
        {
            _logger = logger;
        }
        public string CustomerName { get; set; }

        public void CreateCustomer(string name)
        {
            _logger.LogInformation($"Logging Enabled: Customer Name: {name}");
            CustomerName = name;
        }
    }
}
