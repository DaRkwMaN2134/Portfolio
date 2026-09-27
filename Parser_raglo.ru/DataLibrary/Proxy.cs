using System;
using System.Collections.Generic;
using System.Text;

namespace DataLibrary
{
    public class Proxy
    {
        public string address { get; set; }
        public string username { get; set; }
        public string password { get; set; }

        public Proxy(string Address, string Username, string Password)
        {
            address = Address;
            username = Username;
            password = Password;
        }
    }
}
