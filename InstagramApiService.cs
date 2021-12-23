using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace PhilippsSmartGarten.DailyInstagramPost;

public class InstagramApiService
{
    private readonly string _instagramBusinessId;

    public class CreateMediaObjectResult
    {
        public string Id { get; set; }
    }

    public class PublishMediaObjectResult
    {
        public string Id { get; set; }
    }

    private HttpClient _client;

    public InstagramApiService(string instagramBusinessId)
    {
        _instagramBusinessId = instagramBusinessId;
        _client = new HttpClient
        {
            BaseAddress = new Uri("https://graph.facebook.com")
        };
    }

    public async Task<CreateMediaObjectResult> CreateMediaObject(string accessToken, string imageUrl, string caption)
    {
        var requestUri =
            $"{_instagramBusinessId}/media?image_url={HttpUtility.UrlEncode(imageUrl)}&caption={HttpUtility.UrlEncode(caption)}&access_token={accessToken}";

        var response = await _client.PostAsync(requestUri, null);
        var responseString = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var createMediaObjectResult = JsonSerializer.Deserialize<CreateMediaObjectResult>(responseString, options);

        return createMediaObjectResult;
    }

    public async Task<PublishMediaObjectResult> PublishMediaObject(string accessToken, string mediaObjectId)
    {
        var requestUri = $"{_instagramBusinessId}/media_publish?creation_id={mediaObjectId}&access_token={accessToken}";

        var response = await _client.PostAsync(requestUri, null);
        var responseString = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var createMediaObjectResult = JsonSerializer.Deserialize<PublishMediaObjectResult>(responseString, options);

        return createMediaObjectResult;
    }
}