using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using System.Threading.Tasks;
using System.Net.Http;

namespace ReadRenderRSSFeed.Pages
{
    public class IndexModel : PageModel
    {
        public List<RssData> RssList { get; set; } = new();

        public async Task OnGet()
        {
            RssList = new List<RssData>();
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync("http://scripting.com/rss.xml");

                if (response.IsSuccessStatusCode)
                {

                    string XmlContent = await response.Content.ReadAsStringAsync();

                    XmlDocument XmlDoc = new XmlDocument();
                    XmlDoc.LoadXml(XmlContent);

                    foreach (XmlNode node in XmlDoc.SelectNodes("rss/channel/item").Cast<XmlNode>())
                    {
                        RssData RssDataObj = new RssData
                        {
                            Title = node.SelectSingleNode("title")?.InnerText,
                            Description = node.SelectSingleNode("description")?.InnerText,
                            PubDate = node.SelectSingleNode("pubDate")?.InnerText,
                            Link = node.SelectSingleNode("link")?.InnerText,
                        };

                        RssList.Add(RssDataObj);
                    }
                }
                else
                {
                    StatusCode((int)response.StatusCode);
                }
            }
        }
    }

    public class RssData
    {
        public string? Title { get; set; }
        public string? PubDate { get; set; }
        public String? Description { get; set; }
        public string? Link { get; set; }

    }
}
