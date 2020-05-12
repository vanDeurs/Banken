using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Concurrent;

namespace Server
{
    sealed class XmlStoringDocument
    {
        private readonly string savedPosition = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "users.xml");
        private List<Type> listTypes = new List<Type>();
        private static DataContractSerializer serializer;

        // Konstruktor
        public XmlStoringDocument()
        {
            // Lägg till dem klasser som ska serializas
            listTypes.Add(typeof(User));
            listTypes.Add(typeof(CardAccount));
            listTypes.Add(typeof(SavingsAccount));
            listTypes.Add(typeof(Account));

            // Skapa serializer
            serializer = new DataContractSerializer(typeof(ConcurrentDictionary<string, User>), listTypes);

            // Försök att hämta users från XML-dokument. 
            // Om det ej går så hamnar det i catch-blocket där den istället skapar XML-dokumentet.
            ConcurrentDictionary<string, User> users = new ConcurrentDictionary<string, User>();
            try
            {
                using (FileStream inputStream = new FileStream(savedPosition, FileMode.Open, FileAccess.Read))
                {
                    users = (ConcurrentDictionary<string, User>)serializer.ReadObject(inputStream);
                }
            } catch
            {
                try
                {
                    // Serializa ett tomt user dictionary för att skapa XML-dokumentet.
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

        // Skapa eller uppdatera en användare i XML-dokumentet
        public void CreateOrUpdateUser(User user)
        {
            ConcurrentDictionary<string, User> users = new ConcurrentDictionary<string, User>();
            try
            {
                // Hämta lagrade användare
                users = ReadUsers();
            } catch (Exception err)
            {
                Console.WriteLine("Error when fetching users: {0}", err);
                return;
            }

            // Uppdatera eller lägg till användare i dictionaryn
            try
            {
                users.AddOrUpdate(user.SocialSecurityNumber.ToString(), user, (key, oldValue) => user);
            } catch (Exception err)
            {
                Console.WriteLine("Error 1: {0}", err);
                return;
            }

            // Uppdatera XML-dokumentet med den nya user dictionaryn
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

        // Ersätt användare i XML-dokumentet med en ny version av användare
        public void UpdateUsers (ConcurrentDictionary<string, User> updatedUsers)
        {
            try
            {
                using (FileStream outputStream = new FileStream(savedPosition, FileMode.Create, FileAccess.Write))
                {
                    serializer.WriteObject(outputStream, updatedUsers);
                }
            }
            catch (Exception err)
            {
                Console.WriteLine("Error: {0}", err);
                throw;
            }
        }
        
        // Läs in och returnera alla användare från XML-dokumentet
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
