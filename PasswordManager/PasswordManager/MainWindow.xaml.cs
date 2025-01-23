using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PasswordManager
{
    public partial class MainWindow : Window
    {
        private readonly ApiClient apiClient;

        public MainWindow()
        {
            InitializeComponent();
            apiClient = new ApiClient();
            CurrentKeyId = null;
        }

        private async void LoadPasswords_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var passwords = await apiClient.GetPasswordsAsync();
                PasswordsList.Items.Clear();

                foreach (var passwordEntry in passwords)
                {
                    try
                    {
                        // Ładujemy klucz AES na podstawie key_id
                        byte[] aesKey = KeyManager.LoadAesKey(passwordEntry.KeyId);

                        // Odszyfrowujemy hasło
                        string decryptedPassword = CryptoHelper.Decrypt(Convert.FromBase64String(passwordEntry.Password), aesKey);

                        // Dodajemy dane do listy
                        PasswordsList.Items.Add($"{passwordEntry.Site} | {passwordEntry.Username} | {decryptedPassword}");
                    }
                    catch (FileNotFoundException)
                    {
                        PasswordsList.Items.Add($"{passwordEntry.Site} | {passwordEntry.Username} | [Key Missing]");
                    }
                    catch (Exception ex)
                    {
                        PasswordsList.Items.Add($"{passwordEntry.Site} | {passwordEntry.Username} | [Decryption Error: {ex.Message}]");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading passwords: {ex.Message}");
            }
        }

        private async void AddPassword_Click(object sender, RoutedEventArgs e)
        {
            string site = SiteTextBox.Text;
            string username = UsernameTextBox.Text;
            string plainPassword = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(site) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(plainPassword))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            if (string.IsNullOrEmpty(CurrentKeyId))
            {
                MessageBox.Show("Please generate an AES key first.");
                return;
            }

            try
            {
                // Ładujemy klucz AES na podstawie bieżącego identyfikatora
                byte[] aesKey = KeyManager.LoadAesKey(CurrentKeyId);

                // Szyfrujemy hasło
                string encryptedPassword = Convert.ToBase64String(CryptoHelper.Encrypt(plainPassword, aesKey));

                // Wysyłamy zaszyfrowane dane i key_id do backendu
                string result = await apiClient.AddPassword(site, username, encryptedPassword, CurrentKeyId);
                MessageBox.Show(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding password: {ex.Message}");
            }
        }


        //private async void AddPassword_Click(object sender, RoutedEventArgs e)
        //{
        //    string site = SiteTextBox.Text;
        //    string username = UsernameTextBox.Text;
        //    string plainPassword = PasswordBox.Password;
        //    string keyType = (KeyTypeComboBox.SelectedItem as ComboBoxItem).Content.ToString();

        //    string keyFilePath = keyType == "AES" ? "aes_key.bin" : "rsa_public_key.xml";
        //    // Wczytaj klucz AES z lokalnego systemu
        //    if (!File.Exists(keyFilePath))
        //    {
        //        MessageBox.Show("AES key not found. Please generate a key first.");
        //        return;
        //    }

        //    byte[] aesKey = KeyManager.LoadAesKey(keyFilePath);

        //    // Wygeneruj unikalne ID klucza
        //    string keyId = Guid.NewGuid().ToString();
        //    File.WriteAllText($"keys/{keyId}.bin", Convert.ToBase64String(aesKey)); // Zapisz klucz lokalnie

        //    string encryptedPassword = keyType switch
        //    {
        //        "AES" => Convert.ToBase64String(CryptoHelper.Encrypt(plainPassword, aesKey)),
        //        //"RSA" => CryptoHelper.EncryptWithRsa(plainPassword, File.ReadAllText(keyFilePath)),
        //        _ => throw new NotSupportedException("Invalid key type")
        //    };

        //    // Wyślij zaszyfrowane dane i key_id do backendu
        //    string result = await apiClient.AddPassword(site, username, encryptedPassword, keyId);
        //    MessageBox.Show(result);
        //}

        private string CurrentKeyId; // Przechowuje identyfikator bieżącego klucza AES

        private void GenerateKey_Click(object sender, RoutedEventArgs e)
        {
            string keyType = (KeyTypeComboBox.SelectedItem as ComboBoxItem).Content.ToString();
            string keyFilePath = keyType == "AES" ? "aes_key.bin" : "rsa_key_pair.xml";
            if (keyType == "AES")
            {
                try
                {
                    // Generujemy nowy klucz AES i zapisujemy go w folderze "keys/"
                    CurrentKeyId = KeyManager.GenerateNewAesKey();
                    //MessageBox.Show($"New AES key generated with ID: {CurrentKeyId}");
                    CurrentKeyIdTextBox.Text = CurrentKeyId;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error generating key: {ex.Message}");
                }
            }
            else if (keyType == "RSA")
            {
                CurrentKeyId = KeyManager.GenerateRsaKeys();
            }
            MessageBox.Show($"{keyType} key saved to {keyFilePath} \nKey ID: {CurrentKeyId}");
        }


        private void GeneratePassword_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(PasswordLengthTextBox.Text, out int length);
            string allowedChars = "";

            if (IncludeLowercaseCheckBox.IsChecked == true) allowedChars += "abcdefghijklmnopqrstuvwxyz";
            if (IncludeUppercaseCheckBox.IsChecked == true) allowedChars += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (IncludeNumbersCheckBox.IsChecked == true) allowedChars += "0123456789";
            if (IncludeSpecialCheckBox.IsChecked == true) allowedChars += "!@#$%^&*()";

            if (string.IsNullOrEmpty(allowedChars))
            {
                MessageBox.Show("Please select at least one character type.");
                return;
            }

            PasswordBox.Password = PasswordGenerator.GeneratePassword(length, allowedChars);
            MessageBox.Show($"Generated password: {PasswordBox.Password}");
        }

        private void DecryptPassword_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }
    }
}
