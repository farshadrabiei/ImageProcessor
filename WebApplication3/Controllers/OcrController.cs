using Emgu.CV;
using Microsoft.AspNetCore.Mvc;
using Tesseract;

namespace OcrApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OcrController : ControllerBase
    {
        private readonly string _tessDataPath;

        /// <summary>
        /// add eng.traineddata and fas.traineddata in directory tessdata
        /// </summary>
        /// <param name="env"></param>
        public OcrController(IWebHostEnvironment env)
        {
            // مسیر پوشه tessdata
            _tessDataPath = Path.Combine(env.ContentRootPath, "tessdata");
        }

        [HttpPost("extract-text")]
        public async Task<IActionResult> ExtractText(IFormFile imageFile, string lang = "fas+eng")
        {
            if (imageFile == null || imageFile.Length == 0)
                return BadRequest("تصویر ارسال نشده است.");

            // ذخیره موقت فایل
            var filePath = Path.GetTempFileName();
            using (var stream = System.IO.File.Create(filePath))
            {
                await imageFile.CopyToAsync(stream);
            }

            try
            {
                // استفاده از Tesseract
                using var engine = new TesseractEngine(_tessDataPath, lang, EngineMode.Default);
                using var img = Pix.LoadFromFile(filePath);
                using var page = engine.Process(img);

                var text = page.GetText();
                var confidence = page.GetMeanConfidence();

                return Ok(new
                {
                    ExtractedText = text,
                    Confidence = confidence
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"خطا در OCR: {ex.Message}");
            }
            finally
            {
                System.IO.File.Delete(filePath);
            }
        }
    }



}

 