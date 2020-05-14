using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

class PlantUMLMarkdownReplacer
{

    public string ReplaceRegex { get; private set; }

    public Dictionary<char, char> TranslationDict { get; private set; }

    public PlantUMLMarkdownReplacer()
    {
        var plant_uml_alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_";
        var base64_alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        TranslationDict = base64_alphabet.Zip(plant_uml_alphabet, (f, s) => new Tuple<char, char>(f, s)).ToDictionary(f => f.Item1, f => f.Item2);
        ReplaceRegex = @"```plantuml((.|\n)*?)```";
    }

    private byte[] Compress(byte[] a)
    {
        using (var ms = new System.IO.MemoryStream())
        {
            using (var compressor =
                   new Ionic.Zlib.ZlibStream(ms,
                                              CompressionMode.Compress,
                                              CompressionLevel.Level9))
            {
                compressor.Write(a, 0, a.Length);
            }

            return ms.ToArray();
        }
    }

    public string CreateString(string plantUMLDescription, string plantUMLUrl)
    {
        var utf8_text = Encoding.UTF8.GetBytes(plantUMLDescription.Replace("\r\n", "\n"));

        var compressed = Compress(utf8_text);
        var fskip = compressed.Skip(2);
        compressed = fskip.Take(fskip.Count() - 4).ToArray();
        var b64string = Convert.ToBase64String(compressed).Replace("=", ""); ;

        var translated_plant_uml_string = new string(b64string.ToList().Select(c => TranslationDict[c]).ToArray());
        return translated_plant_uml_string;
    }

    public string ReplaceMarkDown(string markdownDocument, string plantUMLServerUrl)
    {
        var res = Regex.Replace(markdownDocument, ReplaceRegex, m => {
            var platuml_text = m.Groups[1].Value;
            var plant_string = CreateString(platuml_text, plantUMLServerUrl);
            var plant_url = plantUMLServerUrl + plant_string;
            var replacement = $"![alt text]({plant_url})";
            return replacement;
        });
        return res;
    }




}