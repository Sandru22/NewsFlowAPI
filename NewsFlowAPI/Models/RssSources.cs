using Org.BouncyCastle.Crypto.Modes;
using static System.Net.WebRequestMethods;

namespace NewsFlowAPI.Models
{
    public static class RssSources
    {
        public static readonly Dictionary<string, string> FeedCategoryMap = new()
    {
        { "https://www.stiripesurse.ro/rss/sport.xml", "Sport" },
        { "https://www.g4media.ro/sport/feed", "Sport" },
        { "https://www.stiripesurse.ro/rss/politica.xml", "Politica" },
        { "https://www.stiripesurse.ro/rss/externe.xml", "Externe" },
        { "https://www.stiripesurse.ro/rss/meteo.xml", "Meteo" },
        { "https://www.promotor.ro/feed", "Auto" },
        { "https://www.libertatea.ro/tehnologie/feed", "Tech" },
        { "https://arenait.ro/feed/", "Tech" },
        { "https://hotnews.ro/c/economie/feed", "Economie" },
        {"https://hotnews.ro/c/sport/feed"," Sport" },
        {"https://hotnews.ro/c/science/feed", "Tech" },
        {"https://hotnews.ro/c/actualitate/politic", "Politica" },
        {"https://hotnews.ro/c/actualitate/international", "Externe" },
        {"https://hotnews.ro/c/actualitate/sanatate-actualitate/feed", "Sanatate" },
        {"https://www.digi24.ro/rss/stiri/actualitate/politica", "Politica" },
        {"https://www.digi24.ro/rss/stiri/actualitate", "Actuale" },
        {"https://www.digi24.ro/rss/stiri/economie", "Economie" },
        {"https://www.digi24.ro/rss/stiri/sport", "Sport" },
        {"https://www.digi24.ro/rss/stiri/externe", "Externe" },
        {"https://www.digi24.ro/rss/stiri/sci-tech", "Tech" },
        {"https://iamsport.ro/rss","Sport" },
        {"https://playtech.ro/feed/","Tech" },
        {"https://www.go4it.ro/rss", "Tech" },
        {"https://gadget.ro/feed/","Tech" },
        {"https://www.automarket.ro/rss/", "Auto" },
            {"https://0-100.ro/feed/", "Auto" },
            {"https://www.digisport.ro/rss", "Sport" },
            {"https://www.prosport.ro/feed", "Sport" },
            {"https://www.sport.ro/rss", "Sport" },
            {"https://rss.stirileprotv.ro/stiri/politic", "Politica" },
            {"https://rss.stirileprotv.ro/stiri/sport", "Sport" },
            {"https://rss.stirileprotv.ro/stiri/social/", "Sociale" },
            {"https://rss.stirileprotv.ro/stiri/actualitate/", "Actuale" },
            {"https://rss.stirileprotv.ro/stiri/financiar/", "Economie" },
            {"https://rss.stirileprotv.ro/stiri/i-like-ai/", "Tech" },
            {"https://rss.stirileprotv.ro/stiri/international/", "Externe" },
            {"https://rss.stirileprotv.ro/stiri/sanatate/", "Sanatate" },
            {"https://rss.stirileprotv.ro/stiri/vremea/", "Meteo" },
            {"https://www.meteoromania.ro/feed/", "Meteo" },
            {"https://www.b1tv.ro/politica/rss","Politica" },
            {"https://www.b1tv.ro/economic/rss", "Economie" },
            {"https://www.b1tv.ro/high-tech/rss", "Tech" },
            {"https://www.b1tv.ro/auto/rss", "Auto" },
            {"https://www.b1tv.ro/externe/rss", "Externe" },
            {"https://www.b1tv.ro/sport/rss", "Sport" },
            {"https://tvrinfo.ro/category/actualitate/feed/","Actuale" },
            {"https://tvrinfo.ro/category/extern/feed/" , "Externe"},
            {"https://tvrinfo.ro/category/politic/feed/" , "Politica"   }






    };


        public static readonly string[] Feeds = new[]
        {
        "https://www.stiripesurse.ro/rss/sport.xml",
        "https://www.stiripesurse.ro/rss/politica.xml",
        "https://www.stiripesurse.ro/rss/externe.xml",
        "https://www.stiripesurse.ro/rss/meteo.xml",
        "https://www.automarket.ro/rss/",
        "https://www.promotor.ro/feed",
        "https://www.libertatea.ro/tehnologie/feed",
        "https://arenait.ro/feed/",
        "https://www.g4media.ro/feed",
        "https://www.g4media.ro/politica/feed",
        "https://www.g4media.ro/analize/feed",
        "https://hotnews.ro/feed",
        "https://hotnews.ro/c/economie/feed",
        "https://hotnews.ro/c/sport/feed",
        "https://hotnews.ro/c/actualitate/politic",
        "https://hotnews.ro/c/actualitate/international",
        "https://hotnews.ro/c/actualitate/sanatate-actualitate/feed",
        "https://recorder.ro/feed/",
        "https://www.digi24.ro/rss",
        "https://www.digi24.ro/rss/stiri/actualitate",
        "https://www.digi24.ro/rss/stiri/economie",
        "https://www.digi24.ro/rss/stiri/sport",
        "https://www.digi24.ro/rss/stiri/externe",
        "https://rss.stirileprotv.ro/",
        "https://www.news.ro/rss",
        "https://snoop.ro/feed/",
        "https://www.edupedu.ro/feed/",
        "https://www.biziday.ro/feed/",
        "https://iamnews.ro/rss",
        "https://iamsport.ro/rss",
        "https://playtech.ro/feed/",
        "https://www.go4it.ro/rss",
        "https://gadget.ro/feed/",
        "https://www.automarket.ro/rss/",
        "https://www.promotor.ro/feed",
        "https://0-100.ro/feed/",
        "https://www.digisport.ro/rss",
        "https://www.prosport.ro/feed",
        "https://www.sport.ro/rss",
        "https://rss.stirileprotv.ro/",
        "https://rss.stirileprotv.ro/stiri/politic",
        "https://rss.stirileprotv.ro/stiri/sport",
        "https://rss.stirileprotv.ro/stiri/social/",
        "https://rss.stirileprotv.ro/stiri/actualitate/",
        "https://rss.stirileprotv.ro/stiri/i-like-ai/",
        "https://rss.stirileprotv.ro/stiri/financiar/",
        "https://rss.stirileprotv.ro/stiri/sanatate/",
            "https://rss.stirileprotv.ro/stiri/vremea/",
            "https://www.meteoromania.ro/feed/",
            "https://www.b1tv.ro/politica/rss",
            "https://www.b1tv.ro/rss",
            "https://www.b1tv.ro/economic/rss",
            "https://www.b1tv.ro/high-tech/rss",
            "https://www.b1tv.ro/auto/rss",
            "https://www.b1tv.ro/externe/rss",
            "https://www.b1tv.ro/sport/rss",
           "https://www.g4media.ro/sport/feed",
"https://hotnews.ro/c/science/feed",
"https://www.digi24.ro/rss/stiri/actualitate/politica",
"https://www.digi24.ro/rss/stiri/sci-tech",
"https://rss.stirileprotv.ro/stiri/international/",
"https://tvrinfo.ro/",
"https://tvrinfo.ro/category/extern/feed/",
"https://tvrinfo.ro/category/politic/feed/"


    };
    }
}
