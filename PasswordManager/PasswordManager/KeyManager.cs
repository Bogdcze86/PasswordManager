using System;
using System.IO;
using System.Security.Cryptography;

public static class KeyManager
{
    private static readonly string KeysDirectory = "keys";

    static KeyManager()
    {
        // Tworzymy folder "keys", jeśli jeszcze nie istnieje
        if (!Directory.Exists(KeysDirectory))
        {
            Directory.CreateDirectory(KeysDirectory);
        }
    }

    // Funkcja generowania nowego klucza AES i zapisywania w folderze "keys"
    public static string GenerateNewAesKey()
    {
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.GenerateKey();

        // Generujemy unikalny identyfikator dla klucza
        string keyId = Guid.NewGuid().ToString();
        string keyPath = Path.Combine(KeysDirectory, $"{keyId}.bin");

        // Zapisujemy klucz w pliku
        File.WriteAllBytes(keyPath, aes.Key);

        return keyId; // Zwracamy identyfikator klucza
    }

    // Funkcja ładowania klucza AES na podstawie key_id
    public static byte[] LoadAesKey(string keyId)
    {
        string keyPath = Path.Combine(KeysDirectory, $"{keyId}.bin");

        if (!File.Exists(keyPath))
        {
            throw new FileNotFoundException($"Key with ID {keyId} not found.");
        }

        return File.ReadAllBytes(keyPath);
    }

    public static string GenerateRsaKeys()
    {
        string keyId = Guid.NewGuid().ToString();
        string keyPath = Path.Combine(KeysDirectory, $"{keyId}.bin");
        return keyId;
        //TODO
        /*Rozwiń tę metodę by dodawać również klucze RSA i dodaj analogicznie jak AES ładowanie:
KeyManager.cs:
public static string GenerateRsaKeys()
{
    string keyId = Guid.NewGuid().ToString();
    string keyPath = Path.Combine(KeysDirectory, $"{keyId}.bin");
    return keyId;
}
MainWindow.xaml.cs:
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
}*/
    }
}
