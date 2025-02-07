using AI_Buddy.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows.Forms;

namespace AI_Buddy.Forms
{
    public partial class PropertiesFrm : Form
    {
        AIProperties _aiProperties = new AIProperties();

        public PropertiesFrm()
        {
            InitializeComponent();

            propertiesAIPrompt.PropertyValueChanged += PropertiesAIPrompt_PropertyValueChanged;
        }

        private void PropertiesAIPrompt_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            btnSave.Enabled = true; // Enable the button when a property is modified
        }

        private void PropertiesFrm_Load(object sender, EventArgs e)
        {
            // read file settings and populate settings class, if no file display defaults
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), _aiProperties.SettingsFilename);

            if (File.Exists(filePath))
            {
                _aiProperties = LoadFromJson<AIProperties>(filePath);
            }
            
            this.propertiesAIPrompt.SelectedObject = _aiProperties;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), _defaultFilename);
            SaveToJson(_aiProperties, filePath);

            MessageBox.Show("File has been saved successfully.", "Save Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }


        public void SaveToJson<T>(T obj, string filePath)
        {
            string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public T LoadFromJson<T>(string filePath)
        {
            if (!File.Exists(filePath)) return default;
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
