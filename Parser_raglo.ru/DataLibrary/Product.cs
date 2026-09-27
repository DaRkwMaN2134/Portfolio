using System;
using System.Collections.Generic;
using System.Text;

namespace DataLibrary
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public string Article { get; set; }
        public string Pictureurl { get; set; }
        public List<string> Tags { get; set; } = new();
    }
}
