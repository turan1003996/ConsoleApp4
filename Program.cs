using System;
using System.Globalization;

namespace ConsoleApp4
{

    class InsufficientBalanceException : Exception
    {
        public InsufficientBalanceException(string message) : base(message)
        {
        }
    }


    class TransactionRecord
    {
        public DateTime Timestamp { get; set; }
        public string Description { get; set; }

        public TransactionRecord(string description, DateTime? customTime = null)
        {
            Timestamp = customTime ?? DateTime.Now;
            Description = description;
        }

        public override string ToString()
        {
            return $"[{Timestamp:dd.MM.yyyy HH:mm:ss}] {Description}";
        }
    }

    class Card
    {
        public string PAN { get; set; }
        public string PIN { get; set; }
        public string CVC { get; set; }
        public string ExpireDate { get; set; }
        public decimal Balance { get; set; }


        private TransactionRecord[] history = new TransactionRecord[100];
        private int historyCount = 0;

        public Card(string pan, string pin, string cvc, string expireDate, decimal balance)
        {
            PAN = pan;
            PIN = pin;
            CVC = cvc;
            ExpireDate = expireDate;
            Balance = balance;
        }

        public void AddTransaction(string description, DateTime? customTime = null)
        {
            if (historyCount < history.Length)
            {
                history[historyCount] = new TransactionRecord(description, customTime);
                historyCount++;
            }
        }

        public void ShowTransactions(int? lastDays = null)
        {
            Console.WriteLine("\n--- Əməliyyatlar tarixçəsi ---");
            bool found = false;
            DateTime cutoff = lastDays.HasValue ? DateTime.Now.AddDays(-lastDays.Value) : DateTime.MinValue;

            for (int i = 0; i < historyCount; i++)
            {
                if (!lastDays.HasValue || history[i].Timestamp >= cutoff)
                {
                    Console.WriteLine(history[i].ToString());
                    found = true;
                }
            }

            if (!found)
            {
                Console.WriteLine("Seçilmiş dövr üzrə əməliyyat tapılmadı.");
            }
        }
    }

    class User
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public Card CreditCard { get; set; }

        public User(string name, string surname, Card creditCard)
        {
            Name = name;
            Surname = surname;
            CreditCard = creditCard;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            User[] users = CreateUsers();

            while (true)
            {
                User currentUser = AuthenticateByPin(users);
                Console.WriteLine($"\n{currentUser.Name} {currentUser.Surname} xoş gəlmisiniz, zəhmət olmasa aşağıdakılardan birini seçərdiniz:");

                bool stayInMenu = true;
                while (stayInMenu)
                {
                    Console.WriteLine("\n1. Balans");
                    Console.WriteLine("2. Nağd pul");
                    Console.WriteLine("3. Edilən əməliyyatların siyahısı");
                    Console.WriteLine("4. Kartdan karta köçürmə");
                    Console.WriteLine("0. Kartı çıxar / Çıxış");
                    Console.Write("Seçiminiz: ");

                    string choice = Console.ReadLine()?.Trim();

                    switch (choice)
                    {
                        case "1":
                            Console.WriteLine($"\nCari balansınız: {currentUser.CreditCard.Balance} AZN");
                            break;

                        case "2":
                            HandleWithdraw(currentUser);
                            break;

                        case "3":
                            HandleTransactions(currentUser);
                            break;

                        case "4":
                            HandleTransfer(currentUser, users);
                            break;

                        case "0":
                            Console.WriteLine("\nTəşəkkür edirik! Əsas ekrana qayıdılır...\n");
                            stayInMenu = false;
                            break;

                        default:
                            Console.WriteLine("Yanlış seçim! Yenidən cəhd edin.");
                            break;
                    }
                }
            }
        }

        static User[] CreateUsers()
        {
            User[] users = new User[5];

            users[0] = new User("Turan", "Şahvələdov",
                new Card("4169738812345671", "1111", "342", "09/28", 450.00m));
            users[0].CreditCard.AddTransaction("İlkin balans: 450 AZN", DateTime.Now.AddDays(-8));
            users[0].CreditCard.AddTransaction("Market ödənişi: -25 AZN", DateTime.Now.AddDays(-3));

            users[1] = new User("Aysel", "Məmmədova",
                new Card("4169738812345672", "2222", "719", "11/27", 120.00m));

            users[2] = new User("Murad", "Əliyev",
                new Card("4169738812345673", "3333", "508", "04/29", 800.00m));

            users[3] = new User("Nigar", "Həsənova",
                new Card("4169738812345674", "4444", "921", "01/28", 65.50m));

            users[4] = new User("Elvin", "Quliyev",
                new Card("4169738812345675", "5555", "184", "07/27", 300.00m));

            return users;
        }

        static User AuthenticateByPin(User[] users)
        {
            while (true)
            {
                Console.Write("Zəhmət olmasa PIN kodu daxil edin: ");
                string pin = Console.ReadLine()?.Trim();

                for (int i = 0; i < users.Length; i++)
                {
                    if (users[i].CreditCard.PIN == pin)
                    {
                        return users[i];
                    }
                }

                Console.WriteLine("bu PIN koda aid kart tapilmadi\n");
            }
        }

        static void HandleWithdraw(User user)
        {
            Console.WriteLine("\nMəbləği seçin:");
            Console.WriteLine("1. 10 AZN");
            Console.WriteLine("2. 20 AZN");
            Console.WriteLine("3. 50 AZN");
            Console.WriteLine("4. 100 AZN");
            Console.WriteLine("5. Digər");
            Console.Write("Seçiminiz: ");

            string option = Console.ReadLine()?.Trim();
            decimal amount = 0;

            switch (option)
            {
                case "1":
                    amount = 10m;
                    break;
                case "2":
                    amount = 20m;
                    break;
                case "3":
                    amount = 50m;
                    break;
                case "4":
                    amount = 100m;
                    break;
                case "5":
                    Console.Write("Çıxarmaq istədiyiniz məbləği daxil edin (AZN): ");
                    if (!decimal.TryParse(Console.ReadLine()?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out amount) || amount <= 0)
                    {
                        Console.WriteLine("Düzgün müsbət məbləğ daxil edin!");
                        return;
                    }
                    break;
                default:
                    Console.WriteLine("Yanlış seçim!");
                    return;
            }

            try
            {
                if (amount > user.CreditCard.Balance)
                {
                    throw new InsufficientBalanceException("Balansda yeterli qeder mebleg yoxdur");
                }

                user.CreditCard.Balance -= amount;
                user.CreditCard.AddTransaction($"Nağd pul çıxarışı: -{amount} AZN (Balans: {user.CreditCard.Balance} AZN)");
                Console.WriteLine($"{amount} AZN nağdlaşdırıldı. Yeni balans: {user.CreditCard.Balance} AZN");
            }
            catch (InsufficientBalanceException ex)
            {
                Console.WriteLine($"Xəta: {ex.Message}");
            }
        }

        static void HandleTransactions(User user)
        {
            Console.WriteLine("\nHansı dövr üzrə əməliyyatlara baxmaq istəyirsiniz?");
            Console.WriteLine("0. Bütün əməliyyatlar");
            Console.WriteLine("1. Son 1 gün");
            Console.WriteLine("2. Son 5 gün");
            Console.WriteLine("3. Son 10 gün");
            Console.Write("Seçiminiz: ");

            string filter = Console.ReadLine()?.Trim();

            switch (filter)
            {
                case "0":
                    user.CreditCard.ShowTransactions(null);
                    break;
                case "1":
                    user.CreditCard.ShowTransactions(1);
                    break;
                case "2":
                    user.CreditCard.ShowTransactions(5);
                    break;
                case "3":
                    user.CreditCard.ShowTransactions(10);
                    break;
                default:
                    Console.WriteLine("Yanlış seçim!");
                    break;
            }
        }

        static void HandleTransfer(User sender, User[] users)
        {
            Console.Write("\nKöçürmə etmək istədiyiniz kartın PIN kodunu daxil edin: ");
            string targetPin = Console.ReadLine()?.Trim();

            User recipient = null;
            for (int i = 0; i < users.Length; i++)
            {
                if (users[i].CreditCard.PIN == targetPin)
                {
                    recipient = users[i];
                    break;
                }
            }

            if (recipient == null)
            {
                Console.WriteLine("Daxil etdiyiniz PIN koda aid kart tapılmadı.");
                return;
            }

            if (recipient == sender)
            {
                Console.WriteLine("Öz kartınıza köçürmə edə bilməzsiniz.");
                return;
            }

            Console.Write($"Köçürüləcək məbləği daxil edin ({recipient.Name} {recipient.Surname} kartına, AZN): ");
            if (!decimal.TryParse(Console.ReadLine()?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount) || amount <= 0)
            {
                Console.WriteLine("Düzgün müsbət məbləğ daxil edin!");
                return;
            }

            try
            {
                if (amount > sender.CreditCard.Balance)
                {
                    throw new InsufficientBalanceException("Balansda yeterli qeder mebleg yoxdur");
                }

                sender.CreditCard.Balance -= amount;
                recipient.CreditCard.Balance += amount;

                sender.CreditCard.AddTransaction($"Köçürmə ({recipient.Name} {recipient.Surname}): -{amount} AZN");
                recipient.CreditCard.AddTransaction($"Mədaxil ({sender.Name} {sender.Surname}-dən): +{amount} AZN");

                Console.WriteLine($"Köçürmə uğurla tamamlandı! Yeni balansınız: {sender.CreditCard.Balance} AZN");
            }
            catch (InsufficientBalanceException ex)
            {
                Console.WriteLine($"Xəta: {ex.Message}");
            }
        }
    }
}