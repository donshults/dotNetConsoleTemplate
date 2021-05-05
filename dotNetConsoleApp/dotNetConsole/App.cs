using DemoLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNetConsole
{
    public class App
    {
        private readonly IDemoMessages _messages;

        public App(IDemoMessages messages)
        {
            _messages = messages;
        }

        public void Run()
        {
            Console.WriteLine(_messages.SayHello());
            Console.WriteLine(_messages.SayGoodBye());
            Console.ReadLine();
        }
    }
}
