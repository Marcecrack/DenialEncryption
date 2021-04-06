using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace DenialEncryption
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using StreamReader r = new StreamReader(string.Concat(System.Environment.CurrentDirectory, "\\messages.txt"));
            string json = r.ReadToEnd();
            var temp = JObject.Parse(json);

            var message = GetMessages(temp);
            var test = message.ListMessages.Where(m => m.Key == textBox2.Text).FirstOrDefault().Text;
            if (!string.IsNullOrEmpty(test))
            {
                var result = EncryptHelper.Decrypt(test, textBox2.Text);
                MessageBox.Show(result);
            }
            else
                MessageBox.Show("Wow...you were tricked");
        }
        public Messages GetMessages(JObject json)
        {
            var message = new Messages();
            var actual = false;
            foreach (var mes in json)
            {
                if (actual)
                {
                    var test = mes.Value.ToList();
                    message.ListMessages = new List<Message>();
                    foreach (var prueba in test)
                    {
                        message.ListMessages.Add(JsonConvert.DeserializeObject<Message>(prueba.ToString()));
                    }
                    actual = false;
                }
                if (string.Equals(mes.Value.ToString(), textBox1.Text))
                {
                    message.Text = mes.Value.ToString();
                    actual = true;
                }
            }
            return message;
        }
    }
}
