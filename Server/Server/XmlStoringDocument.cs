using System;
using System.Collections.Generic;
using System.Xml;

namespace Server
{
    sealed class XmlStoringDocument : XmlDocument
    {
        private string savedPosition;
        private XmlDocument xmlDocument;

        // Konstruktor
        public XmlStoringDocument()
        {
            savedPosition = "C:\\Users/alexander/documents/users.xml";
            // Skapa XML dokumentC:\Users\Alexander\Documents\StoraInlämning\Server\Server\XmlStoringDocument2.cs
            xmlDocument = new XmlDocument();

            // Ange version och encoding for XML dokument...
            XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "utf-8", null);

            // ... och lägg till i dokumentet
            xmlDocument.AppendChild(xmlDeclaration);

            // Lägg till rotelementet "messages" där alla meddelanden kommer att sparas
            XmlNode messages = xmlDocument.CreateElement("usersList");
            xmlDocument.AppendChild(messages);

            // Spara till fil
            try
            {
                xmlDocument.Save(savedPosition);
            }
            catch (Exception err)
            {
                Console.WriteLine("Err: {0}", err);
                throw new XmlDocumentException("Kunde inte spara xml dokument");
            }
        }
        public void CreateUser(User user)
        {
            xmlDocument.Load(savedPosition);

            // Skapa användare
            XmlNode newUser = xmlDocument.CreateElement("user");

            // Lägg till namn
            XmlElement name = xmlDocument.CreateElement("name");
            name.InnerText = user.Name;

            /// ... och personnummer
            XmlElement socialSecurityNumber = xmlDocument.CreateElement("ssn");
            socialSecurityNumber.InnerText = user.SocialSecurityNumber.ToString();


            newUser.AppendChild(name);
            newUser.AppendChild(socialSecurityNumber);

            // Lägg till i messageList i dokumentet
            XmlNode rootList = xmlDocument.SelectSingleNode("usersList");
            if (rootList == null)
            {
                throw new XmlDocumentException("Rootlist är null i AddUser()");
            }
            rootList.AppendChild(newUser);

            Console.WriteLine("rootList: {0}", rootList);
            xmlDocument.Save(savedPosition);

            ReadUsers();
        }
        public IDictionary<int, string> ReadUsers()
        {
            // Ladda in dokument och hämta meddelanden
            this.xmlDocument.Load(savedPosition);
            XmlNodeList users = xmlDocument.SelectNodes("usersList");
            Console.WriteLine("users in readusers(): {0}", users.Count);

            if (users == null)
            {
                throw new XmlDocumentException("Rootlist är null i ReadUsers()");
            }

            //List<XmlNode> usersList = new List<XmlNode>();
            IDictionary<int, string> usersList = new Dictionary<int, string>();

            // Gå igenom dokumentet och spara alla meddelanden i en List
            foreach (XmlNode root in users)
            {
                Console.WriteLine("root: {0}", root);
                foreach (XmlNode user in root.ChildNodes)
                {
                    string name = "";
                    int ssn = 0;
                    int index = 0;
                    List <Account> accounts = new List<Account>();

                    Console.WriteLine("user 1: {0}", user);
                    foreach (XmlNode children in user.ChildNodes)
                    {
                        Console.WriteLine("Children: {0}", children.InnerText);

                        if (index == 0) name = children.InnerText;
                        else ssn = int.Parse(children.InnerText);
                        index++;
                    }
                    usersList.Add(ssn, name);
                }
            }
            return usersList;
        }
        /*public List<Account> ReadAccounts()
        {
            // Ladda in dokument och hämta meddelanden
            this.xmlDocument.Load(savedPosition);
            XmlNodeList users = xmlDocument.SelectNodes("usersList");
            Console.WriteLine("users in readusers(): {0}", users.Count);

            if (users == null)
            {
                throw new XmlDocumentException("Rootlist är null i ReadUsers()");
            }

            //List<XmlNode> usersList = new List<XmlNode>();
            IDictionary<int, string> usersList = new Dictionary<int, string>();

            // Gå igenom dokumentet och spara alla meddelanden i en List
            foreach (XmlNode root in users)
            {
                Console.WriteLine("root: {0}", root);
                foreach (XmlNode user in root.ChildNodes)
                {
                    string name = "";
                    int ssn = 0;
                    int index = 0;

                    Console.WriteLine("user 1: {0}", user);
                    foreach (XmlNode children in user.ChildNodes)
                    {
                        Console.WriteLine("Children: {0}", children.InnerText);

                        if (index == 0) name = children.InnerText;
                        else ssn = int.Parse(children.InnerText);
                        index++;
                    }
                    usersList.Add(ssn, name);
                }
            }
            return usersList;
        }*/
    }
}
