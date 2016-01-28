using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refactoring
{
    public class Tusc
    {
        const int EXIT = 7;

        public static void Start(List<User> users, List<Product> products)
        {
            DisplayWelcomeMessage();
            UserCredentials userCredentials = PromptForUserCredentials(users);

            if (userCredentials.AreValid)
            {
                DisplayLoginSuccessfulMessage(userCredentials.Name);
                User currentUser = GetUser(users, userCredentials);
                
                DisplayBalance(currentUser.Balance);
                GoShopping(currentUser, products);
                UpdateAccounts(users, products);

                PromptUserToExit();
                return;
            }
        }

        private static UserCredentials PromptForUserCredentials(List<User> users)
        {
            var userCredentials = new UserCredentials();
            string name = PromptForUserName();

            while (!userCredentials.AreValid && !string.IsNullOrEmpty(name))
            {
                if (!UserNameIsValid(users, name))
                {
                    DisplayInvalidUserNameMessage();
                    name = PromptForUserName();
                }
                else
                {
                    string password = PromptForUserPassword();
                    userCredentials = new UserCredentials(name, password);

                    if (!PasswordIsValid(users, userCredentials))
                    {
                        DisplayInvalidPasswordMessage();
                        name = PromptForUserName();
                    }
                    else
                    {
                        userCredentials.AreValid = true;
                    }
                }
            }
            return userCredentials;
        }

        private static void UpdateAccounts(List<User> users, List<Product> products)
        {
            SaveChangedBalances(users);
            SaveChangedQuantities(products);
        }

        private static void GoShopping(User currentUser, List<Product> products)
        {
            double userBalance = currentUser.Balance;
            bool exitRequested = false;

            while (!exitRequested)
            {
                int itemSelected = PromptForPurchaseItem(products);

                if (SelectedItemIsProduct(itemSelected))
                {
                    Product selectedProduct = products[itemSelected];
                    DisplayPurchaseSummary(selectedProduct, userBalance);

                    int requestedQuantity = PromptForPurchaseQuantity();

                    if (UserDoesNotHaveEnoughMoneyForPurchase(selectedProduct, userBalance, requestedQuantity))
                    {
                        DisplayNotEnoughMoneyMessage();
                        continue;
                    }

                    if (NotEnoughProductQuantityAvailable(selectedProduct, requestedQuantity))
                    {
                        DisplayOutOfStockMessage(selectedProduct);
                        continue;
                    }

                    if (requestedQuantity > 0)
                    {
                        userBalance = UpdateUserBalance(userBalance, selectedProduct, requestedQuantity);
                        UpdateProductQuantity(selectedProduct, requestedQuantity);

                        DisplayReceiptMessage(userBalance, selectedProduct, requestedQuantity);
                    }
                    else
                    {
                        DisplayPurchaseCancelledMessage();
                    }
                }
                else if (itemSelected == EXIT)
                {
                    currentUser.Balance = userBalance;
                    exitRequested = true;
                }
                else
                {
                    DisplayInvalidProductSelectedMessage();
                }
            }
        }

        private static bool SelectedItemIsProduct(int itemSelected)
        {
            return itemSelected >= 0 && itemSelected < EXIT;
        }

        private static void UpdateProductQuantity(Product selectedProduct, int requestedQuantity)
        {
            selectedProduct.Quantity = selectedProduct.Quantity - requestedQuantity;
        }

        private static double UpdateUserBalance(double userBalance, Product selectedProduct, int requestedQuantity)
        {
            userBalance = userBalance - selectedProduct.Price * requestedQuantity;
            return userBalance;
        }

        private static void DisplayReceiptMessage(double balance, Product selectedProduct, int requestedQuantity)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("You bought " + requestedQuantity + " " + selectedProduct.Name);
            Console.WriteLine("Your new balance is " + balance.ToString("C"));
            Console.ResetColor();
        }

        private static void DisplayInvalidProductSelectedMessage()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine("Invalid Product ID selected.");
            Console.ResetColor();
        }
        private static void DisplayPurchaseCancelledMessage()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine("Purchase cancelled");
            Console.ResetColor();
        }

        private static void DisplayOutOfStockMessage(Product product)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("Sorry, " + product.Name + " is out of stock");
            Console.ResetColor();
        }

        private static bool NotEnoughProductQuantityAvailable(Product product, int quantity)
        {
            return product.Quantity <= quantity;
        }

        private static void DisplayNotEnoughMoneyMessage()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("You do not have enough money to buy that.");
            Console.ResetColor();
        }

        private static bool UserDoesNotHaveEnoughMoneyForPurchase(Product product, double balance, int quantity)
        {
            return balance - product.Price * quantity < 0;
        }

        private static void DisplayPurchaseSummary(Product product, double balance)
        {
            Console.WriteLine();
            Console.WriteLine("You want to buy: " + product.Name);
            Console.WriteLine("Your balance is " + balance.ToString("C"));
        }

        private static int PromptForPurchaseQuantity()
        {
            Console.WriteLine("Enter amount to purchase:");
            string answer = Console.ReadLine();
            int quantity = Convert.ToInt32(answer);
            return quantity;
        }

        private static void DisplayInvalidPasswordMessage()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("You entered an invalid password.");
            Console.ResetColor();
        }

        private static void DisplayInvalidUserNameMessage()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("You entered an invalid user.");
            Console.ResetColor();
        }


        private static void DisplayWelcomeMessage()
        {
            Console.WriteLine("Welcome to TUSC");
            Console.WriteLine("---------------");
        }

        private static void PromptUserToExit()
        {
            Console.WriteLine();
            Console.WriteLine("Press Enter key to exit");
            Console.ReadLine();
        }

        private static void SaveChangedQuantities(List<Product> products)
        {
            string json2 = JsonConvert.SerializeObject(products, Formatting.Indented);
            File.WriteAllText(@"Data\Products.json", json2);
        }

        private static void SaveChangedBalances(List<User> users)
        {
            string json = JsonConvert.SerializeObject(users, Formatting.Indented);
            File.WriteAllText(@"Data\Users.json", json);
        }

        private static int PromptForPurchaseItem(List<Product> products)
        {
            ShowProductList(products);
            return PromptForSelectedProduct();
        }

        private static int PromptForSelectedProduct()
        {
            const int INDEX_MODIFIER = 1;
            Console.WriteLine("Enter a number:");
            string answer = Console.ReadLine();
            int num = Convert.ToInt32(answer);

            return num - INDEX_MODIFIER;
        }

        private static void ShowProductList(List<Product> products)
        {
            Console.WriteLine();
            Console.WriteLine("What would you like to buy?");
            for (int i = 0; i < products.Count; i++)
            {
                Product product = products[i];
                Console.WriteLine(i + 1 + ": " + product.Name + " (" + product.Price.ToString("C") + ")");
            }
            Console.WriteLine(products.Count + 1 + ": Exit");
        }

        private static User GetUser(List<User> users, UserCredentials userCredentials)
        {
            User matchedUser = new User();

            foreach (User user in users)
            {
                if (userCredentials.MatchUser(user))
                {
                    matchedUser = user;
                }
            }
            return matchedUser;
        }

        private static bool PasswordIsValid(List<User> users, UserCredentials userCredentials)
        {
            bool passwordIsValid = false;

            foreach (User user in users)
            {
                if (userCredentials.MatchUser(user))
                {
                    passwordIsValid = true;
                }
            }

            return passwordIsValid;
        }

        private static void DisplayBalance(double balance)
        {
            Console.WriteLine();
            Console.WriteLine("Your balance is " + balance.ToString("C"));
        }

        private static void DisplayLoginSuccessfulMessage(string name)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("Login successful! Welcome " + name + "!");
            Console.ResetColor();
        }

        private static string PromptForUserPassword()
        {
            Console.WriteLine("Enter Password:");
            string password = Console.ReadLine();
            return password;
        }

        private static bool UserNameIsValid(List<User> users, string name)
        {
            bool userIsValid = false;
            for (int i = 0; i < users.Count; i++)
            {
                User user = users[i];

                if (user.Name == name)
                {
                    userIsValid = true;
                }
            }

            return userIsValid;
        }

        private static string PromptForUserName()
        {
            Console.WriteLine();
            Console.WriteLine("Enter Username:");
            string name = Console.ReadLine();
            return name;
        }
    }
}
