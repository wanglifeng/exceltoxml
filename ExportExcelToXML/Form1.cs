using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml;

namespace ExportExcelToXML
{
    public partial class Form1 : Form
    {
        private String excelPath = string.Empty;
        private Dictionary<int, String> mapping = new Dictionary<int, String>();
        private ExcelWorksheet sheet = null;
        private ExcelPackage package = null;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnChooseFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dg = new OpenFileDialog() { DefaultExt = "xls,xlxs", CheckFileExists = true })
            {
                if (dg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (!String.IsNullOrEmpty(dg.FileName))
                    {
                        excelPath = dg.FileName;
                        package = new ExcelPackage(new FileInfo(excelPath));
                        //cmbxSheets.DataSource = package.Workbook.Worksheets;
                        List<String> lists = new List<string>();
                        lists.AddRange(package.Workbook.Worksheets.Select(t => t.Name).ToList());
                        //sheet = package.Workbook.Worksheets["Sheet1"];
                        //InitFieldsMapping(sheet);
                        cmbxSheets.DataSource = lists;
                    }
                }
            }
        }

        private void InitFieldsMapping(ExcelWorksheet sheet)
        {
            mapping.Clear();
            flwPanel.Controls.Clear();
            //create a row
            if (sheet.Dimension.End.Row > sheet.Dimension.Start.Row)
            {
                for (int i = sheet.Dimension.Start.Column; i <= sheet.Dimension.End.Column; i++)
                {
                    var range = sheet.Cells[1, i];
                    this.flwPanel.Controls.Add(new Label() { Text = range.Text, Width = flwPanel.Width / 2 - 10 });
                    this.flwPanel.Controls.Add(new TextBox() { Width = flwPanel.Width / 2 - 10 });
                    mapping.Add(i, range.Text);
                }
            }

        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            Dictionary<String, String> nameMapping = new Dictionary<string, string>();
            for (int i = 0; i < flwPanel.Controls.Count; i = i + 2)
            {
                nameMapping.Add(flwPanel.Controls[i].Text.Replace(" ", "_"), (flwPanel.Controls[i + 1] as TextBox).Text);
            }
            var keys = mapping.Keys.ToList();
            foreach (int key in keys)
            {
                var value = mapping[key].Replace(" ", "_");
                mapping[key] = nameMapping[value];
            }

            SaveFileDialog dialog = new SaveFileDialog() { DefaultExt = "xml", Filter = "XML file(*.xml)|*.xml", OverwritePrompt = true, AddExtension = true };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                XmlWriter writer = new XmlTextWriter(dialog.FileName, Encoding.UTF8);
                writer.WriteStartDocument();
                writer.WriteStartElement("Records");
                for (int rowIndex = sheet.Dimension.Start.Row + 1; rowIndex <= sheet.Dimension.End.Row; rowIndex++)
                {
                    writer.WriteStartElement("Record");

                    foreach (KeyValuePair<int, string> pair in mapping)
                    {
                        writer.WriteStartElement(pair.Value);
                        writer.WriteString(sheet.Cells[rowIndex, pair.Key].Text);
                        writer.WriteEndElement();

                        if (pair.Value == "FirstName")
                        {
                            writer.WriteStartElement("LastName");
                            writer.WriteString(" ");
                            writer.WriteEndElement();
                            //writer.WriteElementString("LastName", " ");
                        }
                    }

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.Flush();
                writer.Close();
                MessageBox.Show("Success");
            }

        }

        private void cmbxSheets_SelectedValueChanged(object sender, EventArgs e)
        {
            sheet = package.Workbook.Worksheets[cmbxSheets.SelectedValue.ToString()];
            InitFieldsMapping(sheet);
        }
    }
}
