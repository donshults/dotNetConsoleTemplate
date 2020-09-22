using System;
using System.Collections.Generic;
using System.Text;

namespace dotNetConsole1.Services
{
    public class Customer : ICustomer
    {
        public string CustomerName { get; set; }

        public void CreateCustomer(string name)
        {
            CustomerName = name;
        }
    }
}
