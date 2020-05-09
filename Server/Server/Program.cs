using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Concurrent;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            // Anslutningsinställningar
            IPAddress myIP = IPAddress.Parse("127.0.0.1");
            int port = 8069;

            // Skapa TcpListernet, börja lyssna och vänta på anslutning
            TcpListener tcpListener = new TcpListener(myIP, port);
            tcpListener.Start();

            Console.WriteLine("Server har startat.");
            Console.WriteLine("Väntar på anslutning...");

            // Skapa XML dokument
            XmlStoringDocument2 xmlDocument;
            try
            {
                xmlDocument = new XmlStoringDocument2();
            }
            catch (XmlDocumentException e)
            {
                Console.WriteLine("Error: {0}", e);
                return;
            }

            // Skapa socket som ska användas för anslutning
            Socket socket = tcpListener.AcceptSocket();
            Console.WriteLine("Anslutning accepterad från {0}", socket.RemoteEndPoint);

            while (true)
            {
                try
                {
                    Console.WriteLine("New message from client..");
                    // Ta emot meddelande
                    Byte[] byteMessage = new byte[256];
                    int byteMessageSize = socket.Receive(byteMessage);

                    // Konvertera meddelande
                    string read = "";
                    for (int i = 0; i < byteMessageSize; i++)
                    {
                        read += Convert.ToChar(byteMessage[i]);
                    }
                    // Välj alternativ utifrån mottaget meddelande
                    int operation = int.Parse(read[0].ToString());
                    string message = read.Remove(0, 1);
                    switch (operation)
                    {
                        // Login
                        case 1:
                            Boolean loggedIn = Login(message, xmlDocument);
                            if (loggedIn)
                            {
                                ReturnToClient(socket, "Successful_Login");
                            } else
                            {
                                ReturnToClient(socket, "Failed_Login");
                            }
                            break;
                        case 2:
                            Boolean createdUser = CreateUser(message, xmlDocument);
                            if (createdUser)
                            {
                                ReturnToClient(socket, "Successful_User_Creation");
                            }
                            else
                            {
                                ReturnToClient(socket, "Failed_User_Creation");
                            }
                            break;
                        case 3:
                            ReadUsers(xmlDocument, socket);
                            break;
                        case 4:
                            ReadAccounts(xmlDocument, socket);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: {0}", e);
                    break;
                }
            }
        }

        private static void ReadUsers (XmlStoringDocument2 xmlDocument, Socket socket)
        {
            // Users
            ConcurrentDictionary<string, User> users = xmlDocument.ReadUsers();

            // Byte array
            byte[] bytesData = ToBytes(users);
            socket.Send(bytesData);
        }

        private static void ReadAccounts(XmlStoringDocument2 xmlDocument, Socket socket)
        {
            // Accounts

            //List<Account> accounts = xmlDocument.ReadAccounts();

            // Byte array
            //byte[] bytesData = ToBytes(accounts);
            //socket.Send(bytesData);
        }

        private static bool CreateUser (string message, XmlStoringDocument2 xmlDocument)
        {
            string[] data = message.Split('.');
            int ssn = int.Parse(data[0]);
            string name = data[1];

            try
            {
                User user = new User(name, ssn);
                xmlDocument.CreateOrUpdateUser(user);
            } catch (Exception err)
            {
                Console.WriteLine("Err in CreateUser(): {0}", err);
                return false;
            }
            return true;
        }

        private static byte[] ToBytes(List<string> users)
        {
            byte[] dataAsBytes = users.AsEnumerable().SelectMany(s => ASCIIEncoding.ASCII.GetBytes(s)).ToArray();
            return dataAsBytes;
        }
        private static byte[] ToBytes(ConcurrentDictionary<string, User> users)
        {
            string data = "";
            Byte[] byteData = Encoding.ASCII.GetBytes(data);
            foreach (KeyValuePair<string, User> user in users)
            {
                data += user.Value.Name;
                data += '.';
                data += user.Key;
                data += '|';
            }
            Console.WriteLine("Data: {0}", data);
            byteData = Encoding.ASCII.GetBytes(data);
            return byteData;
        }
        private static Boolean Login (string ssn, XmlStoringDocument2 xmlDocument)
        {
            ConcurrentDictionary<string, User> users = xmlDocument.ReadUsers();
            foreach (KeyValuePair<string, User> user in users)
            {
                Console.WriteLine("user in login: {0}", user.Value);
                Console.WriteLine("user ssn in login: {0}", user.Key);
                if (user.Key == ssn) return true;
            }
            return false;
        }

        private static void ReturnToClient (Socket socket, string data)
        {
            Byte[] byteData = Encoding.ASCII.GetBytes(data);
            socket.Send(byteData);
        }
    }
}
