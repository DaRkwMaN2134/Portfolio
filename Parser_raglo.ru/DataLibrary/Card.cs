namespace DataLibrary
{
    public class Card
    {
        public string categoryname {  get; set; }
        public string article {  get; set; }
        public string pictureurl { get; set; }
        public string price { get; set; }
        public string description { get; set; }
        public Card(string Categoryname, string Article, string Pictureurl, string Price, string Description)
        {
            categoryname = Categoryname;
            article = Article;
            pictureurl = Pictureurl;
            price = Price;
            description = Description;
        }
    }
}
