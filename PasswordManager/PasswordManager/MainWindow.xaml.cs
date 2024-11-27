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
        }

        private async void AddPassword_Click(object sender, RoutedEventArgs e)
        {
            string site = SiteTextBox.Text;
            string username = UsernameTextBox.Text;
            string plainPassword = PasswordBox.Password;
            string keyType = (KeyTypeComboBox.SelectedItem as ComboBoxItem).Content.ToString();

            string keyFilePath = keyType == "AES" ? "aes_key.bin" : "rsa_public_key.xml";
            if (!File.Exists(keyFilePath))
            {
                MessageBox.Show($"{keyType} key not found. Please generate a key first.");
                return;
            }

            string encryptedPassword = keyType switch
            {
                "AES" => Convert.ToBase64String(CryptoHelper.Encrypt(plainPassword, KeyManager.LoadAesKey(keyFilePath))),
                "RSA" => CryptoHelper.EncryptWithRsa(plainPassword, File.ReadAllText(keyFilePath)),
                _ => throw new NotSupportedException("Invalid key type")
            };

            string result = await apiClient.AddPassword(site, username, encryptedPassword);
            MessageBox.Show(result);
        }

        private void GenerateKey_Click(object sender, RoutedEventArgs e)
        {
            string keyType = (KeyTypeComboBox.SelectedItem as ComboBoxItem).Content.ToString();
            string keyFilePath = keyType == "AES" ? "aes_key.bin" : "rsa_key_pair.xml";

            if (keyType == "AES")
            {
                KeyManager.GenerateAesKey(keyFilePath);
            }
            else if (keyType == "RSA")
            {
                KeyManager.GenerateRsaKeys(keyFilePath);
            }

            MessageBox.Show($"{keyType} key saved to {keyFilePath}");
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
