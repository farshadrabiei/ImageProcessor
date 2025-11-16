using Microsoft.AspNetCore.Mvc;

namespace WebApplication3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private string key = "123";
        // حذف کانال R
        [HttpPost("remove-red")]
        public IActionResult RemoveRedChannel(IFormFile file)
        {
            var result = ImageProcessor.RemoveRedChannel(file);
            return File(result, "image/jpeg");
        }

        //   تبدیل به Grayscale
        [HttpPost("grayscale")]
        public IActionResult ConvertToGray(IFormFile file)
        {
            var result = ImageProcessor.ConvertToGray(file);
            return File(result, "image/jpeg");
        }

        //  کاهش کیفیت JPEG
        [HttpPost("compress")]
        public IActionResult CompressJpeg(IFormFile file, [FromQuery] int quality = 50)
        {
            var result = ImageProcessor.CompressJpeg(file, quality);
            return File(result, "image/jpeg");
        }


        [HttpPost("remove-red")]
        public IActionResult RemoveRedChannelEncrypt(IFormFile file)
        {
            var result = ImageProcessorEncrypt.RemoveRedChannel(file, key);
            return File(result, "image/jpeg");
        }

       
        [HttpPost("grayscale")]
        public IActionResult ConvertToGrayEncrypt(IFormFile file)
        {
            var result = ImageProcessorEncrypt.ConvertToGray(file, key);
            return File(result, "image/jpeg");
        }

      
        [HttpPost("compress")]
        public IActionResult CompressJpegEncrypt(IFormFile file, [FromQuery] int quality = 50)
        {
            var result = ImageProcessorEncrypt.CompressJpeg(file, key, quality);
            return File(result, "image/jpeg");
        }
    }
}
