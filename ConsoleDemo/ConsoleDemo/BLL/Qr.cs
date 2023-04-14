﻿using ConsoleDemo.Util;
using System.Drawing;

namespace ConsoleDemo.BLL
{

    /// <summary>
    /// 二维码
    /// </summary>
    public class Qr
    {

        /// <summary>
        /// 启动
        /// </summary>
        public static void Start()
        {
            ZXing.QrCode.Internal.QRCode qr = ZXing.QrCode.Internal.Encoder.encode("ConsoleDemo", ZXing.QrCode.Internal.ErrorCorrectionLevel.H);
            Bitmap bitmap = ImageUtils.QrBytes2Bitmap(qr.Matrix.Array, 10);
            ImageUtils.SaveBitmap(bitmap, "E:/qr.png");
        }

    }

}
