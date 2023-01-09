using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace WpfCopyTool
{
    public class AsXML<ListType>
    {
        public List<ListType> KopieItems = new List<ListType>();

        public virtual void ToXML()
        {
            var XMLFileNameType = typeof(ListType).Name + 's';
            string FileName = String.Format("{0}\\{1}_{2}.xml", Init.MainIDdir, Environment.UserName, XMLFileNameType);
            var XMLContaner = ListenWeiche();
            Type[] ExtraTyps = new Type[2] { typeof(DruckerInfo), typeof(Drive) };
            XmlSerializer xs = new XmlSerializer(XMLContaner.GetType(), ExtraTyps);
            TextWriter tw = new StreamWriter(FileName);
            xs.Serialize(tw, XMLContaner);
        }
        public virtual void LoadXML()
        {
            var XMLFileNameType = typeof(ListType).Name + 's';
            string FileName = String.Format("{0}\\{1}_{2}.xml", Init.MainIDdir, Environment.UserName, XMLFileNameType);
            if (!File.Exists(FileName))  return;
            XmlSerializer xs = new XmlSerializer(typeof(XMLContaner));
            using (var sr = new StreamReader(FileName))
            {
                var druckerContainer = (XMLContaner)xs.Deserialize(sr);
                foreach (var item in druckerContainer.GetType().GetProperties())
                {
                    if (item.PropertyType == KopieItems.GetType())
                        KopieItems = (List<ListType>)item.GetValue(druckerContainer);
                }
            }
        }
        XMLContaner ListenWeiche()
        {
            var Container = new XMLContaner();
            var Felder = Container.GetType().GetProperties();
            foreach (var item in Felder)
            {
                if (item.PropertyType == KopieItems.GetType())
                    item.SetValue(Container, KopieItems);
                //else
                //{
                //    item.SetValue(Container, Activator.CreateInstance(item.PropertyType));
                //}
            }
            return Container;
        }

    }
    public class XMLContaner
    {
        public List<DruckerInfo> Drucker { get; set; }

        public List<Drive> drives { get; set; }



    }

}