using Microsoft.AspNetCore.Http;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class ImageProcessor 
{
    // متد عمومی برای خواندن فایل و دیکد کردن
    private static Mat LoadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("فایل ورودی نامعتبر است.");

        using var ms = new MemoryStream();
        file.CopyTo(ms);

        var img = new Mat();
        CvInvoke.Imdecode(ms.ToArray(), ImreadModes.AnyColor, img);
        return img;
    }

    // متد عمومی برای Encode کردن خروجی به JPEG
    private static byte[] EncodeToJpeg(Mat mat, int quality = 80)
    {
        using var outBuf = new VectorOfByte();
        CvInvoke.Imencode(".jpg", mat, outBuf,
            new KeyValuePair<ImwriteFlags, int>[] {
                new KeyValuePair<ImwriteFlags, int>(ImwriteFlags.JpegQuality, quality)
            });
        return outBuf.ToArray();
    }

    // 1️⃣ حذف کانال R
    public static byte[] RemoveRedChannel(IFormFile file, int jpegQuality = 80)
    {
        using var img = LoadImage(file);

        using var channels = new VectorOfMat();
        CvInvoke.Split(img, channels);

        if (channels.Size >= 3)
            channels[2].SetTo(new MCvScalar(0)); // حذف کانال R

        using var reduced = new Mat();
        CvInvoke.Merge(channels, reduced);

        return EncodeToJpeg(reduced, jpegQuality);
    }

    // 2️⃣ تبدیل به Grayscale
    public static byte[] ConvertToGray(IFormFile file, int jpegQuality = 80)
    {
        using var img = LoadImage(file);

        using var gray = new Mat();
        CvInvoke.CvtColor(img, gray, ColorConversion.Bgr2Gray);

        return EncodeToJpeg(gray, jpegQuality);
    }

    // 3️⃣ کاهش حجم JPEG
    public static byte[] CompressJpeg(IFormFile file, int jpegQuality = 50)
    {
        using var img = LoadImage(file);
        return EncodeToJpeg(img, jpegQuality);
    }
}

public static class ImageProcessorEncrypt
{
    // متد عمومی برای خواندن فایل و دیکد کردن
    private static Mat LoadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("فایل ورودی نامعتبر است.");

        using var ms = new MemoryStream();
        file.CopyTo(ms);

        var img = new Mat();
        CvInvoke.Imdecode(ms.ToArray(), ImreadModes.AnyColor, img);
        return img;
    }

    // متد عمومی برای Encode کردن خروجی به JPEG
    private static byte[] EncodeToJpeg(Mat mat, int quality = 80)
    {
        using var outBuf = new VectorOfByte();
        CvInvoke.Imencode(".jpg", mat, outBuf,
            new KeyValuePair<ImwriteFlags, int>[] {
                new KeyValuePair<ImwriteFlags, int>(ImwriteFlags.JpegQuality, quality)
            });
        return outBuf.ToArray();
    }

    // متد رمزنگاری با AES
    private static byte[] Encrypt(byte[] data, string key)
    {
        using var aes = Aes.Create();
        aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes(key)); // کلید 256 بیتی
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        ms.Write(aes.IV, 0, aes.IV.Length); // ذخیره IV در ابتدای فایل
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        cs.Write(data, 0, data.Length);
        cs.FlushFinalBlock();
        return ms.ToArray();
    }

    // متد دیکریپت
    public static byte[] Decrypt(byte[] encryptedData, string key)
    {
        using var aes = Aes.Create();
        aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes(key));

        using var ms = new MemoryStream(encryptedData);
        var iv = new byte[16];
        ms.Read(iv, 0, iv.Length);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var result = new MemoryStream();
        cs.CopyTo(result);
        return result.ToArray();
    }

    // 1️⃣ حذف کانال R و ذخیره رمزنگاری شده
    public static byte[] RemoveRedChannel(IFormFile file, string key, int jpegQuality = 80)
    {
        using var img = LoadImage(file);

        using var channels = new VectorOfMat();
        CvInvoke.Split(img, channels);

        if (channels.Size >= 3)
            channels[2].SetTo(new MCvScalar(0)); // حذف کانال R

        using var reduced = new Mat();
        CvInvoke.Merge(channels, reduced);

        var jpegData = EncodeToJpeg(reduced, jpegQuality);
        return Encrypt(jpegData, key);
    }

    // 2️⃣ تبدیل به Grayscale و ذخیره رمزنگاری شده
    public static byte[] ConvertToGray(IFormFile file, string key, int jpegQuality = 80)
    {
        using var img = LoadImage(file);

        using var gray = new Mat();
        CvInvoke.CvtColor(img, gray, ColorConversion.Bgr2Gray);

        var jpegData = EncodeToJpeg(gray, jpegQuality);
        return Encrypt(jpegData, key);
    }

    // 3️⃣ کاهش حجم JPEG و ذخیره رمزنگاری شده
    public static byte[] CompressJpeg(IFormFile file, string key, int jpegQuality = 50)
    {
        using var img = LoadImage(file);
        var jpegData = EncodeToJpeg(img, jpegQuality);
        return Encrypt(jpegData, key);
    }
}



