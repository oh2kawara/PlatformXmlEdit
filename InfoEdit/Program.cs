using System;
using System.Collections.Generic;
using System.Xml;
using CommandLine;

namespace InfoEdit
{
    class Program
    {
        #region コマンドラインオプション

        public class Option
        {
            [Value(0, MetaName = "info plist", HelpText = "Info.plistファイルを指定してください", Required = true)]
            public string InfoPlist { get; set; }

            [Option('n', "name", HelpText = "CFBundleDisplayName")]
            public string Name { get; set; }
            [Option('i', "identifier", HelpText = "CFBundleIdentifier")]
            public string Identifier { get; set; }

            [Option("minOsVersion", HelpText = "MinimumOSVersion")]
            public string MinOSVer { get; set; }
        }

        #endregion

        #region DictMap

        public class DictMap
        {
            public class DictPair
            {
                public string key;
                public XmlElement value;
            }

            private XmlDocument xmldoc;
            private XmlNode dict;
            private List<DictPair> list;

            public DictMap(XmlNode dict)
            {
                this.dict = dict;
                xmldoc = dict.OwnerDocument;
                MakeList();
            }

            protected void MakeList()
            {
                list = new List<DictPair>();
                var en = dict.GetEnumerator();

                for(; ; )
                {
                    if (!en.MoveNext()) break;
                    var key = en.Current as XmlElement;
                    if (key == null || key.Name != "key")
                    {
                        throw new Exception("dictノードの形式が正しくありません");
                    }
                    if (!en.MoveNext())
                    {
                        throw new Exception("dictノードが壊れています");
                    }
                    var val = en.Current as XmlElement;
                    if (val == null)
                    {
                        throw new Exception("dictノードが壊れています");
                    }
                    list.Add(new DictPair() {
                        key = key.InnerText,
                        value = val,
                    });
                }
            }

            /// <summary>
            /// 新しいペア情報に書き直す
            /// </summary>
            public void save()
            {
                dict.RemoveAll();
                foreach(var pair in list)
                {
                    var key = xmldoc.CreateElement("key");
                    key.InnerText = pair.key;
                    dict.AppendChild(key);
                    dict.AppendChild(pair.value);
                }
            }

            protected XmlElement CreateStringValue(string value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                var xmlval = xmldoc.CreateElement("string");
                xmlval.InnerText = value;
                return xmlval;
            }

            protected DictPair FindPair(string key)
            {
                if (String.IsNullOrEmpty(key))
                {
                    throw new ArgumentException();
                }

                foreach (var pair in list)
                {
                    if (pair.key == key)
                    {
                        return pair;
                    }
                }
                return null;
            }

            public void SetStringValue(string key, string value)
            {
                var pair = FindPair(key);
                var xmlval = CreateStringValue(value);
                if (null != pair)
                {
                    pair.value = xmlval;
                }
                else
                {
                    list.Add(new DictPair()
                    {
                        key = key,
                        value = xmlval,
                    });
                }
            }
        }

        #endregion

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Option>(args).WithParsed<Option>((opt) => {
                var xmldoc = new XmlDocument();
                xmldoc.Load(opt.InfoPlist);

                var dict = xmldoc.SelectSingleNode("/plist/dict");
                if (null == dict)
                {
                    throw new Exception("Info.plistの形式が正しくありません");
                }
                var dictMap = new DictMap(dict);

                // CFBundleDisplayName
                if (opt.Name != null)
                {
                    dictMap.SetStringValue("CFBundleDisplayName", opt.Name);
                }

                // CFBundleIdentifier
                if (opt.Identifier != null)
                {
                    dictMap.SetStringValue("CFBundleIdentifier", opt.Identifier);
                }

                // MinimumOSVersion
                if (opt.MinOSVer != null)
                {
                    dictMap.SetStringValue("MinimumOSVersion", opt.MinOSVer);
                }

                dictMap.save();
                var outFileName = opt.InfoPlist;
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
