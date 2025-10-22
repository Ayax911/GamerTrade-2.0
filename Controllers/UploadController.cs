using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;

namespace APiGamer.Controllers
{
    public class UploadController : ControllerBase
    {
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadJuego(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest("No se envió ningún archivo.");

            var supabaseUrl = "https://wsaxsxynrtoexbupxdni.supabase.co";
            var supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6IndzYXhzeHlucnRvZXhidXB4ZG5pIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc2MTA4NjIyMiwiZXhwIjoyMDc2NjYyMjIyfQ.GeI7KuEFef0lXSOFx5FsLgHU4GkgLUOJVrlecwH6uCs"; // o anon key si el bucket es público
            var bucket = "Juegos";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", supabaseKey);

            var fileName = $"{Guid.NewGuid()}_{archivo.FileName}";
            var content = new MultipartFormDataContent
    {
        { new StreamContent(archivo.OpenReadStream()), "file", fileName }
    };

            var response = await client.PostAsync($"{supabaseUrl}/storage/v1/object/{bucket}/{fileName}", content);
            if (!response.IsSuccessStatusCode)
                return BadRequest(await response.Content.ReadAsStringAsync());

            var fileUrl = $"{supabaseUrl}/storage/v1/object/public/{bucket}/{fileName}";
            return Ok(new { fileUrl });
        }

    }
}
