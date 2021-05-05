using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoLib
{
    public class DemoMessages : IDemoMessages
    {
        public string SayHello() => "Hello Viewer";
        public string SayGoodBye() => "Goodbye, Farwell, and Good Day!";
    }
}
