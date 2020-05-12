using System;
using System.Text;
using System.Net.Sockets;

namespace Client
{
    class Program
    {
        // Bool som visar om en användare är inloggad
        static bool isLoggedIn = false;

        // Lagrar den inloggades personnummer
        static string loggedInSocialSecurityNumber;

        static void Main(string[] args)
        {
            string adress = "127.0.0.1";
            int port = 8083;

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

            // Klientens huvudsakliga struktur
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
                catch (Exception err)
                {
                    Console.WriteLine("Err: {0}", err);
                    continue;
                }
            }
        }

        // Meny som visas för inloggade användare
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

        // Visar användaren konton
        static void ViewAccounts(TcpClient client)
        {
            NetworkStream tcpStream = client.GetStream();

            string message = "4" + loggedInSocialSecurityNumber;
            TcpWrite(tcpStream, message);

            string messageFromServer = "";
            try
            {
                messageFromServer = TcpRead(tcpStream);
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
                    // Olika alternativ som användaren kan göra med sina konton
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
                            return;
                        default:
                            Console.WriteLine("Kunde inte läsa ditt val. Försök igen.");
                            Console.WriteLine("");
                            break;
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

        // Användaren kan skapa ett nytt konto
        private static void CreateAccount(TcpClient client)
        {
            Console.WriteLine("Kontotyper");
            Console.WriteLine("1. Sparkonto");
            Console.WriteLine("2. Kortkonto");
            Console.WriteLine("");
            Console.Write("Typ av konto (välj nummer): ");

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

                    TcpWrite(tcpStream, dataToSend);
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

        // Användaren kan radera ett konto
        private static void DeleteAccount(TcpClient client)
        {
            int number;

            while (true)
            {
                Console.Write("Konto att radera (kontonummer): ");
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

            TcpWrite(tcpStream, dataToSend);
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

        // Användare kan ta ut pengar
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

            TcpWrite(tcpStream, dataToSend);
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

        // Användare kan sätta in pengar
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

            TcpWrite(tcpStream, dataToSend);
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

        // Användaren loggar ut
        static void Logout (TcpClient client)
        {
            isLoggedIn = false;
            loggedInSocialSecurityNumber = "";
        }

        // Meny som visas för utloggade användare
        static void ShowLoggedOutMenu (TcpClient client)
        {
            // Användare väljer ett alternativ
            Console.WriteLine("1. Logga in");
            Console.WriteLine("2. Skapa användare");
            Console.WriteLine("3. Se alla användare");
            Console.WriteLine("4. Stäng program.");
            Console.WriteLine("");

            while (true)
            {
                try
                {
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
                            continue;
                    }
                    return;
                } catch
                {
                    Console.WriteLine("Kunde inte läsa ditt val. Försök igen.");
                    Console.WriteLine("");
                    continue;
                }
            }

        }

        // Radera en användare
        private static void DeleteUser(TcpClient client)
        {

            Console.Write("Användare att radera (personnummer): ");
            string number = Console.ReadLine();

            NetworkStream tcpStream = client.GetStream();

            string dataToSend = "9" + number;

            TcpWrite(tcpStream, dataToSend);
            string messageFromServer = TcpRead(tcpStream);

            if (messageFromServer == "Successful_Operation")
            {
                Console.WriteLine("Du har raderat användaren med personnummer {0}.", number);
            }
            else
            {
                Console.WriteLine("Operation misslyckades.");
            }
        }

        // Stäng av programmet
        static void CloseProgram(TcpClient client)
        {
            client.Close();
            Environment.Exit(1);
        }
        
        // Funktion för att skriva till servern
        static void TcpWrite (NetworkStream tcpStream, string text)
        {
            // Gör om me
            Byte[] byteMessage = Encoding.ASCII.GetBytes(text);
            try
            {
                tcpStream.Write(byteMessage, 0, byteMessage.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error 2: {0}", e);
                throw;
            }
        }

        // Funktion för att läsa in och konvertera meddelanden från servern
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

        // Användare kan logga in
        static void Login (TcpClient client)
        {
            // Skriv in personnummer och skicka till server.
            // Om inloggningen lyckas så är det personnumret som
            // ska skickas med i alla requests för indentifiera personen.
 
            Console.WriteLine("Logga in");
            Console.WriteLine("");
 
            Console.Write("Personnummer: ");
            string socialSecurityNumber = "1" + Console.ReadLine();

            NetworkStream tcpStream = client.GetStream();

            TcpWrite(tcpStream, socialSecurityNumber);
            string messageFromServer = TcpRead(tcpStream);
            
            if (messageFromServer == "Successful_Operation")
            {
                Console.WriteLine("Inloggning lyckades.");
                loggedInSocialSecurityNumber = socialSecurityNumber.Remove(0, 1);
                isLoggedIn = true;
            } else
            {
                Console.WriteLine("Inloggning misslyckades. Försök igen.");
                Console.WriteLine("Server: {0}", messageFromServer);
            }
        }

        // Skapa en ny användare
        static void CreateUser (TcpClient client)
        {
            Console.WriteLine("Ny användare");
            Console.WriteLine("");

            string data = "2";

            while (true)
            {
                try
                {
                    Console.Write("Personnnummer: ");
                    Int64 socialSecurityNumber =  Int64.Parse(Console.ReadLine());

                    Console.Write("Namn: ");
                    string name = Console.ReadLine();

                    data += socialSecurityNumber.ToString();
                    data += ".";
                    data += name;

                    NetworkStream tcpStream = client.GetStream();

                    TcpWrite(tcpStream, data);
                    string messageFromServer = TcpRead(tcpStream);

                    if (messageFromServer == "Successful_Operation")
                    {
                        Console.WriteLine("Användare med namnet '{0}' har skapats.", name);
                    } else
                    {
                        Console.WriteLine("Skapandet misslyckades.");
                    }
                    return;
                } catch
                {
                    Console.WriteLine("Felaktigt input. Försök igen.");
                    Console.WriteLine("");
                    continue;
                }
            }

        }

        // Läs in och skriv ut alla existerande användare
        static void ReadUsers(TcpClient client)
        {
            // Skicka till servern att alla användare ska skickas tillbaka.
            // Servern kollar i XMl.
            NetworkStream tcpStream = client.GetStream();

            TcpWrite(tcpStream, "3");
            string serverMessage = TcpRead(tcpStream);

            if (serverMessage == "no_users")
            {
                Console.WriteLine("Det finns inga sparande användare.");
                return;
            }

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

            while (true)
            {
                try
                {
                    // Användare väljer ett alternativ
                    Console.WriteLine("1. Radera användare.");
                    Console.WriteLine("2. Gå tillbaka.");
                    Console.WriteLine("");
                    Console.Write("Vad vill du göra: ");
                    int option = int.Parse(Console.ReadLine());

                    switch (option)
                    {
                        case 1:
                            DeleteUser(client);
                            break;
                        case 2:
                            return;
                        default:
                            Console.WriteLine("Kunde inte läsa ditt val. Försök igen.");
                            Console.WriteLine("");
                            break;
                    }

                } catch
                {
                    Console.WriteLine("Kunde inte läsa ditt val. Försök igen.");
                    Console.WriteLine("");
                    continue;
                }
            }

        }
    }
}
