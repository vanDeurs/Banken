﻿using System;
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
                        case 1:
                            Boolean loggedIn = Login(message, xmlDocument);
                            if (loggedIn)
                            {
                                ReturnToClient(socket, "Successful_Operation");
                            } else
                            {
                                ReturnToClient(socket, "Failed_Operation");
                            }
                            break;
                        case 2:
                            Boolean createdUser = CreateUser(message, xmlDocument);
                            if (createdUser)
                            {
                                ReturnToClient(socket, "Successful_Operation");
                            }
                            else
                            {
                                ReturnToClient(socket, "Failed_Operation");
                            }
                            break;
                        case 3:
                            ReadUsers(xmlDocument, socket);
                            break;
                        case 4:
                            ReadAccounts(message, xmlDocument, socket);
                            break;
                        case 5:
                            Boolean insertedMoney = TransferMoney(true, message, xmlDocument);
                            if (insertedMoney)
                            {
                                ReturnToClient(socket, "Successful_Operation");
                            }
                            else
                            {
                                ReturnToClient(socket, "Failed_Operation");
                            }
                            break;
                        case 6:
                            Boolean tookOutMoney = TransferMoney(false, message, xmlDocument);
                            if (tookOutMoney)
                            {
                                ReturnToClient(socket, "Successful_Operation");
                            } else
                            {
                                ReturnToClient(socket, "Failed_Operation");
                            }
                            break;
                        case 7:
                            Boolean deletedAccount = DeleteAccount(message, xmlDocument);
                            if (deletedAccount)
                            {
                                ReturnToClient(socket, "Successful_Operation");
                            }
                            else
                            {
                                ReturnToClient(socket, "Failed_Operation");
                            }
                            break;
                        case 8:
                            Boolean createdAccount = CreateAccount(message, xmlDocument);
                            if (createdAccount)
                            {
                                ReturnToClient(socket, "Successful_Operation");
                            }
                            else
                            {
                                ReturnToClient(socket, "Failed_Operation");
                            }
                            break;
                        case 9:
                            Boolean deletedUser = DeleteUser(message, xmlDocument);
                            if (deletedUser)
                            {
                                ReturnToClient(socket, "Successful_Operation");
                            }
                            else
                            {
                                ReturnToClient(socket, "Failed_Operation");
                            }
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

        private static bool DeleteUser(string serverMessage, XmlStoringDocument2 xmlDocument)
        {
            string socialSecurityNumberToDelete = serverMessage;

            ConcurrentDictionary<string, User> users = xmlDocument.ReadUsers();
            foreach (KeyValuePair<string, User> user in users)
            {
                if (user.Key == socialSecurityNumberToDelete)
                {
                    try
                    {
                        users.TryRemove(user.Key, out User deletedUser);
                        xmlDocument.UpdateUsers(users);
                        return true;
                    } catch (Exception err)
                    {
                        Console.WriteLine("Error: {0}", err);
                        return false;
                    }
                };
            }
            return false;
        }

        private static bool CreateAccount(string serverMessage, XmlStoringDocument2 xmlDocument)
        {
            string[] information = serverMessage.Split(char.Parse("|"));

            string socialSecurityNumber = information[0];
            string accountType = information[1];
            string accountName = information[2];

            try
            {
                User loggedInUser = GetUser(socialSecurityNumber, xmlDocument);
                loggedInUser.CreateAccount(accountType, accountName);
                xmlDocument.CreateOrUpdateUser(loggedInUser);
                return true;
            }
            catch (Exception err)
            {
                Console.WriteLine("Err: {0}", err);
            }
            return false;
        }

        private static bool DeleteAccount(string serverMessage, XmlStoringDocument2 xmlDocument)
        {
            string[] information = serverMessage.Split(char.Parse("|"));

            string socialSecurityNumber = information[0];
            string accountNumber = information[1];

            try
            {
                User loggedInUser = GetUser(socialSecurityNumber, xmlDocument);
                loggedInUser.DeleteAccount(accountNumber);
                xmlDocument.CreateOrUpdateUser(loggedInUser);
                return true;
            }
            catch (Exception err)
            {
                Console.WriteLine("Err: {0}", err);
            }
            return false;
        }

        private static bool TransferMoney(bool insertMoney, string serverMessage, XmlStoringDocument2 xmlDocument)
        {
            string[] information = serverMessage.Split(char.Parse("|"));

            string socialSecurityNumber = information[0];
            string accountNumber = information[1];
            decimal balance = decimal.Parse(information[2]);

            try
            {
                User loggedInUser = GetUser(socialSecurityNumber, xmlDocument);
                Account account = loggedInUser[accountNumber];
                if (insertMoney) account.AddFunds(balance);
                else account.TakeOutFunds(balance);
                xmlDocument.CreateOrUpdateUser(loggedInUser);
                return true;
            }
            catch (Exception err)
            {
                Console.WriteLine("Err: {0}", err);
            }
            return false;
        }

        private static void ReadUsers (XmlStoringDocument2 xmlDocument, Socket socket)
        {
            // Users
            byte[] bytesData;
            ConcurrentDictionary<string, User> users = xmlDocument.ReadUsers();

            if (users.Count == 0)
            {
                bytesData = Encoding.ASCII.GetBytes("Det finns inga sparade användare.");
            } else
            {
                bytesData = ToBytes(users);
            }
            socket.Send(bytesData);
        }

        private static Account GetUserAccount(string socialSecurityNumber, string accountNumber, XmlStoringDocument2 xmlDocument)
        {
            // Users
            List<Account> loggedInUserAccounts = GetUser(socialSecurityNumber, xmlDocument).Accounts;

            if (loggedInUserAccounts.Count > 0)
            {
                foreach (Account account in loggedInUserAccounts)
                {
                    if (account.Number == int.Parse(accountNumber))
                    {
                        return account;
                    }
                }
            }
            throw new Exception("No account found.");
        }

        private static User GetUser (string socialSecurityNumber, XmlStoringDocument2 xmlDocument)
        {
            // Users
            ConcurrentDictionary<string, User> users = xmlDocument.ReadUsers();

            foreach (KeyValuePair<string, User> user in users)
            {
                if (user.Key == socialSecurityNumber)
                {
                    return user.Value;
                };
            }
            throw new Exception("No user found.");
        }

        private static void ReadAccounts(string socialSecurityNumber, XmlStoringDocument2 xmlDocument, Socket socket)
        {
            // Users
            List<Account> loggedInUserAccounts = new List<Account>();
            try
            {
                loggedInUserAccounts = GetUser(socialSecurityNumber, xmlDocument).Accounts;
            } catch (Exception err)
            {
                Console.WriteLine("Error: {0}", err);
                byte[] errorData = Encoding.ASCII.GetBytes("Ett fel uppstod.");
                socket.Send(errorData);
                return;
            }
            string dataToSend = "";

            if (loggedInUserAccounts == null)
            {
                Console.WriteLine("Logged in user cannot be found..");
                dataToSend = "Ett fel uppstod. Kan ej hitta användare.";
            }
            else if (loggedInUserAccounts.Count == 0)
            {
                Console.WriteLine("No accounts on user.");
                dataToSend = "Du har inga konton.";
            }
            else
            {
                string accounts = "";
                foreach (Account account in loggedInUserAccounts)
                {
                    string name = "Namn: " + account.Name;
                    string balance = "Saldo: " + account.Balance.ToString();
                    string number = "Kontonummer: " + account.Number.ToString();
                    accounts += name;
                    accounts += "\n";
                    accounts += number;
                    accounts += "\n";
                    accounts += balance;
                    accounts += "\n";
                    accounts += "--------------------";
                    accounts += "\n";
                }
                Console.WriteLine("Accounts: {0}", accounts);
                dataToSend = accounts;
            }

            // Transfer to bytes and send
            byte[] bytesData= Encoding.ASCII.GetBytes(dataToSend);
            socket.Send(bytesData);
        }

        private static bool CreateUser (string message, XmlStoringDocument2 xmlDocument)
        {
            string[] data = message.Split('.');
            string ssn = data[0];
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
            byteData = Encoding.ASCII.GetBytes(data);
            return byteData;
        }
        private static Boolean Login (string ssn, XmlStoringDocument2 xmlDocument)
        {
            ConcurrentDictionary<string, User> users = xmlDocument.ReadUsers();
            foreach (KeyValuePair<string, User> user in users)
            {
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
