using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
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

            // Initialize placeholder logic
            FilterTextBox.TextChanged += FilterTextBox_TextChanged;
            FilterTextBox.GotFocus += FilterTextBox_GotFocus;
            FilterTextBox.LostFocus += FilterTextBox_LostFocus;
            UsernameTextBox.TextChanged += UsernameTextBox_TextChanged;
            UsernameTextBox.GotFocus += UsernameTextBox_GotFocus;
            UsernameTextBox.LostFocus += UsernameTextBox_LostFocus;
            SiteTextBox.TextChanged += SiteTextBox_TextChanged;
            SiteTextBox.GotFocus += SiteTextBox_GotFocus;
            SiteTextBox.LostFocus += SiteTextBox_LostFocus;

            // Set initial placeholder visibility
            UpdatePlaceholderVisibility();
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholderVisibility();
        }

        private void FilterTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility();
        }

        private void FilterTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility();
        }
        private void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholderVisibility();
        }

        private void UsernameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility();
        }

        private void UsernameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility();
        }

        private void SiteTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholderVisibility();
        }

        private void SiteTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility();
        }

        private void SiteTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility();
        }

        private void UpdatePlaceholderVisibility()
        {
            FilterPlaceholder.Visibility = string.IsNullOrEmpty(FilterTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            SiteTextPlaceholder.Visibility = string.IsNullOrEmpty(SiteTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            UsernameTextPlaceholder.Visibility = string.IsNullOrEmpty(UsernameTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            LoadPasswords_Click(sender, e);
        }

        private async void LoadPasswords_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var passwords = await apiClient.GetPasswordsAsync();
                PasswordsList.Items.Clear();

                // Apply filtering
                string filterText = FilterTextBox.Text.ToLower();
                var filteredPasswords = passwords
                    .Where(p => p.Site.ToLower().Contains(filterText) || p.Username.ToLower().Contains(filterText))
                    .ToList();

                // Apply sorting
                var sortOption = (SortComboBox.SelectedItem as ComboBoxItem).Content.ToString();
                switch (sortOption)
                {
                    case "Sort by Site (A-Z)":
                        filteredPasswords = filteredPasswords.OrderBy(p => p.Site).ToList();
                        break;
                    case "Sort by Site (Z-A)":
                        filteredPasswords = filteredPasswords.OrderByDescending(p => p.Site).ToList();
                        break;
                    case "Sort by Username (A-Z)":
                        filteredPasswords = filteredPasswords.OrderBy(p => p.Username).ToList();
                        break;
                    case "Sort by Username (Z-A)":
                        filteredPasswords = filteredPasswords.OrderByDescending(p => p.Username).ToList();
                        break;
                }

                // Display the sorted and filtered passwords
                foreach (var passwordEntry in filteredPasswords)
                {
                    try
                    {
                        string decryptedPassword;
                        if (passwordEntry.KeyId.StartsWith("RSA_")) // Check if it's an RSA key
                        {
                            byte[] privateKey = KeyManager.LoadRsaPrivateKey(passwordEntry.KeyId);
                            decryptedPassword = CryptoHelper.DecryptWithRsa(passwordEntry.Password, privateKey);
                        }
                        else if (passwordEntry.KeyId.StartsWith("AES_")) // Check if it's an AES key
                        {
                            byte[] aesKey = KeyManager.LoadAesKey(passwordEntry.KeyId);
                            decryptedPassword = CryptoHelper.Decrypt(Convert.FromBase64String(passwordEntry.Password), aesKey);
                        }
                        else
                        {
                            throw new NotSupportedException($"Unsupported key type for key ID: {passwordEntry.KeyId}");
                        }

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
            string keyType = (KeyTypeComboBox.SelectedItem as ComboBoxItem).Content.ToString();

            if (string.IsNullOrWhiteSpace(site) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(plainPassword))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            if (string.IsNullOrEmpty(CurrentKeyId))
            {
                MessageBox.Show("Please generate a key first.");
                return;
            }

            try
            {
                string encryptedPassword;
                if (keyType == "AES")
                {
                    byte[] aesKey = KeyManager.LoadAesKey(CurrentKeyId);
                    encryptedPassword = Convert.ToBase64String(CryptoHelper.Encrypt(plainPassword, aesKey));
                }
                else if (keyType == "RSA")
                {
                    byte[] publicKey = KeyManager.LoadRsaPublicKey(CurrentKeyId);
                    encryptedPassword = CryptoHelper.EncryptWithRsa(plainPassword, publicKey);
                }
                else
                {
                    throw new NotSupportedException("Invalid key type");
                }

                string result = await apiClient.AddPassword(site, username, encryptedPassword, CurrentKeyId);
                MessageBox.Show(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding password: {ex.Message}");
            }
        }

        private string CurrentKeyId; // Przechowuje identyfikator bieżącego klucza

        private void GenerateKey_Click(object sender, RoutedEventArgs e)
        {
            string keyType = (KeyTypeComboBox.SelectedItem as ComboBoxItem).Content.ToString();

            try
            {
                if (keyType == "AES")
                {
                    // Generate a new AES key
                    CurrentKeyId = KeyManager.GenerateNewAesKey();
                    MessageBox.Show($"New AES key generated with ID: {CurrentKeyId}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (keyType == "RSA")
                {
                    // Generate a new RSA key pair
                    CurrentKeyId = KeyManager.GenerateRsaKeys();
                    MessageBox.Show($"New RSA key pair generated with ID: {CurrentKeyId}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Invalid key type selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Update the UI with the current key ID
                CurrentKeyIdTextBox.Text = CurrentKeyId;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating key: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordsList.SelectedItem != null)
            {
                // Pobierz zawartość wybranego elementu
                string selectedItem = PasswordsList.SelectedItem.ToString();

                // Podziel tekst na części za pomocą separatora "|"
                string[] parts = selectedItem.Split('|');

                if (parts.Length > 0)
                {
                    // Usuń białe znaki i wybierz ostatnią część
                    string password = parts[^1].Trim();

                    // Skopiuj wybrany fragment do schowka
                    Clipboard.SetText(password);
                }
                else
                {
                    MessageBox.Show("Nie można znaleźć fragmentu tekstu.");
                }
            }
            else
            {
                MessageBox.Show("Nie wybrano żadnego elementu!");
            }
        }

        private void LoadKey_Click(object sender, RoutedEventArgs e)
        {
            string keyType = (KeyTypeComboBox.SelectedItem as ComboBoxItem).Content.ToString();

            // Create OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = $"Select {keyType} Key File",
                Filter = keyType == "AES"
                    ? "Binary Files (*.bin)|*.bin|All Files (*.*)|*.*"
                    : "PEM Files (*.pem)|*.pem|All Files (*.*)|*.*"
            };

            // Show OpenFileDialog and check if user selected a file
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                try
                {
                    if (keyType == "AES")
                    {
                        // Read the AES key file
                        byte[] aesKey = File.ReadAllBytes(filePath);

                        // Handle the loaded AES key (e.g., store it in memory)
                        CurrentKeyId = Path.GetFileNameWithoutExtension(filePath);
                        CurrentKeyIdTextBox.Text = CurrentKeyId;
                        MessageBox.Show("AES Key loaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (keyType == "RSA")
                    {
                        // Read the RSA key file
                        byte[] rsaKey = File.ReadAllBytes(filePath);

                        // Extract the key ID from the file name (assuming the file name is in the format "RSA_{keyId}_public.pem" or "RSA_{keyId}_private.pem")
                        string fileName = Path.GetFileNameWithoutExtension(filePath);
                        if (fileName.EndsWith("_public") || fileName.EndsWith("_private"))
                        {
                            CurrentKeyId = fileName.Substring(0, fileName.LastIndexOf('_')); // Extract the key ID without the suffix
                        }
                        else
                        {
                            CurrentKeyId = fileName; // Fallback if the file name doesn't follow the expected format
                        }

                        CurrentKeyIdTextBox.Text = CurrentKeyId;
                        MessageBox.Show("RSA Key loaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Invalid key type selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading {keyType} Key: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}