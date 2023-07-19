using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml;
using Microsoft.AspNetCore.Html;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DealingWithOPML.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public List<Feed> FeedList { get; set; }
        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            FeedList = new List<Feed>();
            _httpClientFactory = httpClientFactory;
        }

        public List<RssItem> RssList { get; set; } = new List<RssItem>();
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int PageCount { get; set; }
        public int PageSize { get; private set; } = 20;
        public List<RssItem> FavoriteList { get; set; } = new List<RssItem>();

        public async Task<IActionResult> OnGet(int pageNumber = 1)
        {
            var httpClient = _httpClientFactory.CreateClient();
            HttpResponseMessage opmlResponse = await httpClient.GetAsync("https://blue.feedland.org/opml?screenname=dave");

            if (opmlResponse.IsSuccessStatusCode)
            {
                string opmlContent = await opmlResponse.Content.ReadAsStringAsync();
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(opmlContent);

                XmlNodeList outlineNodes = xmlDocument.GetElementsByTagName("outline");
                foreach (XmlNode outlineNode in outlineNodes)
                {
                    if (outlineNode.Attributes["xmlUrl"] != null)
                    {
                        Feed feed = new Feed();
                        feed.XmlUrl = outlineNode.Attributes["xmlUrl"].Value;
                        feed.HtmlUrl = outlineNode.Attributes["htmlUrl"].Value;
                        feed.Text = outlineNode.Attributes["text"].Value;

                        FeedList.Add(feed);
                    }
                }

                List<RssItem> rssItems = new List<RssItem>();
                foreach (var feed in FeedList)
                {
                    string xml = await httpClient.GetStringAsync(feed.XmlUrl);
                    XmlDocument document1 = new XmlDocument();
                    document1.LoadXml(xml);
                    XmlNodeList itemNodes = document1.GetElementsByTagName("item");

                    foreach (XmlElement itemNode in itemNodes)
                    {
                        RssItem item = new RssItem();
                        item.Title = itemNode.SelectSingleNode("title")?.InnerText;
                        item.Description = itemNode.SelectSingleNode("description")?.InnerText;
                        item.Link = itemNode.SelectSingleNode("link")?.InnerText;
                        item.PubDate = itemNode.SelectSingleNode("pubDate")?.InnerText;

                        rssItems.Add(item);
                    }
                }

                RssList = rssItems;

                TotalPages = (int)Math.Ceiling((double)RssList.Count / PageSize);
                PageNumber = pageNumber;

                RssList = RssList
                    .Skip((pageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();
            }
            else
            {
                return StatusCode((int)opmlResponse.StatusCode);
            }

            return Page();
        }

    }
    public class RssItem
    {
        public string Title { get; set; }
        public string PubDate { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public bool IsFavorite { get; set; }
    }

    public class Feed
    {
        public string XmlUrl { get; set; }
        public string HtmlUrl { get; set; }
        public string Text { get; set; }
    }
}