using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;

namespace Decryptor_app
{
    public partial class Form1 : Form
    {
        
        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle; // 또는 FormBorderStyle.FixedDialog
            // 버튼의 Click 이벤트 핸들러를 설정합니다.
            Browse.Click += new EventHandler(Browse_Click);
            Decryption.Click += new EventHandler(Decryption_Click);
        }
        private void Browse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                label1.Text = dialog.SelectedPath;
            }
        }
        private void Decryption_Click(object sender, EventArgs e)
        {
            try
            {
                if (label1.Text == "Browse your encrypted file path...")
                {
                    MessageBox.Show("Select Decryption Folder path");
                }
                else if (!Directory.GetFiles(label1.Text, "*.!Locked", SearchOption.AllDirectories).Any() ||
                !File.Exists(Path.Combine(label1.Text, "key.bin")) ||
                !File.Exists(Path.Combine(label1.Text, "iv.bin")))
                {
                    MessageBox.Show("No files to decrypt");
                }
                else
                {
                    // Read encryption key and initialization vector
                    string directoryPath = label1.Text;
                    byte[] key = File.ReadAllBytes(Path.Combine(directoryPath, "key.bin"));
                    byte[] iv = File.ReadAllBytes(Path.Combine(directoryPath, "iv.bin"));

                    // Get all encrypted files in the directory
                    string[] filePaths = Directory.GetFiles(directoryPath, "*.!Locked", SearchOption.AllDirectories);

                    // Create an instance of the AES encryption algorithm
                    using (Aes aes = Aes.Create())
                    {
                        // Set encryption key and initialization vector
                        aes.Key = key;
                        aes.IV = iv;

                        // Delete !INFO!.txt file
                        string infoFilePath = Path.Combine(directoryPath, "!INFO!.txt");
                        if (File.Exists(infoFilePath))
                        {
                            File.Delete(infoFilePath);
                        }

                        // Decrypt all files in the directory
                        foreach (string encryptedFilePath in filePaths)
                        {
                            // Separate file name and extension
                            string fileName = Path.GetFileNameWithoutExtension(encryptedFilePath).Replace(".!Locked", "");
                            string fileExtension = Path.GetExtension(fileName);

                            // Create decrypted file path
                            string decryptedFilePath = Path.Combine(Path.GetDirectoryName(encryptedFilePath), fileName);

                            //Add string iv and key
                            string IVfilepath = Path.Combine(directoryPath, "iv.bin");
                            string Keyfilepath = Path.Combine(directoryPath, "key.bin");

                            // Create file streams
                            using (FileStream encryptedFileStream = new FileStream(encryptedFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                using (FileStream decryptedFileStream = new FileStream(decryptedFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                                {
                                    // Create decryption stream
                                    using (ICryptoTransform decryptor = aes.CreateDecryptor())
                                    {
                                        // Decrypt the file contents and write to the decrypted file stream
                                        using (CryptoStream cryptoStream = new CryptoStream(encryptedFileStream, decryptor, CryptoStreamMode.Read))
                                        {
                                            cryptoStream.CopyTo(decryptedFileStream);
                                        }
                                    }
                                }
                            }

                            // Delete the encrypted file
                            File.Delete(encryptedFilePath);
                            File.Delete(IVfilepath);
                            File.Delete(Keyfilepath);
                        }
                    }
                    MessageBox.Show("Decryption is completed!");
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Access is denied.");
            }
        }
    }
}
