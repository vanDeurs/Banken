using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace Client
{
    class Program
    {
        static bool isLoggedIn = false;
        static string loggedInSocialSecurityNumber;

        static void Main(string[] args)
        {
            string adress = "127.0.0.1";
            int port = 8069;

            // Anslut till server
            TcpClient client = new TcpClient();
            try
            {
                client.Connect(adress, port);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error 1: {0}", e);
                return;
            }

            while (true)
            {
                try
                {
                    if (isLoggedIn)
                    {
                        ShowLoggedInMenu(client);
                        continue;
                    }
                    else
                    {
                        ShowLoggedOutMenu(client);
                        continue;
                    }
                }
                catch
                {
                    Console.WriteLine("Kunde inte läsa ditt val. Försök igen.");
                    Console.WriteLine("");
                    continue;
                }
            }
        }

        static void ShowLoggedInMenu (TcpClient client)
        {
            Console.WriteLine("1. Se konton");
            Console.WriteLine("2. Logga ut");

            int option = int.Parse(Console.ReadLine());

            switch (option)
            {
                case 1:
                    ViewAccounts(client);
                    break;
                case 2:
                    Logout(client);
                    break;
                default:
                    Console.WriteLine("Kunde inte läsa ditt val. Försök igen.");
                    Console.WriteLine("");
                    break;
            }
        }

        static void ViewAccounts(TcpClient client)
        {
            // Tell server to return accounts
            NetworkStream tcpStream = client.GetStream();

            string message = "4" + loggedInSocialSecurityNumber;

            string clientMessage = TcpWrite(tcpStream, message);

            Console.WriteLine("Klient: {0}", clientMessage);

            if (clientMessage != "Servern har tagit emot din förfrågan.") return;

            try
            {
                string messageFromServer = TcpRead(tcpStream);
                Console.WriteLine("Konton");
                Console.WriteLine("");
                Console.WriteLine(messageFromServer);
            } catch (Exception err)
            {
                Console.WriteLine("Error: {0}", err);
                return;
            }

            int option;
            while (true)
            {
                PresentAccountOptions(client);
                try
                {
                    option = int.Parse(Console.ReadLine());
                    break;
                }
                catch
                {
                    Console.WriteLine("Kunde inte läsa ditt val. Försök igen. 1");
                    Console.WriteLine("");
                    continue;
                }
            }

            switch (option)
            {
                case 1:
                    InsertMoney(client);
                    break;
                case 2:
                    TakeOutMoney(client);
                    break;
                case 3:
                    DeleteAccount(client);
                    break;
                case 4:
                    CreateAccount(client);
                    break;
                case 5:
                    break;
                default:
                    Console.WriteLine("Kunde inte läsa ditt val. Försök igen. 2");
                    Console.WriteLine("");
                    break;
            }
        }

        private static void CreateAccount(TcpClient client)
        {
            Console.WriteLine("Kontotyper");
            Console.WriteLine("1. Sparkonto");
            Console.WriteLine("2. Kortkonto");
            Console.WriteLine("");
            Console.Write("Typ av konto: ");

            while (true)
            {
                try
                {
                    int option = int.Parse(Console.ReadLine());
                    string account = "";
                    
                    switch (option)
                    {
                        case 1:
                            account = "SavingsAccount";
                            break;
                        case 2:
                            account = "CardAccount";
                            break;
                        default:
                            Console.WriteLine("Felaktigt val. Försök igen.");
                            break;
                    }
                    Console.Write("Ange kontonamn: ");
                    string name = Console.ReadLine();

                    string dataToSend = "8" + loggedInSocialSecurityNumber + "|" + account + "|" + name;

                    NetworkStream tcpStream = client.GetStream();

                    string clientMessage = TcpWrite(tcpStream, dataToSend);
                    Console.WriteLine("Klient: {0}", clientMessage);

                    if (clientMessage != "Servern har tagit emot din förfrågan.") return;

                    string messageFromServer = TcpRead(tcpStream);

                    if (messageFromServer == "Successful_Operation")
                    {
                        Console.WriteLine("Konto med namn {0} har öppnats.", name);
                    }
                    else
                    {
                        Console.WriteLine("Operationen misslyckades.");
                    }
                    break;
                } catch
                {
                    Console.WriteLine("Felaktigt val. Försök igen.");
                    Console.WriteLine("");
                    continue;
                }
            }
        }

        private static void DeleteAccount(TcpClient client)
        {
            int number;

            while (true)
            {
                Console.Write("Kontonummer att radera: ");
                bool numberSuccess = int.TryParse(Console.ReadLine(), out number);

                if (!numberSuccess)
                {
                    Console.WriteLine("Nånting gick fel med din input. Försök igen.");
                    continue;
                }
                else
                {
                    break;
                }
            }

            NetworkStream tcpStream = client.GetStream();

            string dataToSend = "7" + loggedInSocialSecurityNumber + "|" + number;

            string clientMessage = TcpWrite(tcpStream, dataToSend);
            Console.WriteLine("Klient: {0}", clientMessage);

            if (clientMessage != "Servern har tagit emot din förfrågan.") return;

            string messageFromServer = TcpRead(tcpStream);

            if (messageFromServer == "Successful_Operation")
            {
                Console.WriteLine("Konto med nummer {0} har raderats.",number);
            }
            else
            {
                Console.WriteLine("Operationen misslyckades.");
            }
        }

        private static void TakeOutMoney(TcpClient client)
        {
            int number;
            decimal balance;

            while (true)
            {
                Console.Write("Kontonummer att ta ut pengar från: ");
                bool numberSuccess = int.TryParse(Console.ReadLine(), out number);

                Console.Write("Summa att ta ut: ");
                bool balanceSuccess = decimal.TryParse(Console.ReadLine(), out balance);

                if (!numberSuccess || !balanceSuccess)
                {
                    Console.WriteLine("Nånting gick fel med din input. Försök igen.");
                    continue;
                } else
                {
                    break;
                }
            }

            NetworkStream tcpStream = client.GetStream();

            string dataToSend = "6" + loggedInSocialSecurityNumber + "|" + number + "|" + balance;

            string clientMessage = TcpWrite(tcpStream, dataToSend);
            Console.WriteLine("Klient: {0}", clientMessage);

            if (clientMessage != "Servern har tagit emot din förfrågan.") return;

            string messageFromServer = TcpRead(tcpStream);

            if (messageFromServer == "Successful_Operation")
            {
                Console.WriteLine("{0} har tagits ut från konto med nummer {1}", balance, number);
            }
            else
            {
                Console.WriteLine("Uttaget misslyckades.");
            }
        }

        private static void InsertMoney(TcpClient client)
        {
            int number;
            decimal balance;

            while (true)
            {
                Console.Write("Kontonummer att sätta in pengar i: ");
                bool numberSuccess = int.TryParse(Console.ReadLine(), out number);

                Console.Write("Summa att sätta in: ");
                bool balanceSuccess = decimal.TryParse(Console.ReadLine(), out balance);

                if (!numberSuccess || !balanceSuccess)
                {
                    Console.WriteLine("Nånting gick fel med din input. Försök igen.");
                    continue;
                }
                else
                {
                    break;
                }
            }

            NetworkStream tcpStream = client.GetStream();

            string dataToSend = "5" + loggedInSocialSecurityNumber + "|" + number + "|" + balance;

            string clientMessage = TcpWrite(tcpStream, dataToSend);
            Console.WriteLine("Klient: {0}", clientMessage);

            if (clientMessage != "Servern har tagit emot din förfrågan.") return;

            string messageFromServer = TcpRead(tcpStream);

            if (messageFromServer == "Successful_Operation")
            {
                Console.WriteLine("{0} har satts in på konto med nummer {1}", balance, number);
            }
            else
            {
                Console.WriteLine("Intaget misslyckades.");
            }
        }

        static void PresentAccountOptions (TcpClient client)
        {
            Console.WriteLine("1. Sätt in pengar");
            Console.WriteLine("2. Ta ut pengar");
            Console.WriteLine("3. Avsluta konto");
            Console.WriteLine("4. Öppna nytt konto");
            Console.WriteLine("5. Gå tillbaka");
        }

        static void Logout (TcpClient client)
        {
            isLoggedIn = false;
            loggedInSocialSecurityNumber = "";
        }

        static void ShowLoggedOutMenu (TcpClient client)
        {
            // Användare väljer ett alternativ
            Console.WriteLine("1. Logga in");
            Console.WriteLine("2. Skapa användare");
            Console.WriteLine("3. Se alla användare");
            Console.WriteLine("4. Stäng program.");
            Console.WriteLine("");
            Console.Write("Vad vill du göra: ");

            int option = int.Parse(Console.ReadLine());

            switch (option)
            {
                case 1:
                    Login(client);
                    break;
                case 2:
                    CreateUser(client);
                    break;
                case 3:
                    ReadUsers(client);
                    break;
                case 4:
                    CloseProgram(client);
                    break;
                default:
                    Console.WriteLine("Kunde inte läsa ditt val. Försök igen.");
                    Console.WriteLine("");
                    break;
            }
        }

        private static void DeleteUser(TcpClient client)
        {

            Console.Write("Användare att radera (personnummer): ");
            string number = Console.ReadLine();

            NetworkStream tcpStream = client.GetStream();

            string dataToSend = "9" + number;

            string clientMessage = TcpWrite(tcpStream, dataToSend);
            Console.WriteLine("Klient: {0}", clientMessage);

            if (clientMessage != "Servern har tagit emot din förfrågan.") return;

            string messageFromServer = TcpRead(tcpStream);

            if (messageFromServer == "Successful_Operation")
            {
                Console.WriteLine("Operationen lyckades.");
            }
            else
            {
                Console.WriteLine("Inloggning misslyckades. Försök igen.");
                Console.WriteLine("Server: {0}", messageFromServer);
            }
        }

        static void CloseProgram(TcpClient client)
        {
            client.Close();
            Environment.Exit(1);
        }
        static String TcpWrite (NetworkStream tcpStream, string text)
        {
            // Gör om me
            Byte[] byteMessage = Encoding.ASCII.GetBytes(text);
            string clientMessage;
            try
            {
                tcpStream.Write(byteMessage, 0, byteMessage.Length);
                clientMessage = "Servern har tagit emot din förfrågan.";
                return clientMessage;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error 2: {0}", e);
                clientMessage = e.ToString();
            }
            return clientMessage;
        }

        static String TcpRead (NetworkStream tcpStream)
        {
            // Ta emot meddelande
            Byte[] byteMessageRead = new byte[256];
            int byteMessageReadSize;

            string message;

            try
            {
                byteMessageReadSize = tcpStream.Read(byteMessageRead, 0, byteMessageRead.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error 3: {0}", e);
                message = e.ToString();
                return message;
            }

            // Konvertera meddelande till string
            string read = "";
            for (int i = 0; i < byteMessageReadSize; i++)
            {
                read += Convert.ToChar(byteMessageRead[i]);
            }
            message = read;
            return message;
        }

        static void Login (TcpClient client)
        {
            // Skriv in personnummer och skicka till server.
            // Om inloggningen lyckas så är det personnumret som
            // ska skickas med i alla requests för indentifiera personen.
 
            Console.WriteLine("Logga in");
            Console.WriteLine("");
 
            Console.WriteLine("Personnummer: ");
            string socialSecurityNumber = "1" + Console.ReadLine();

            NetworkStream tcpStream = client.GetStream();

            string clientMessage = TcpWrite(tcpStream, socialSecurityNumber);
            Console.WriteLine("Klient: {0}", clientMessage);

            if (clientMessage != "Servern har tagit emot din förfrågan.") return;

            string messageFromServer = TcpRead(tcpStream);
            
            if (messageFromServer == "Successful_Operation")
            {
                // Store the logged in social security number so we know which account is logged in.
                Console.WriteLine("Inloggning lyckades.");
                loggedInSocialSecurityNumber = socialSecurityNumber.Remove(0, 1);
                isLoggedIn = true;
            } else
            {
                Console.WriteLine("Inloggning misslyckades. Försök igen.");
                Console.WriteLine("Server: {0}", messageFromServer);
            }
        }
        static void CreateUser (TcpClient client)
        {
            // Skriv in personnummer för användare
            // Få tillbaka ett svar på om det fungerade.

            Console.WriteLine("Ny användare");
            Console.WriteLine("");
            Console.Write("Personnnummer: ");

            string data = "2";

            string socialSecurityNumber = Console.ReadLine();

            Console.Write("Namn: ");
            string name = Console.ReadLine();

            data += socialSecurityNumber;
            data += ".";
            data += name;

            NetworkStream tcpStream = client.GetStream();

            string clientMessage = TcpWrite(tcpStream, data);
            Console.WriteLine("Klient: {0}", clientMessage);

            if (clientMessage != "Servern har tagit emot din förfrågan.") return;

            string messageFromServer = TcpRead(tcpStream);

            if (messageFromServer == "Successful_Operation")
            {
                Console.WriteLine("Användare med namnet '{0}' har skapats.", name);
            } else
            {
                Console.WriteLine("Skapandet misslyckades. Försök igen.");
                Console.WriteLine("Server: {0}", messageFromServer);
            }
        }
        static void ReadUsers(TcpClient client)
        {
            // Skicka till servern att alla användare ska skickas tillbaka.
            // Servern kollar i XMl.
            NetworkStream tcpStream = client.GetStream();

            string clientMessage = TcpWrite(tcpStream, "3");
            Console.WriteLine("Klient: {0}", clientMessage);

            if (clientMessage != "Servern har tagit emot din förfrågan.") return;

            string serverMessage = TcpRead(tcpStream);

            // Dela upp strängen efter en breakpoint så att vi kan
            // få ut användarna.
            string[] users = serverMessage.Split(char.Parse("|"));

            for (int i = 0; i < users.Length; i++)
            {
                string[] user = users[i].Split('.');
                string name = user[0];
                string ssn = user[1];
                Console.WriteLine("Namn: {0}", name);
                Console.WriteLine("Personnummer: {0}", ssn);
                Console.WriteLine("----------------------------");
            }
            // Användare väljer ett alternativ
            Console.WriteLine("1. Radera användare.");
            Console.WriteLine("2. Gå tillbaka.");
            Console.WriteLine("");
            Console.Write("Vad vill du göra 1: ");

            int option = int.Parse(Console.ReadLine());

            switch (option)
            {
                case 1:
                    DeleteUser(client);
                    break;
                case 2:
                    break;
                default:
                    Console.WriteLine("Kunde inte läsa ditt val 1. Försök igen.");
                    Console.WriteLine("");
                    break;
            }
        }
    }
}
