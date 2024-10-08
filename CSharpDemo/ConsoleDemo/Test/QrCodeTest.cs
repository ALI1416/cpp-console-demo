using ConsoleDemo.Util;
using NUnit.Framework;
using System;
using System.Drawing;
using Z.QRCodeEncoder.Net;
using ZXing;

namespace ConsoleDemo.Test
{

    /// <summary>
    /// 二维码测试
    /// </summary>
    [TestFixture]
    public class QrCodeTest
    {

        private static readonly string content = "爱上对方过后就哭了啊123456789012345678901234567890";
        private static readonly int level = 3;
        private static readonly string path = "E:/qr2.png";

        /// <summary>
        /// 测试
        /// </summary>
        [Test]
        public static void Test()
        {
            // 生成二维码
            QRCode qr = new QRCode(content, level);
            Bitmap bitmap = ImageUtils.QrBytes2Bitmap(qr.Matrix, 10);
            ImageUtils.SaveBitmap(bitmap, path);
            // 识别二维码
            BarcodeReader reader = new BarcodeReader();
            Bitmap bitmapResult = new Bitmap(path);
            Result result = reader.Decode(bitmapResult);
            string contentResult = result.ToString();
            Console.WriteLine(contentResult);
            Assert.That(content, Is.EqualTo(contentResult));
        }

    }
}
