using HtmlAgilityPack;
using sclib.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace sclib
{
    public class Scrape
    {
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
            try
            {
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
                }
                return detail;
            }
            catch (Exception exc)
            {
                return null;
            }
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
    }
}
