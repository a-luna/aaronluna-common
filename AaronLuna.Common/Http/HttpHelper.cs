namespace AaronLuna.Common.Http
{
    using Result;
    using System.Net.Http;
    using System.Threading.Tasks;

    public static class HttpHelper
    {
        public static async Task<Result<string>> GetUrlContentAsStringAsync(string url)
        {
            string urlContent;
            try
            {
                using (var httpClient = new HttpClient())
                using (var httpResonse = await httpClient.GetAsync(url).ConfigureAwait(false))
                {
                    urlContent = await httpResonse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (string.IsNullOrEmpty(urlContent))
                    {
                        return Result.Fail<string>($"Unable to retrieve URL content ({url}), check internet connection");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                return Result.Fail<string>($"{ex.Message} ({ex.GetType()})");
            }

            return Result.Ok(urlContent);
        }
    }
}
