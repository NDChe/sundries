using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SteganographyNetCore;

internal class MainWindowVm : NotifyPropertyChangedBase
{
    /*  Img gets divided into 4 byte block, last bit of each is being used as a information storage.
        Once dropped, press "encrypt" - the image you uploaded will be replaced by the image with 
        information encrypted inside. Press "decrypt" to get the info back. */

    private string _text;
    private BitmapImage? _sourceImage;
    private Bitmap? _sourceImageBitmap;

    public MainWindowVm()
    {
        _text = "";
        _sourceImage = null;
        _sourceImageBitmap = null;
    }

    public ICommand EncryptCommand => new RelayCommands.RelayCommand(param => Encrypt());

    public ICommand DecryptCommand => new RelayCommands.RelayCommand(param => Decrypt());

    public BitmapImage? SourceImage
    {
        get => _sourceImage;
        set => SetField(ref _sourceImage, value);
    }

    public string Text
    {
        get => _text;
        set => SetField(ref _text, value);
    }

    public void GetSourceImage(string file)
    {
        var image = new Bitmap(file);
        _sourceImageBitmap = image;
        SourceImage = BitmapToImageSource(image);
    }

    public void Encrypt()
    {
        if (_sourceImage is null)
            return;

        if (string.IsNullOrEmpty(Text))
            return;

        var textByteArray = Encoding.UTF8.GetBytes(Text + "#over#");
        var newImg = CombineBitmapWithByteArray(_sourceImageBitmap, textByteArray);
        newImg.Save("result.png");
        SourceImage = BitmapToImageSource(newImg);
        Text = string.Empty;
    }

    public void Decrypt()
    {
        if (_sourceImage is null)
            return;

        var textByteArray = RetreiveByteArrayFromBitmap(_sourceImageBitmap);
        var text = System.Text.Encoding.UTF8.GetString(textByteArray);

        if (text.Contains('#'))
            text = text[..text.IndexOf('#')];

        Text = text;
    }

    private Bitmap? CombineBitmapWithByteArray(Bitmap? source, byte[] infoToEncrypt)
    {
        var bitsToEncrypt = new BitArray(infoToEncrypt);
        var k = 0;

        for (var i = 0; i < source.Width; i++)
        {
            for (var j = 0; j < source.Height; j++)
            {
                var pixel = source.GetPixel(i, j);

                if (k >= bitsToEncrypt.Length) break;
                var red = MergeByteWithBit(pixel.R, bitsToEncrypt[k++]);
                if (k >= bitsToEncrypt.Length) break;
                var green = MergeByteWithBit(pixel.G, bitsToEncrypt[k++]);
                if (k >= bitsToEncrypt.Length) break;
                var blue = MergeByteWithBit(pixel.B, bitsToEncrypt[k++]);
                if (k >= bitsToEncrypt.Length) break;

                var newColor = Color.FromArgb(pixel.A, red, green, blue);

                source.SetPixel(i, j, newColor);
            }
        }

        return source;
    }
    private byte[] RetreiveByteArrayFromBitmap(Bitmap? source)
    {
        var bits = new List<bool>();

        for (var i = 0; i < source.Width; i++)
        {
            for (var j = 0; j < source.Height; j++)
            {
                var pixel = source.GetPixel(i, j);
                bits.Add(GetLastBitFromByte(pixel.R));
                bits.Add(GetLastBitFromByte(pixel.G));
                bits.Add(GetLastBitFromByte(pixel.B));
            }
            
        }

        var bytes = BitArrayToByteArray(new BitArray(bits.ToArray()));
        return bytes;
    }

    private byte MergeByteWithBit(byte colorByte, bool bit)
    {
        var colorByteArray = new[] { colorByte };
        var colorBits = Reverse(new BitArray(colorByteArray));
        colorBits[^1] = bit;

        var mergedColorByte = BitArrayToByte(colorBits);
        return mergedColorByte;
    }

    private bool GetLastBitFromByte(byte source) //правильное
    {
        var colorByteArray = new[] { source };
        var colorBits = new BitArray(colorByteArray);
        return colorBits[0];
    }

    //private bool GetLastBitFromByte(byte source)
    //{
    //    var colorByteArray = new[] { source };
    //    var colorBits = new BitArray(colorByteArray);
    //    return colorBits[^1];
    //}

    private BitArray Reverse(BitArray array) //правильное
    {
        var length = array.Length;
        var mid = (length / 2);

        for (var i = 0; i < mid; i++)
        {
            (array[i], array[length - i - 1]) = (array[length - i - 1], array[i]);
        }

        return array;
    }

    private byte[] BitArrayToByteArray(BitArray bits)
    {
        const int BYTE = 8;
        var length = (bits.Count / BYTE) + ((bits.Count % BYTE == 0) ? 0 : 1);
        var bytes = new byte[length];

        for (var i = 0; i < bits.Length; i++)
        {

            var bitIndex = i % BYTE;
            var byteIndex = i / BYTE;

            var mask = (bits[i] ? 1 : 0) << bitIndex;
            bytes[byteIndex] |= (byte)mask;

        }

        return bytes;
    }

    private byte BitArrayToByte(BitArray bits)
    {
        bits = Reverse(bits);

        if (bits.Count != 8)
        {
            throw new ArgumentException("bits");
        }
        var bytes = new byte[1];
        bits.CopyTo(bytes, 0);
        return bytes[0];
    }

    BitmapImage BitmapToImageSource(Bitmap? bitmap)
    {
        using var memory = new MemoryStream();
        bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
        memory.Position = 0;
        var bitmapimage = new BitmapImage();
        bitmapimage.BeginInit();
        bitmapimage.StreamSource = memory;
        bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapimage.EndInit();

        return bitmapimage;
    }
}