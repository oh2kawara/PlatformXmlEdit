using System;
using System.Xml;
using CommandLine;

namespace Common
{
    /// <summary>
    /// コマンドラインオプションから取得できるXMLファイル管理
    /// </summary>
    public class OptionXml
    {
        #region コマンドラインオプション

        public class Option
        {
            [Value(0, MetaName = "manifest file", HelpText = "マニフェストXMLファイルを指定してください", Required = true)]
            public string XmlFile { get; set; }

            [Value(1, MetaName = "setting file", HelpText = "セッティングXMLファイルを指定してください", Required = true)]
            public string SettingFile { get; set; }
        }

        #endregion

        public OptionXml(string[] args)
        {
            Parser.Default.ParseArguments<Option>(args)
            .WithParsed<Option>((opt) =>
            {
                PlatformXml = new XmlDocument();
                PlatformXml.Load(opt.XmlFile);
                PlatformXmlFileName = opt.XmlFile;

                SettingXml = new XmlDocument();
                SettingXml.Load(opt.SettingFile);
            })
            .WithNotParsed<Option>((errorList) =>
            {
                //foreach(var err in errorList)
                //{
                //    Console.WriteLine(err);
                //}
                System.Environment.Exit(1);
            });
        }

        /// <summary>
        /// プラットフォームXMLファイル名
        /// </summary>
        public string PlatformXmlFileName { get; private set; }

        /// <summary>
        /// プラットフォーム設定XML
        /// </summary>
        public XmlDocument PlatformXml { get; private set; }

        /// <summary>
        /// セッティングXML
        /// </summary>
        public XmlDocument SettingXml { get; private set; }

        /// <summary>
        /// 更新したプラットフォームXMLを保存
        /// </summary>
        public void WriteXml(XmlDocument xmldoc = null)
        {
            if (xmldoc == null)
            {
                xmldoc = PlatformXml;
            }
            var outFileName = PlatformXmlFileName;
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
        }
    }
}
