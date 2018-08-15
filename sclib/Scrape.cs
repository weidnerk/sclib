using HtmlAgilityPack;
using sclib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace sclib
{
    public class Scrape
    {
        private const string mainUrl = "https://www.samsclub.com";

        // have to get into detail to get item id
        //
        // 07.04.2018 came across this https://www.samsclub.com/sams/huggies-similac-aveeno-baby-ecos-5pc-bundle/prod22381122.ip?xid=plp_product_1_31
        // where you create a bundle of, say, 5 items.  
        // then itemNo is found differently.  Looks like found as: 
        //      <span itemprop=productID>980128004</span>
        //
        public static async Task<ItemDetail> GetDetail(string url)
        {
            var detail = new ItemDetail();
            string itemId = null;
            string descr = null;
            int i = 0;
            try
            {
                //if (url == "https://www.samsclub.com/sams/huggies-similac-aveeno-baby-ecos-5pc-bundle/prod22381122.ip?xid=plp_product_1_40")
                //{
                //    int zzi = 999;
                //}
                using (HttpClient client = new HttpClient())
                using (HttpResponseMessage response = await client.GetAsync(url))
                using (HttpContent content = response.Content)
                {
                    string result = await content.ReadAsStringAsync();

                    var htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(result);

                    // not sure how i found this but this input is of type hidden
                    var z = htmlDocument.DocumentNode.SelectNodes("//input[@id='itemNo']").FirstOrDefault();
                    var z1 = z.Attributes["value"].Value;
                    itemId = z1;
                    if (itemId == "980043352")
                    {
                        int zb = 100;
                    }

                    var descrNode = htmlDocument.DocumentNode.SelectNodes("//div[contains(@class,'itemBullets')]//li");
                    var node = htmlDocument.DocumentNode.SelectNodes("//div[contains(@class,'itemBullets')]");
                    descr = node.ElementAt(0).InnerHtml;

                    detail.itemId = itemId;
                    detail.descrHtml = descr;

                    var priceCollection = htmlDocument.DocumentNode.SelectNodes("//span[@itemprop='price']");
                    if (priceCollection != null)
                    {
                        string p = priceCollection.ElementAt(0).InnerText;
                        detail.price = parsePrice(p);
                    }
                    var availCollection = htmlDocument.DocumentNode.SelectNodes("//div[@class='biggraybtn']");
                    if (availCollection != null)
                    {
                        string p = availCollection.ElementAt(0).InnerText;
                        if (!string.IsNullOrEmpty(p))
                            detail.availability = p;
                    }

                    var img = htmlDocument.DocumentNode.SelectNodes("//div[contains(@class,'imgCol')]");
                    if (img != null)
                    {
                        var imhDoc = new HtmlDocument();
                        imhDoc.LoadHtml(img[0].OuterHtml);
                        var imgUrls = imhDoc.DocumentNode.SelectNodes("//img/@src").First();
                        var im = "https:" + imgUrls.Attributes["src"].Value;
                        detail.imageUrl = im;
                    }

                    //var picCollection = htmlDocument.DocumentNode.SelectNodes("//div[contains(@class,'sc-image-viewer-sellpoint-hero')]");
                    var pic = htmlDocument.DocumentNode.SelectNodes("//div[@id='plImageHolder']");
                    if (pic != null)
                    {
                        var imhDoc = new HtmlDocument();
                        imhDoc.LoadHtml(pic[0].OuterHtml);
                        var imgUrls = imhDoc.DocumentNode.SelectNodes("//img/@src").First();
                        var im = "https:" + imgUrls.Attributes["src"].Value;
                        bool done = false;
                        detail.picUrl = new List<string>();
                        do
                        {
                            //if (i == 38)
                            //{
                            //    int zzz = 100;
                            //}
                            string uri = createPicUrl(im, i++);
                            FetchPic(uri);

                            if (FetchPic(uri))
                                detail.picUrl.Add(uri);
                            else done = true;
                            if (i == 10) done = true;   // backstop
                        } while (!done);

                        detail.imageUrl = im;
                    }

                }
                return detail;
            }
            catch (Exception exc)
            {
                return null;
            }
        }

        // How to collect all item images?
        // Use createPicUrl to get next image, but when does it end? 
        // If ask for non existant, sam's returns place holder image - that image has size 11,451 bytes
        // How else to do it?
        static bool FetchPic(string uri)
        {
            WebClient wc = new WebClient();
            byte[] bytes = wc.DownloadData(uri);
            if (bytes.Length == 11451) return false;
            return true;
        }
    

        static string createPicUrl(string incomingUrl, int index)
        {
            string uri = null;
            char letter;
            switch (index)
            {
                case 0:
                    letter = 'A';
                    break;
                case 1:
                    letter = 'B';
                    break;
                case 2:
                    letter = 'C';
                    break;
                case 3:
                    letter = 'D';
                    break;
                case 4:
                    letter = 'E';
                    break;
                case 5:
                    letter = 'F';
                    break;
                case 6:
                    letter = 'G';
                    break;
                case 7:
                    letter = 'H';
                    break;
                case 8:
                    letter = 'I';
                    break;
                case 9:
                    letter = 'J';
                    break;
                case 10:
                    letter = 'K';
                    break;
                default:
                    letter = 'Z';
                    break;
            }
            int pos = incomingUrl.IndexOf('?');
            uri = incomingUrl.Substring(0, pos - 1);
            uri += letter + "?wid=685&hei=685";
            return uri;
        }

        static bool isValidUrl(string uriName)
        {
            Uri uriResult;
            bool result = Uri.TryCreate(uriName, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            return result;
        }

        static decimal parsePrice(string priceStr)
        {
            if (string.IsNullOrEmpty(priceStr))
                return 0;
            else
            {
                return Convert.ToDecimal(priceStr);
            }
        }

        public static List<StarRating> getStars(HtmlDocument doc)
        {
            var starCollection = doc.DocumentNode.SelectNodes("//div[@class='sc-product-card-stars']");
            List<StarRating> starsCollection = new List<StarRating>();
            foreach (HtmlNode item in starCollection)
            {
                var htmlTitle = new HtmlDocument();
                htmlTitle.LoadHtml(item.InnerHtml);

                var avg = htmlTitle.DocumentNode.SelectNodes("//span[@class='seo-avg-rating']").FirstOrDefault();
                var best = htmlTitle.DocumentNode.SelectNodes("//span[@class='seo-best-rating']").FirstOrDefault();
                var reviews = htmlTitle.DocumentNode.SelectNodes("//span[@class='seo-review-count']").FirstOrDefault();

                var rating = new StarRating();
                rating.seo_avg_rating = avg.InnerText;
                rating.seo_best_rating = best.InnerText;
                rating.seo_review_count = reviews.InnerText;
                starsCollection.Add(rating);
            }
            return starsCollection;
        }

        public static List<string> getImageUrls(HtmlDocument htmlDocument)
        {
            var q = htmlDocument.DocumentNode.SelectNodes("//div[@id='plImageHolder']");

            List<string> limitList = new List<string>();
            foreach (HtmlNode item in q)
            {
                var htmlTitle = new HtmlDocument();
                htmlTitle.LoadHtml(item.InnerHtml);

                // note that 'contains' works despite there being a space at the end of the class 
                var limits = htmlTitle.DocumentNode.SelectNodes("//img[@itemprop='image']");
                if (limits != null)
                    limitList.Add(limits.ElementAt(0).InnerText);
                else limitList.Add(null);
            }
            return limitList;
        }

        public static List<string> getLimits(HtmlDocument htmlDocument)
        {
            var q = htmlDocument.DocumentNode.SelectNodes("//div[@class='sc-money-box-wrapper']");

            List<string> limitList = new List<string>();
            foreach (HtmlNode item in q)
            {
                var htmlTitle = new HtmlDocument();
                htmlTitle.LoadHtml(item.InnerHtml);

                // note that 'contains' works despite there being a space at the end of the class 
                var limits = htmlTitle.DocumentNode.SelectNodes("//div[contains(@class,'sc-channel-limits')]");
                if (limits != null)
                    limitList.Add(limits.ElementAt(0).InnerText);
                else limitList.Add(null);
            }
            return limitList;
        }

        public static List<string> getStock(HtmlDocument htmlDocument)
        {
            var q = htmlDocument.DocumentNode.SelectNodes("//div[@class='sc-money-box-wrapper']");

            List<string> stockList = new List<string>();
            foreach (HtmlNode item in q)
            {
                var htmlTitle = new HtmlDocument();
                htmlTitle.LoadHtml(item.InnerHtml);

                var shipping = htmlTitle.DocumentNode.SelectNodes("//div[@class='sc-channel-stock']");
                if (shipping != null)
                    stockList.Add(shipping.ElementAt(0).InnerText);
                else stockList.Add(null);
            }
            return stockList;
        }

        public static List<SamsClubItem> getTitles(HtmlDocument htmlDocument)
        {
            List<SamsClubItem> SamsClubListings = new List<SamsClubItem>();

            var itemCollection = htmlDocument.DocumentNode.SelectNodes("//div[@class='sc-product-card-title']");
            foreach (HtmlNode item in itemCollection)
            {

                var htmlTitle = new HtmlDocument();
                htmlTitle.LoadHtml(item.InnerHtml);

                var title = htmlTitle.DocumentNode.SelectNodes("//a[@href]").FirstOrDefault();

                var href = mainUrl + title.Attributes["href"].Value;
                var ttl = title.InnerText;

                var l = new SamsClubItem();
                l.Title = ttl;
                l.Url = href;
                SamsClubListings.Add(l);
            }
            return SamsClubListings;
        }

        public static List<string> getPrices(HtmlDocument htmlDocument)
        {
            var q = htmlDocument.DocumentNode.SelectNodes("//div[@class='sc-money-box-wrapper']");

            List<string> priceList = new List<string>();
            foreach (HtmlNode x in q)
            {
                var htmlTitle = new HtmlDocument();
                htmlTitle.LoadHtml(x.InnerHtml);
                var priceCollection = htmlTitle.DocumentNode.SelectNodes("//span[@class='Price-group']");
                if (priceCollection != null)
                {
                    string p = priceCollection.ElementAt(0).InnerText;
                    priceList.Add(p);
                }
                else
                    priceList.Add(null);
            }
            return priceList;
        }


    }
}
