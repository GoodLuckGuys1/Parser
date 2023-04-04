namespace OzonLive;

public static class Data
{
    public static object GetData(string url)
    {
        try
        {
            using var hdl = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = System.Net.DecompressionMethods.GZip |
                                         System.Net.DecompressionMethods.Deflate |
                                         System.Net.DecompressionMethods.None,
                CookieContainer = new System.Net.CookieContainer()
            };
            
            using (HttpClient clnt = new HttpClient(hdl, false))
            {
                clnt.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:80.0) Gecko/20100101 Firefox/80.0");
                clnt.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
                clnt.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
                clnt.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                clnt.DefaultRequestHeaders.Add("Connection", "keep-alive");
                clnt.DefaultRequestHeaders.Add("Referer", url);

                using (var resp = clnt
                           .GetAsync(
                               $"https://www.ozon.ru/api/entrypoint-api.bx/page/json/v2?url=" +
                               "/search/?deny_category_prediction=true&from_global=true&" +
                               "text=%D0%B2%D0%B0%D0%BA%D1%83%D1%83%D0%BC%D0%B0%D1%82%D0%BE%D1%80" +
                               "&page_changed=true" +
                               "&layout_container=categorySearchMegapagination" +
                               "&layout_page_index=7&page=7")
                           .Result)
                {
                    if (resp.IsSuccessStatusCode)
                    {
                        var json = resp.Content.ReadAsStringAsync().Result;
                        if (!string.IsNullOrEmpty(json))
                        {
                            object result = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                            return result;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return null;
    }
}