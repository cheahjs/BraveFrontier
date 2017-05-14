using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BfPngDecrypt
{
    internal class Program
    {
        private static readonly byte[] key =
        {
            0x00, 0x01, 0x04, 0x09, 0x10, 0x19, 0x24, 0x31, 0x40, 0x51, 0x64, 0x79, 0x90, 0xA9, 0xC4, 0xE1,
            0x00, 0x21, 0x44, 0x69, 0x90, 0xB9, 0xE4, 0x11, 0x40, 0x71, 0xA4, 0xD9, 0x10, 0x49, 0x84, 0xC1,
            0x00, 0x41, 0x84, 0xC9, 0x10, 0x59, 0xA4, 0xF1, 0x40, 0x91, 0xE4, 0x39, 0x90, 0xE9, 0x44, 0xA1,
            0x00, 0x61, 0xC4, 0x29, 0x90, 0xF9, 0x64, 0xD1, 0x40, 0xB1, 0x24, 0x99, 0x10, 0x89, 0x04, 0x81,
            0x00, 0x81, 0x04, 0x89, 0x10, 0x99, 0x24, 0xB1, 0x40, 0xD1, 0x64, 0xF9, 0x90, 0x29, 0xC4, 0x61,
            0x00, 0xA1, 0x44, 0xE9, 0x90, 0x39, 0xE4, 0x91, 0x40, 0xF1, 0xA4, 0x59, 0x10, 0xC9, 0x84, 0x41,
            0x00, 0xC1, 0x84, 0x49, 0x10, 0xD9, 0xA4, 0x71, 0x40, 0x11, 0xE4, 0xB9, 0x90, 0x69, 0x44, 0x21,
            0x00, 0xE1, 0xC4, 0xA9, 0x90, 0x79, 0x64, 0x51, 0x40, 0x31, 0x24, 0x19, 0x10, 0x09, 0x04, 0x01
        };

        private static ProgramState _state = ProgramState.Decode;

        private static void Main(string[] args)
        {
            foreach (var path in args)
            {
                if (path == "-encode")
                {
                    _state = ProgramState.Encode;
                }

                if (File.Exists(path))
                {
                    if (Path.GetExtension(path) != ".jpg" && Path.GetExtension(path) != ".png")
                    {
                        continue;
                    }

                    var bytes = File.ReadAllBytes(path);

                    if (_state == ProgramState.Decode)
                    {
                        if ((bytes[0] == 0xFF && bytes[1] == 0xD9) /*Encoded JPEG*/
                            || (bytes[0] == 0x89 && bytes[1] == 0x51) /*Encoded PNG*/)
                        {
                            Console.WriteLine("Decoding {0}.", Path.GetFileName(path));

                            bytes = DecodeFile(bytes);

                            File.WriteAllBytes(
                                Path.Combine(Path.GetDirectoryName(path), "..", "decoded_images",
                                    Path.GetFileName(path)),
                                bytes);
                            Console.WriteLine("Decoded.");
                        }
                        else
                        {
                            Console.WriteLine("Skipping {0}, not encoded.", Path.GetFileName(path));
                            try
                            {
                                File.Copy(path, Path.Combine(Path.GetDirectoryName(path), "..", "decoded_images",
                                    Path.GetFileName(path)));
                            }
                            catch
                            {
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Encoding {0}.", Path.GetFileName(path));

                        bytes = EncodeFile(bytes);

                        File.WriteAllBytes(
                                Path.Combine(Path.GetDirectoryName(path), "..", "encoded_images",
                                    Path.GetFileName(path)),
                                bytes);
                        Console.WriteLine("Encoded.");
                    }
                }
                else if (Directory.Exists(path))
                {
                    foreach (var file in Directory.GetFiles(path))
                    {
                        if (Path.GetExtension(file) != ".jpg" && Path.GetExtension(file) != ".png")
                        {
                            continue;
                        }

                        var bytes = File.ReadAllBytes(file);

                        if (_state == ProgramState.Decode)
                        {
                            if ((bytes[0] == 0xFF && bytes[1] == 0xD9) /*Encoded JPEG*/
                                || (bytes[0] == 0x89 && bytes[1] == 0x51) /*Encoded PNG*/)
                            {
                                Console.WriteLine("Decoding {0}.", Path.GetFileName(file));

                                bytes = DecodeFile(bytes);

                                File.WriteAllBytes(
                                    Path.Combine(Path.GetDirectoryName(file), "..", "decoded_images",
                                        Path.GetFileName(file)),
                                    bytes);
                                Console.WriteLine("Decoded.");
                            }
                            else
                            {
                                Console.WriteLine("Skipping {0}, not encoded.", Path.GetFileName(file));
                                try
                                {
                                    File.Copy(file, Path.Combine(Path.GetDirectoryName(file), "..", "decoded_images",
                                        Path.GetFileName(file)));
                                }
                                catch (Exception exception)
                                {
                                    Console.WriteLine(exception);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Encoding {0}.", Path.GetFileName(file));

                            bytes = EncodeFile(bytes);

                            File.WriteAllBytes(
                                    Path.Combine(Path.GetDirectoryName(file), "..", "encoded_images",
                                        Path.GetFileName(file)),
                                    bytes);
                            Console.WriteLine("Encoded.");
                        }
                    }
                }
            }
            Console.ReadLine();
        }


        private static byte[] DecodeFile(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] -= key[i % 128];
            }
            return bytes;
        }

        private static byte[] EncodeFile(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] += key[i % 128];
            }
            return bytes;
        }

        private static void GenerateKey()
        {
            Console.Write("0x00,");
            for (byte keyVal = 0, b = 0, increment = 1; b < 127; b++, increment += 2)
            {
                keyVal += increment;
                Console.Write("0x{0},", keyVal.ToString("X").PadLeft(2, '0'));
            }
        }

        private enum ProgramState
        {
            Encode,
            Decode
        }
    }
}
