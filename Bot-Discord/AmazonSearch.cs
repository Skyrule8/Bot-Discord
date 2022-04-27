﻿using System.Text.RegularExpressions;
using Discord.WebSocket;
using Scraping.Web;
using TestCode;

namespace Bot_Discord;

public class AmazonSearch
{
    public static Task UrlWithParameter(string parameters, SocketMessage message)
    {
        string words = parameters.Replace(' ', '+');
        Console.WriteLine(words);

        string url = "https://www.amazon.fr/s?k=" + words;
        Console.WriteLine(url);

        var ret = new HttpRequestFluent(true)
            .FromUrl(url)
            .Load();
        
        Console.WriteLine("test-après ret");

        var byClassContain =
            ret.HtmlPage.GetByClassNameContains("sg-col-4-of-12 " +
                                                "s-result-item s-asin sg-col-4-of-16 " +
                                                "sg-col s-widget-spacing-small sg-col-4-of-20");

        Console.WriteLine("test après class");
        var list = new List<AmazonObj>();
        foreach (var result in byClassContain)
        {
            Console.WriteLine("foreach");
            
            if (!result.InnerHtml.Contains(
                    "<i class=\"a-icon a-icon-prime a-icon-medium\" role=\"img\" aria-label=\"Amazon Prime\"></i>"))
                continue;

            var urlPicture = String.Join("", Regex.Matches(result.InnerHtml, @"src=(.+?) srcset")
                    .Select(m => m.Groups[1].Value))
                .Replace('"', ' ')
                .Trim();

            var urlRedirection = String.Join("", Regex.Matches(result.InnerHtml, @"/gp/(.+?)>")
                    .Select(m => m.Groups[1].Value))
                .Replace('"', ' ')
                .Trim();

            var title = String.Join("",
                    Regex.Matches(result.InnerHtml, $@"a-size-medium a-color-base a-text-normal(.+?)</span>")
                        .Select(m => m.Groups[1].Value))
                .Replace('"', ' ')
                .Trim();

            if (title == "")
            {
                title = String.Join("",
                        Regex.Matches(result.InnerHtml, $@"a-size-base a-color-base a-text-normal(.+?)</span>")
                            .Select(m => m.Groups[1].Value))
                    .Replace('"', ' ')
                    .Trim();
            }

            var price = String.Join("", Regex.Matches(result.InnerHtml, @"a-price-whole"">(.+?)</span>")
                    .Select(m => m.Groups[1].Value))
                .Replace('"', ' ')
                .Trim();

            var stars = String.Join("", Regex.Matches(result.InnerHtml, @"a-icon-alt(.+?)étoiles")
                    .Select(m => m.Groups[1].Value))
                .Replace('"', ' ')
                .Replace('>', ' ')
                .Trim();

            // result.InnerText.Replace("\n", " ").TrimStart()

            AmazonObj obj = new AmazonObj()
            {
                Name = title.Replace(">", ""),
                UrlPicture = urlPicture,
                UrlRedirection = urlRedirection,
                Price = price,
                Stars = stars
            };


            return Task.FromResult(message.Channel.SendMessageAsync(
                $"Voici ce que j'ai trouvé : {obj.Name}: - Prix : {obj.Price} - Photo : {obj.UrlPicture} - Rating : {obj.Stars}"));
        }
        return Task.FromResult(message.Channel.SendMessageAsync("c raté"));
    }
}