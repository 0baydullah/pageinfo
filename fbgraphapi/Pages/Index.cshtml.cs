using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace fbgraphapi.Pages
{

    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public string GraphVersion { get; set; } = "";

        [BindProperty]
        public string PageId { get; set; } = "";

        [BindProperty]
        public string PageAccessToken { get; set; } = "";

        [BindProperty]
        public string Fields { get; set; } = "id,name";

        public string? Result { get; set; }

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(GraphVersion))
            {
                ErrorMessage = "Graph API version is required.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(PageId))
            {
                ErrorMessage = "Page ID is required.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(PageAccessToken))
            {
                ErrorMessage = "Page Access Token is required.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Fields))
            {
                Fields = "id,name";
            }

            var client = _httpClientFactory.CreateClient();

            var url =
                $"https://graph.facebook.com/" +
                $"{GraphVersion}/" +
                $"{PageId}" +
                $"?fields={Uri.EscapeDataString(Fields)}" +
                $"&access_token={Uri.EscapeDataString(PageAccessToken)}";

            try
            {
                var response = await client.GetAsync(url);

                var content = await response.Content.ReadAsStringAsync();

                try
                {
                    using var jsonDocument =
                        JsonDocument.Parse(content);

                    Result = JsonSerializer.Serialize(
                        jsonDocument,
                        new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });
                }
                catch
                {
                    Result = content;
                }

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = Result;
                    Result = null;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            return Page();
        }

    }
}

