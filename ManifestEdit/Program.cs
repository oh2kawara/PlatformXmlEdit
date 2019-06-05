using System;
using CommandLine;
using System.Xml;

namespace ManifestEdit
{
    class Program
    {
        #region コマンドラインオプション

        public class Option
        {
            [Value(0, MetaName = "manifest file", HelpText = "マニフェストXMLファイルを指定してください", Required = true)]
            public string XmlFile { get; set; }

            [Option('p', "package", HelpText = "パッケージ名")]
            public string Package { get; set; }
            [Option('l', "label", HelpText = "アプリ名[一覧用]")]
            public string Label { get; set; }
        }

        #endregion

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Option>(args).WithParsed<Option>((opt) => {
                var xmldoc = new XmlDocument();
                xmldoc.Load(opt.XmlFile);

                var manifest = xmldoc.SelectSingleNode("/manifest");
                if (null == manifest)
                {
                    throw new Exception("AndroidManifest.xmlの形式が正しくありません");
                }
                var androidNs = manifest.Attributes.GetNamedItem("xmlns:android")?.Value;
                if (null == androidNs)
                {
                    throw new Exception("xmlns:android の値の取得に失敗しました");
                }

                if (opt.Package != null)
                {
                    // パッケージの書き換え
                    var attr = xmldoc.CreateAttribute("package");
                    attr.Value = opt.Package;
                    manifest.Attributes.SetNamedItem(attr);
                }

                if (opt.Label != null)
                {
                    // ラベル名の書き換え
                    var application = xmldoc.SelectSingleNode("/manifest/application");
                    if (application != null)
                    {
                        var attr = xmldoc.CreateAttribute("android:label", androidNs);
                        attr.Value = opt.Label;
                        application.Attributes.SetNamedItem(attr);
                    }
                }

                var outFileName = opt.XmlFile;
#if DEBUG
                outFileName += ".test.xml";
#endif
                var xmlsetting = new XmlWriterSettings()
                {
                    Async = false,
                    CheckCharacters = true,
                    CloseOutput = true,
                    Encoding = new System.Text.UTF8Encoding(false),
                    Indent = true,
                    IndentChars = "\t",
                    NewLineChars = "\n",
                };
                var writer = XmlWriter.Create(outFileName, xmlsetting);
                xmldoc.Save(writer);
            });
        }
    }
}
