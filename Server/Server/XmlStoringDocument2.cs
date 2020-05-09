using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Runtime.Serialization;
using System.Collections.Concurrent;

namespace Server
{
    sealed class XmlStoringDocument2
    {
        //private readonly string savedPosition = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location + @"\users.xml");
        private readonly string savedPosition = "C:\\Users/Alexander/Documents/users.xml";
        private List<Type> listTypes = new List<Type>();
        private static DataContractSerializer serializer;

        // Konstruktor
        public XmlStoringDocument2()
        {
            listTypes.Add(typeof(User));
            listTypes.Add(typeof(CardAccount));
            listTypes.Add(typeof(SavingsAccount));
            listTypes.Add(typeof(Account));

            serializer = new DataContractSerializer(typeof(ConcurrentDictionary<string, User>), listTypes);

            // Skapa XML dokument
            ConcurrentDictionary<string, User> users = new ConcurrentDictionary<string, User>();
            try
            {
                using (FileStream inputStream = new FileStream(savedPosition, FileMode.Open, FileAccess.Read))
                {
                    users = (ConcurrentDictionary<string, User>)serializer.ReadObject(inputStream);
                }
            } catch (Exception err)
            {
                Console.WriteLine("Err 1: {0}", err);
                try
                {
                    using (FileStream outputStream = new FileStream(savedPosition, FileMode.Create, FileAccess.Write))
                    {
                        serializer.WriteObject(outputStream, users);
                    }
                }
                catch (Exception err1)
                {
                    Console.WriteLine("Error: {0}", err1);
                    throw;
                }
            }
        }
        public void CreateOrUpdateUser(User user)
        {
            // Get users from XML
            // Add a user to the users dictionary
            // Save XML to file again

            // Get current users
            ConcurrentDictionary<string, User> users = new ConcurrentDictionary<string, User>();
            try
            {
                users = ReadUsers();
            } catch (Exception err)
            {
                Console.WriteLine("Error when fetching users: {0}", err);
                return;
            }

            // Add new user to list of users
            try
            {
                users.AddOrUpdate(user.SocialSecurityNumber.ToString(), user, (key, oldValue) => user);
                Console.WriteLine("Updated/added user: {0}", users);
            } catch (Exception err)
            {
                Console.WriteLine("Error 1: {0}", err);
                return;
            }

            // Update XML with new list
            try
            {
                using (FileStream outputStream = new FileStream(savedPosition, FileMode.Create, FileAccess.Write))
                {
                    serializer.WriteObject(outputStream, users);
                }
            }
            catch (Exception err)
            {
                Console.WriteLine("Error: {0}", err);
                throw;
            }
        }
        public ConcurrentDictionary<string, User> ReadUsers()
        {
            ConcurrentDictionary<string, User> users = new ConcurrentDictionary<string, User>();
            try
            {
                using (FileStream inputStream = new FileStream(savedPosition, FileMode.Open, FileAccess.Read))
                {
                    users = (ConcurrentDictionary<string, User>)serializer.ReadObject(inputStream);
                }
                return users;
            } catch (Exception err)
            {
                Console.WriteLine("Error: {0}", err);
                throw;
            }
        }
    }
}
