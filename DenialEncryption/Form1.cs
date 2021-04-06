using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace DenialEncryption
{
    public partial class Form1 : Form
    {
        int Messages = 0;
        int Times;
        public Form1(int times)
        {
            Times = times;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            save.newData(textBox1.Text, textBox2.Text);
            textBox1.Text = "";
            textBox2.Text = "";
            Messages++;

            if (Messages == Times)
            {
                this.Close();
                save.write();
            }                    
        }
    }

    public class Messages
    {
        [JsonProperty("Text")]
        public string Text { get; set; }
        [JsonProperty("ListMessages")]
        public List<Message> ListMessages { get; set; }
    }
    class Wrapper
    {
        public Messages Messages { get; set; }
    }
    public class Message
    {
        [JsonProperty("Text")]
        public string Text { get; set; }
        [JsonProperty("Key")]
        public string Key { get; set; }
    }

    public class save
    {
        static Message _messages = new Message();
        static Messages Messages = new Messages();
        public static void saveText(string key, string text)
        {
            _messages = new Message
            {
                Key = key,
                Text = text
            };
            if(Messages.ListMessages == null)
                Messages.ListMessages = new List<Message>();
            Messages.ListMessages.Add(_messages);

        }
        public static void newData(string key, string text)
        {
            var newDa = new Message
            {
                Key = key,
                Text = EncryptHelper.Encrypt(text, key)
            };

            saveText(newDa.Key, newDa.Text);
        }

        public static void write()
        {
            string text = "";
            foreach (var mes in Messages.ListMessages)
            {
                text = string.Concat(text, mes.Text);
            }
            MD5 md5 = new MD5CryptoServiceProvider();
            md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(text));
            byte[] result = md5.Hash;

            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                strBuilder.Append(result[i].ToString("x2"));
            }

            text = strBuilder.ToString();

            Messages.Text = text;
            var json = System.Text.Json.JsonSerializer.Serialize(Messages);
            var form = new SecretMessage(text);
            form.ShowDialog();
            File.WriteAllText(string.Concat(System.Environment.CurrentDirectory, "\\messages.txt"), json);
        }
    }

    public static class EncryptHelper
    {
        public static string Encrypt(string clearText, string key)
        {
            string EncryptionKey = key;
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
        public static string Decrypt(string cipherText, string key)
        {
            string EncryptionKey = key;
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
    }
}
