﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TextCompiler
{
    public partial class Form1 : Form
    {
        const string filter = "Text files(*.txt)|*.txt|All files(*.*)|*.*";
        public int countOpenedFiles = 0;

        ToolStripLabel dateLabel;
        ToolStripLabel infoLabel;
        Timer timer;

        public List<File> files = new List<File>();
        public File file;
        public Form1()
        {
            InitializeComponent();
            openFileDialog1.Filter = filter;
            saveFileDialog1.Filter = filter;
            tabControl1.SelectedIndexChanged += TabControl1_SelectedIndexChanged;
            this.MainMenuStrip = menuStrip1; 
            Settings.LoadSettings();
            if(Settings.language != "Русский")
                Settings.UpdateLanguage(this);
            infoLabel = new ToolStripLabel();
            infoLabel.Text = (Settings.language == "Русский") ? "Текущие дата и время: " : "Current time and date: ";
            dateLabel = new ToolStripLabel();
            dateLabel.Text = DateTime.Now.ToString();

            statusStrip1.Items.Add(infoLabel);
            statusStrip1.Items.Add(dateLabel);

            timer = new Timer() { Interval = 1000 };
            timer.Tick += timer_Tick;
            timer.Start();

            KeyDown += Keyboard;
        }
        private void Keyboard(object sender, KeyEventArgs e) //обработчик нажатия клавиш
        {
            if (e.KeyCode == Keys.F1)
            {
                try
                {
                    Help help = new Help();
                    help.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else if (e.Control && e.KeyCode == Keys.N)
            {
                try
                {
                    Create();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else if (e.Control && e.KeyCode == Keys.O)
            {
                try
                {
                    OpenFile();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else if (e.Control && e.KeyCode == Keys.S)
            {
                try
                {
                    Save();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else if (e.Control && e.KeyCode == Keys.W)
            {
                try
                {
                    if (tabControl1.TabPages.Count == 0)
                        this.Close();
                    else
                        Exit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        void timer_Tick(object sender, EventArgs e)
        {
            dateLabel.Text = DateTime.Now.ToString();
        }

        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (countOpenedFiles != 0)
            {
                int index = 0;
                if (tabControl1.TabPages.Count > 1)
                    index = tabControl1.SelectedIndex;
                file = files[index];
            }
        }

        public void ShowEditor(Panel panel, RichTextBox richTextBox)
        {
            panel.Paint += (sender, e) => LineNumberPanel_Paint(sender, e, richTextBox);

            richTextBox.TextChanged += (s, e) => panel.Invalidate();
            richTextBox.VScroll += (s, e) => panel.Invalidate();
            richTextBox.SelectionChanged += (s, e) => panel.Invalidate();
            //richTextBox.TextChanged += (sender, e) => RichTextBox_TextChanged(sender, e, richTextBox);
        }
        private void RichTextBox_TextChanged(object sender, EventArgs e)
        {
            int index = tabControl1.SelectedIndex;
            if (file.isUndoRedo)
                return;
            var rtb = sender as RichTextBox;
            if (rtb == null) return;

            // Сохраняем текущий текст в буфер
            files[index].bufferOfData.Add(rtb.Text);
            files[index].currentVersion = files[index].bufferOfData.Count - 1;
            //files[index].bufferOfData.Add(sender.ToString());
        }

        //private void RichTextBox_TextChanged(object sender, EventArgs e, RichTextBox richTextBox)
        //{
        //    Analyzer analyzer = new Analyzer(richTextBox);
        //    analyzer.HighlightSyntax();
        //}
        public void InitializeCompiler(File file, string title, string fileText)
        {
            TabPage myTabPage = new TabPage(title);
            tabControl1.TabPages.Add(myTabPage);
            SplitContainer splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                IsSplitterFixed = true,  
            };
            int requiredWidth = TextRenderer.MeasureText("100", Settings.font).Width;
            splitContainer.SplitterDistance = requiredWidth;
            splitContainer.SplitterWidth = 1;
            splitContainer.FixedPanel = FixedPanel.Panel1;
            myTabPage.Controls.Add(splitContainer);
            Panel panel = new Panel
            {
                Dock = DockStyle.Left,
                BackColor = SystemColors.Window
            };
            splitContainer.Panel1.Controls.Add(panel);
            RichTextBox richTextBox1 = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None
            };
            splitContainer.Panel2.Controls.Add(richTextBox1);
            richTextBox1.Text = fileText;
            ShowEditor(panel, richTextBox1);
            file.textBox = richTextBox1;
            tabControl1.SelectedTab = myTabPage;
            this.file = file;

            Analyzer analyzer = new Analyzer(richTextBox1);
            analyzer.HighlightSyntax();

            Settings.UpdateFont(tabControl1);
        }

        private void LineNumberPanel_Paint(object sender, PaintEventArgs e, RichTextBox richTextBox)
        {
            int firstIndex = richTextBox.GetCharIndexFromPosition(new Point(0, 0));
            int firstLine = richTextBox.GetLineFromCharIndex(firstIndex);

            int lineHeight = TextRenderer.MeasureText("0", richTextBox.Font).Height;
            int y = 0;

            for (int i = firstLine; y < richTextBox.Height; i++)
            {
                y = (i - firstLine) * lineHeight;
                e.Graphics.DrawString((i + 1).ToString(), richTextBox.Font, Brushes.Black, new PointF(5, y));
            }
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        public void OpenFile()
        {
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;
            // получаем выбранный файл
            string title = openFileDialog1.SafeFileName;
            string fileName = openFileDialog1.FileName;
            // читаем файл в строку
            string fileText = System.IO.File.ReadAllText(fileName);
            File file = new File(countOpenedFiles, title, fileName, fileText);
            files.Add(file);
            if (countOpenedFiles == 0) tabControl1.TabPages.Clear();
            InitializeCompiler(file, title, fileText);
            countOpenedFiles++;
        }

        public void Undo()
        {
            if (file.currentVersion > 0)
            {
                // Сохраняем текущую позицию курсора и выделение
                int selectionStart = file.textBox.SelectionStart;
                int selectionLength = file.textBox.SelectionLength;

                // Переходим к предыдущей версии
                file.currentVersion--;
                file.isUndoRedo = true;
                file.textBox.Text = file.bufferOfData[file.currentVersion];
                file.isUndoRedo = false;

                // Если текст стал короче, корректируем позицию курсора
                if (selectionStart > file.textBox.Text.Length)
                    selectionStart = file.textBox.Text.Length;
                file.textBox.SelectionStart = selectionStart;
                file.textBox.SelectionLength = selectionLength;
            }
        }

        public void Redo()
        {
            if (file.currentVersion < file.bufferOfData.Count - 1)
            {
                // Сохраняем текущую позицию курсора и выделение
                int selectionStart = file.textBox.SelectionStart;
                int selectionLength = file.textBox.SelectionLength;

                file.currentVersion++;
                file.isUndoRedo = true;
                file.textBox.Text = file.bufferOfData[file.currentVersion];
                file.isUndoRedo = false;

                // Корректировка позиции курсора, если новый текст короче
                if (selectionStart > file.textBox.Text.Length)
                    selectionStart = file.textBox.Text.Length;
                file.textBox.SelectionStart = selectionStart;
                file.textBox.SelectionLength = selectionLength;
            }
        }


        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (file != null)
                Undo();
        }


        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            if(file!= null)
                Redo();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Save();
        }

        public void Save()
        {
            if(tabControl1.TabPages.Count > 0) 
                System.IO.File.WriteAllText(file.path, file.textBox.Text);
        }

        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count > 0)
            {
                string fileName = file.fileName;
                saveFileDialog1.FileName = fileName;
                if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
                    return;
                // сохраняем текст в файл
                System.IO.File.WriteAllText(saveFileDialog1.FileName, file.textBox.Text);
            }
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Create();
        }
        public void Create()
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Создание";
                saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                // Отключаем предупреждение о перезаписи
                saveFileDialog.OverwritePrompt = false;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;
                    // Записываем текст в файл (перезапись, если файл уже существует)
                    System.IO.File.WriteAllText(filePath, "");
                    string fileName = System.IO.Path.GetFileName(saveFileDialog.FileName);
                    string path = saveFileDialog.FileName;
                    File file = new File(countOpenedFiles, fileName, path, null);
                    files.Add(file);
                    if (countOpenedFiles == 0) tabControl1.TabPages.Clear();
                    InitializeCompiler(file, fileName, null);
                    countOpenedFiles++;
                }
            }
        }

        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Create();
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            file?.textBox.Copy();
        }

        private void копироватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            file?.textBox.Copy();
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            file?.textBox.Paste();
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            file?.textBox.Cut();
        }

        private void отменитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            file?.textBox.Undo();
        }

        private void повторитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            file?.textBox.Redo();
        }

        private void вырезатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            file?.textBox.Cut();
        }

        private void вставитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            file?.textBox.Paste();
        }

        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(file != null)
                file.textBox.SelectedText = "";
        }

        private void выделитьВсеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            file?.textBox.SelectAll();
        }
        public void Exit()
        {
            if (System.IO.File.ReadAllText(file.path) != file.textBox.Text)
            {
                DialogResult result = MessageBox.Show($"Сохранить изменения в файл {file.fileName}?", "Закрытие файла", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    System.IO.File.WriteAllText(file.path, file.textBox.Text);
                }
            }
            countOpenedFiles--;
            files.Remove(file);
            tabControl1.TabPages.Remove(tabControl1.SelectedTab);
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(tabControl1.TabPages.Count == 0)
                this.Close();
            else 
                Exit();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach(var tabPages in tabControl1.TabPages)
            {
                Exit();
            }
        }

        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SettingsForm settingsForm = new SettingsForm();
                settingsForm.ShowDialog();
                if (settingsForm.update == true)
                {

                    Settings.UpdateLanguage(this);
                    infoLabel.Text = (Settings.language == "Русский") ? "Текущие дата и время:" : "Current time and date";
                }
                Settings.UpdateFont(tabControl1);
                Settings.UpdateLineNumberPanelWidth(tabControl1);
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при изменении настроек: {ex.Message}");
            }
        }

        private void tabControl1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void tabControl1_DragDrop(object sender, DragEventArgs e)
        {
            string[] filesForDrop = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var fileForDrop in filesForDrop)
            {
                if (Path.GetExtension(fileForDrop) == ".txt")
                {
                    string path = fileForDrop.ToString();
                    string title = Path.GetFileName(path);
                    string fileText = System.IO.File.ReadAllText(path);
                    File file = new File(countOpenedFiles, title, path, fileText);
                    files.Add(file);
                    if (countOpenedFiles == 0) tabControl1.TabPages.Clear();
                    InitializeCompiler(file, title, fileText);
                    countOpenedFiles++;
                }
            }
        }

        private void вызовСправкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Help help = new Help();
            help.ShowDialog();
        }

        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            Help help = new Help();
            help.ShowDialog();
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }

        private void toolStripButton11_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }
    }
}
