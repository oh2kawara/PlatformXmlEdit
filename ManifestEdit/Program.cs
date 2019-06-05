using System;
using System.Xml;
using Common;

namespace ManifestEdit
{
    class Program
    {
        static void Main(string[] args)
        {
            var opt = new OptionXml(args);

            var manifest = opt.PlatformXml.SelectSingleNode("/manifest");
            if (null == manifest)
            {
                throw new Exception("AndroidManifest.xmlの形式が正しくありません");
            }
            var androidNs = manifest.Attributes.GetNamedItem("xmlns:android")?.Value;
            if (null == androidNs)
            {
                throw new Exception("xmlns:android の値の取得に失敗しました");
            }
            var application = opt.PlatformXml.SelectSingleNode("/manifest/application");
            if (null == application)
            {
                throw new Exception("AndroidManifest/applicationが取得できません");
            }

            XmlNode node;

            // パッケージの書き換え
            node = opt.SettingXml.SelectSingleNode("config/package");
            if (node != null)
            {
                var attr = opt.PlatformXml.CreateAttribute("package");
                attr.Value = node.InnerText;
                manifest.Attributes.SetNamedItem(attr);
            }

            // ラベル名の書き換え
            node = opt.SettingXml.SelectSingleNode("config/application/label");
            if (node != null)
            {
                var attr = opt.PlatformXml.CreateAttribute("android:label", androidNs);
                attr.Value = node.InnerText;
                application.Attributes.SetNamedItem(attr);
            }

            opt.WriteXml();
        }
    }
}
