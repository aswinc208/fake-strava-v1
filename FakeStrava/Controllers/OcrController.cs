using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace FakeStrava.Controllers
{
    public class OcrController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public OcrController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View("Upload");
        }
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Content("No file selected");

            using var httpClient = _httpClientFactory.CreateClient();
            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            content.Add(new StreamContent(stream), "file", file.FileName);

            var ocrUrl = "http://ocr-service:8000/ocr";
            var response = await httpClient.PostAsync(ocrUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return Content($"OCR failed: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();

            ViewData["Result"] = json;
            return View("Upload");
        }
    }
}
