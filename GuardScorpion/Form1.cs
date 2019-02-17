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

//Author: Sega Chief
//MateriaKeeper v1.1
//Release: 27/01/2019
//Description: Materia Equip-Stats Editor for FF7

namespace GuardScorpion
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Formatting
            this.Text = "MateriaKeeper";
            this.BackColor = Color.DarkCyan;
            this.Location = new Point(300, 300);
            this.MaximizeBox = false;

            // Attach the DataError event to the corresponding event handler.
            this.dataGridView1.DataError +=
                new DataGridViewDataErrorEventHandler(dataGridView1_DataError);

            this.dataGridView1.CellValidating +=
                new DataGridViewCellValidatingEventHandler(dataGridView1_CellValidating);

            // Create the ToolTip and associate with the Form container.
            ToolTip toolTip1 = new ToolTip();

            // Set up the delays for the ToolTip.
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            // Force the ToolTip text to be displayed whether or not the form is active.
            toolTip1.ShowAlways = true;

            // Set up the ToolTip text for the Button and Checkbox.
            toolTip1.SetToolTip(this.pictureBox2, "No stat modifiers; used by Stat Plus Materia, etc.");
            toolTip1.SetToolTip(this.pictureBox3, "Magic Materia: Tier II");
            toolTip1.SetToolTip(this.pictureBox13, "Magic Materia: Tier III");
            toolTip1.SetToolTip(this.pictureBox12, "Unused");
            toolTip1.SetToolTip(this.pictureBox11, "Unused");
            toolTip1.SetToolTip(this.pictureBox10, "Unused");
            toolTip1.SetToolTip(this.pictureBox9, "Cover/Throw");
            toolTip1.SetToolTip(this.pictureBox8, "Deathblow/Enemy Away/EXP Plus/Gil Plus");
            toolTip1.SetToolTip(this.pictureBox7, "Enemy Lure");
            toolTip1.SetToolTip(this.pictureBox6, "Unused");
            toolTip1.SetToolTip(this.pictureBox5, "Double-Cut/Pre-Emptive/Steal");
            toolTip1.SetToolTip(this.pictureBox14, "Magic Materia: Tier I");
            toolTip1.SetToolTip(this.pictureBox4, "Summon Materia: Tier I");
            toolTip1.SetToolTip(this.pictureBox21, "Summon Materia: Tier II");
            toolTip1.SetToolTip(this.pictureBox20, "Summon Materia: Tier III");
            toolTip1.SetToolTip(this.pictureBox15, "Summon Materia: Tier IV");
            toolTip1.SetToolTip(this.pictureBox16, "Summon Materia: KOTR");
            toolTip1.SetToolTip(this.pictureBox17, "Unused");
            toolTip1.SetToolTip(this.pictureBox18, "Unused");
            toolTip1.SetToolTip(this.pictureBox19, "Unused");
        }

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            MessageBox.Show("Invalid value; please enter a value of 0-255.");
            e.Cancel = true;
        }

        private void dataGridView1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            // Validate the CompanyName entry by disallowing empty strings.
            if (String.IsNullOrEmpty(e.FormattedValue.ToString()))
            {
                MessageBox.Show("Invalid value; please enter a value of 0-255.");
                e.Cancel = true;
            }
        }

        private void btnDefaultStats_Click(object sender, EventArgs e)
        {
            string fileName = lblFileName.Text;
            RevertMateriaEffects(fileName);
        }

        private void btnHexOut_Click(object sender, EventArgs e)
        {
            string fileName = lblFileName.Text;
            string dirName = AppDomain.CurrentDomain.BaseDirectory + "\\MateriaOffsets.txt";
            //string dirName = Path.GetDirectoryName(Application.ExecutablePath + "\\MateriaOffsets.txt");
            //string dirName = Path.GetDirectoryName(appPath);

            //dirName = dirName + @"\MatKeepHext.txt";

            try
            {
                // Delete the file if it exists.
                if (File.Exists(dirName))
                {
                    DialogResult result = MessageBox.Show(
                        "Overwrite existing file?", "Warning",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        MessageBox.Show("Delete path: test");
                        //File.Delete(dirName);
                    }
                    else if (result == DialogResult.No)
                    {
                        MessageBox.Show("No path: test");
                        //code for No
                    }
                }

                if (!File.Exists(dirName))
                {
                    try
                    {
                        using (BinaryWriter bw = new BinaryWriter(File.Open(fileName, FileMode.Open)))
                        {
                            //Grabs the ints of GridView and loads them into an Int Array

                            /*The materia equip effects use 1 signed byte for the value.
                              2nd byte depends on this first byte and will be 00 if the
                              byte was positive, or FF if it was negative.

                              The DataGridView is read into an Int array, and then converted
                              into a byte array for re-insertion to the files. There are 320
                              cells to be read and inserted.
                            */

                            int[] intValues = new int[320];
                            int r = 0;
                            int o = 0;
                            int c = 0;
                            int k = 0;

                            while (o != 320)
                            {
                                //Gets the current row + column cell value and then
                                //increments array index  and column for next step
                                intValues[o] = dataGridView1.Rows[r].Cells[c].Value == null ? -1 : Convert.ToInt32(dataGridView1.Rows[r].Cells[c].Value);

                                //Checks for a negative value, adds 255 if true
                                //This makes the entry valid for signed byte conversion later
                                if (intValues[o] < 0)
                                {
                                    intValues[o] = intValues[o] + 256;
                                }
                                o++; c++;

                                //Adjusts every other byte based on value of previous byte
                                //For positive/0 value, must be 0. For negative value, must be FF
                                k = intValues[o - 1];
                                if (k < 128)
                                {
                                    intValues[o] = 0;
                                }
                                else
                                {
                                    intValues[o] = 255;
                                }
                                o++; c++;

                                //If reached last column, then reset and increment Row
                                if (c > 15)
                                {
                                    c = 0;
                                    r++;
                                }
                            }

                            //Converts int array into byte array, then into a string array
                            byte[] array = intValues.Select(b => (byte)b).ToArray();
                            string convert = BitConverter.ToString(array).Replace("-", " ");

                            //This loop attaches the offset for each materia equip effect
                            //along with its hex values; this can be used either for reference
                            //or copy-pasted straight into a hex-edit document.
                            //For info on hexedit, check hextools here: http://forums.qhimm.com/index.php?topic=13574.0
                            File.AppendAllText(dirName, "PC Offsets: Materia-Equip Effects" + Environment.NewLine);
                            int start = 0;
                            int length = 48;
                            int offsetByte = 0x4FD8C8; //Starting offset, PC version
                            string offsetString = "";
                            string offsetPrint = "";
                            for (int i = 0; i != 20; i++)
                            {
                                //Used for last string, to prevent an error trying to read
                                //a space character that isn't at very end of string.
                                if(i == 19)
                                {
                                    length = 47;
                                }
                                offsetString = offsetByte.ToString("X"); //Prints as hex rather than int
                                offsetPrint = offsetString + " = " + convert.Substring(start, length) + Environment.NewLine;
                                File.AppendAllText(dirName, offsetPrint);
                                offsetByte = offsetByte + 16;
                                start = start + 48; //Counts the characters, including spaces
                            }

                            //PSX Version of the printout
                            File.AppendAllText(dirName, Environment.NewLine + "PSX Offsets: Materia-Equip Effects" + Environment.NewLine);
                            start = 0;
                            length = 48;
                            offsetByte = 0x4FD88; //Starting offset, PSX version ntsc/eu version
                            offsetString = "";
                            offsetPrint = "";
                            for (int i = 0; i != 20; i++)
                            {
                                //Used for last string, to prevent an error trying to read
                                //a space character that isn't at very end of string.
                                if (i == 19)
                                {
                                    length = 47;
                                }
                                offsetString = offsetByte.ToString("X"); //Prints as hex rather than int
                                offsetPrint = offsetString + " = " + convert.Substring(start, length) + Environment.NewLine;
                                File.AppendAllText(dirName, offsetPrint);
                                offsetByte = offsetByte + 16;
                                start = start + 48; //Counts the characters, including spaces
                            }
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Error: Please check values entered are bewteen -127 > 128");
                    }
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error: Please check a valid file was loaded.");
            }



        }

        private void btnUpdateExp_Click(object sender, EventArgs e)
        {
            string fileName = lblFileName.Text;
            MateriaEffects(fileName);
        }

        public void MateriaEffects(string fileName)
        {
            try
            {
                using (BinaryWriter bw = new BinaryWriter(File.Open(fileName, FileMode.Open)))
                {
                    //Grabs the ints of GridView and loads them into an Int Array

                    /*The materia equip effects use 1 signed byte for the value.
                      2nd byte depends on this first byte and will be 00 if the
                      byte was positive, or FF if it was negative.

                      The DataGridView is read into an Int array, and then converted
                      into a byte array for re-insertion to the files. There are 320
                      cells to be read and inserted.
                    */

                    int[] intValues = new int[320];
                    int r = 0;
                    int o = 0;
                    int c = 0;
                    int k = 0;

                    while (o != 320)
                    {
                        //Gets the current row + column cell value and then
                        //increments array index  and column for next step
                        intValues[o] = dataGridView1.Rows[r].Cells[c].Value == null ? -1 : Convert.ToInt32(dataGridView1.Rows[r].Cells[c].Value);

                        //Checks for a negative value, adds 255 if true
                        //This makes the entry valid for signed byte conversion later
                        if (intValues[o] < 0)
                        {
                            intValues[o] = intValues[o] + 256;
                        }
                        o++; c++;

                        //Adjusts every other byte based on value of previous byte
                        //For positive/0 value, must be 0. For negative value, must be FF
                        k = intValues[o - 1];
                        if (k < 128)
                        {
                            intValues[o] = 0;
                        }
                        else
                        {
                            intValues[o] = 255;
                        }
                        o++; c++;

                        //If reached last column, then reset and increment Row
                        if (c > 15)
                        {
                            c = 0;
                            r++;
                        }
                    }

                    //Casts the int array into a byte array
                    byte[] array = intValues.Select(b => (byte)b).ToArray();

                    if (fileName.Contains(".bin") || fileName.Contains(".img") || fileName.Contains(".iso"))
                    {
                        bw.BaseStream.Position = 0x4FD88; //Sets the offset
                        bw.Write(array, 0, array.Length); //Overwrites the target with byte array
                        MessageBox.Show("Materia Equip-Effects Patched - PSX");
                    }
                    else
                    {
                        bw.BaseStream.Position = 0x4FD8C8; //Sets the offset
                        bw.Write(array, 0, array.Length); //Overwrites the target with byte array
                        MessageBox.Show("Materia Equip-Effects Patched - EXE");
                    }
                }
            }
            catch
            {
                MessageBox.Show("Error: Please check values entered are bewteen -127 > 128");
            }
        }

        public void RevertMateriaEffects(string fileName)
        {
            try
            {
                using (BinaryWriter bw = new BinaryWriter(File.Open(fileName, FileMode.Open)))
                {
                    //Feeds an array of default values back into the .exe

                    //Casts the int array into a byte array
                    byte[] array = new byte[]
                    {   0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0xFE, 0xFF, 0xFF, 0xFF, 0x02, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFB, 0xFF, 0x05, 0x00,
                        0xFC, 0xFF, 0xFE, 0xFF, 0x04, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF6, 0xFF, 0x0A, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0xFE, 0xFF, 0x00, 0x00, 0x00, 0x00,
                        0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x01, 0x00, 0x01, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFE, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0xFF, 0xFF, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFE, 0xFF, 0x02, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFE, 0xFF, 0x02, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFB, 0xFF, 0x05, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF6, 0xFF, 0x0A, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF6, 0xFF, 0x0F, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF6, 0xFF, 0x14, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};

                    bw.BaseStream.Position = 0x4FD8C8; //Sets the offset
                    bw.Write(array, 0, array.Length); //Overwrites the target with byte array
                    MessageBox.Show("Materia Equip-Effects Reverted to Default");
                }
            }
            catch
            {
                MessageBox.Show("Error: Please check values entered are bewteen -127 > 128");
            }
        }

        private void btnLoadFile_Click(object sender, EventArgs e)
        {
            //Filters file to look for FF7.exe or ff7_en.exe, it can only
            //filter by extension however, so an additional check was added
            //below to confirm the file name.
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "ff7 files (ff7.exe)|ff7*.exe|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Title = "Open FF7.EXE or FF7_EN.EXE";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileCheck = openFileDialog1.FileName;

                //Enforces that opened file is ff7.exe or ff_en.exe
                if (fileCheck.Contains("ff7.exe") || fileCheck.Contains("ff7_en.exe"))
                {
                    try
                    {
                        //Sets the columns, only the actual values are made visible
                        //for modification as the 2nd 'modifier' byte will be adjusted
                        //automatically by the tool based on the 1st byte defined by
                        //the user.
                        BinaryReader br = new BinaryReader(new MemoryStream(File.ReadAllBytes(openFileDialog1.FileName)));

                        DataTable dataTable = new DataTable();
                        dataTable.Columns.Add("STR", typeof(sbyte));
                        dataTable.Columns.Add("STR2", typeof(sbyte));
                        dataTable.Columns.Add("VIT", typeof(sbyte));
                        dataTable.Columns.Add("VIT2", typeof(sbyte));
                        dataTable.Columns.Add("MAG", typeof(sbyte));
                        dataTable.Columns.Add("MAG2", typeof(sbyte));
                        dataTable.Columns.Add("SPR", typeof(sbyte));
                        dataTable.Columns.Add("SPR2", typeof(sbyte));
                        dataTable.Columns.Add("DEX", typeof(sbyte));
                        dataTable.Columns.Add("DEX2", typeof(sbyte));
                        dataTable.Columns.Add("LCK", typeof(sbyte));
                        dataTable.Columns.Add("LCK2", typeof(sbyte));
                        dataTable.Columns.Add("HP%", typeof(sbyte));
                        dataTable.Columns.Add("HP%2", typeof(sbyte));
                        dataTable.Columns.Add("MP%", typeof(sbyte));
                        dataTable.Columns.Add("MP%2", typeof(sbyte));

                        //Reads file from this offset then casts it to a signedbyte array
                        br.BaseStream.Position = 0x4FD8C8;
                        byte[] byteArray = br.ReadBytes(0x140);
                        sbyte[] array = Array.ConvertAll(byteArray, b => (sbyte)b);

                        //Adds the rows/cell values from the byte array
                        int i = 20;
                        int j = 0;
                        while (i != 0)
                        {
                            dataTable.Rows.Add(
                                array[j], array[j + 1], array[j + 2], array[j + 3],
                                array[j + 4], array[j + 5], array[j + 6], array[j + 7],
                                array[j + 8], array[j + 9], array[j + 10], array[j + 11],
                                array[j + 12], array[j + 13], array[j + 14], array[j + 15]);
                            i = i - 1;
                            j = j + 16;
                        }
                        dataGridView1.DataSource = dataTable;

                        //Hides the extra byte modifier used for +/- values
                        //These are adjusted by tool automatically when patching.
                        dataGridView1.Columns[1].Visible = false;
                        dataGridView1.Columns[3].Visible = false;
                        dataGridView1.Columns[5].Visible = false;
                        dataGridView1.Columns[7].Visible = false;
                        dataGridView1.Columns[9].Visible = false;
                        dataGridView1.Columns[11].Visible = false;
                        dataGridView1.Columns[13].Visible = false;
                        dataGridView1.Columns[15].Visible = false;

                        foreach (DataGridViewColumn column in dataGridView1.Columns)
                        {
                            column.SortMode = DataGridViewColumnSortMode.NotSortable;
                        }

                        //Sets column width for better visibility
                        int r = 0;
                        while (r < 16)
                        {
                            DataGridViewColumn column = dataGridView1.Columns[r];
                            column.Width = 60;
                            r++;
                            r++;
                        }

                        //Sets the row value identifier                 
                        foreach (DataGridViewRow s in dataGridView1.Rows)
                        {
                            dataGridView1.Rows[s.Index].HeaderCell.Value =
                                                ("0" + s.Index).ToString();
                        }
                        //Tiers are shown in hexadecimal within the WallMarket tool so row-names
                        //are converted to 'hex' (cosmetically) here for ease of reference between tools.
                        dataGridView1.Rows[10].HeaderCell.Value = ("0A").ToString();
                        dataGridView1.Rows[11].HeaderCell.Value = ("0B").ToString();
                        dataGridView1.Rows[12].HeaderCell.Value = ("0C").ToString();
                        dataGridView1.Rows[13].HeaderCell.Value = ("0D").ToString();
                        dataGridView1.Rows[14].HeaderCell.Value = ("0E").ToString();
                        dataGridView1.Rows[15].HeaderCell.Value = ("0F").ToString();
                        dataGridView1.Rows[16].HeaderCell.Value = ("10").ToString();
                        dataGridView1.Rows[17].HeaderCell.Value = ("11").ToString();
                        dataGridView1.Rows[18].HeaderCell.Value = ("12").ToString();
                        dataGridView1.Rows[19].HeaderCell.Value = ("13").ToString();
                        dataGridView1.RowHeadersWidth = 60;

                        //Target .EXE File Path
                        lblFileName.Text = openFileDialog1.FileName;
                    }
                    catch
                    {
                        MessageBox.Show("An error has occurred; please check that a valid ff7.exe was loaded", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Invalid File; please select a ff7.exe or ff7_en.exe", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void loadPSX_Click_1(object sender, EventArgs e)
        {
            //Filters file to look for a FF7 ISO, however there can be great variety
            //in the actual name of the disc itself or the extension. So for validation,
            //the tool checks 
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            //openFileDialog1.Filter = "ff7 iso (.bin)|*.bin|All Files (*.*)|*.*";

            //openFileDialog1.Filter = "ISO|*.iso|BIN|*.bin|IMG|*.img";

            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Title = "Open an FF7 ISO/.BIN";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileCheck = openFileDialog1.FileName;

                //Enforces that opened file is ff7.exe or ff_en.exe
                if (fileCheck.Contains(".bin") || fileCheck.Contains(".iso") || fileCheck.Contains(".img"))
                {
                    try
                    {
                        //Sets the columns, only the actual values are made visible
                        //for modification as the 2nd 'modifier' byte will be adjusted
                        //automatically by the tool based on the 1st byte defined by
                        //the user.
                        BinaryReader br = new BinaryReader(new MemoryStream(File.ReadAllBytes(openFileDialog1.FileName)));

                        DataTable dataTable = new DataTable();
                        dataTable.Columns.Add("STR", typeof(sbyte));
                        dataTable.Columns.Add("STR2", typeof(sbyte));
                        dataTable.Columns.Add("VIT", typeof(sbyte));
                        dataTable.Columns.Add("VIT2", typeof(sbyte));
                        dataTable.Columns.Add("MAG", typeof(sbyte));
                        dataTable.Columns.Add("MAG2", typeof(sbyte));
                        dataTable.Columns.Add("SPR", typeof(sbyte));
                        dataTable.Columns.Add("SPR2", typeof(sbyte));
                        dataTable.Columns.Add("DEX", typeof(sbyte));
                        dataTable.Columns.Add("DEX2", typeof(sbyte));
                        dataTable.Columns.Add("LCK", typeof(sbyte));
                        dataTable.Columns.Add("LCK2", typeof(sbyte));
                        dataTable.Columns.Add("HP%", typeof(sbyte));
                        dataTable.Columns.Add("HP%2", typeof(sbyte));
                        dataTable.Columns.Add("MP%", typeof(sbyte));
                        dataTable.Columns.Add("MP%2", typeof(sbyte));

                        //Reads file from this offset then casts it to a signedbyte array
                        br.BaseStream.Position = 0x4FD88;
                        byte[] byteArray = br.ReadBytes(0x140);
                        sbyte[] array = Array.ConvertAll(byteArray, b => (sbyte)b);

                        //Adds the rows/cell values from the byte array
                        int i = 20;
                        int j = 0;
                        while (i != 0)
                        {
                            dataTable.Rows.Add(
                                array[j], array[j + 1], array[j + 2], array[j + 3],
                                array[j + 4], array[j + 5], array[j + 6], array[j + 7],
                                array[j + 8], array[j + 9], array[j + 10], array[j + 11],
                                array[j + 12], array[j + 13], array[j + 14], array[j + 15]);
                            i = i - 1;
                            j = j + 16;
                        }
                        dataGridView1.DataSource = dataTable;

                        //Hides the extra byte modifier used for +/- values
                        //These are adjusted by tool automatically when patching.
                        dataGridView1.Columns[1].Visible = false;
                        dataGridView1.Columns[3].Visible = false;
                        dataGridView1.Columns[5].Visible = false;
                        dataGridView1.Columns[7].Visible = false;
                        dataGridView1.Columns[9].Visible = false;
                        dataGridView1.Columns[11].Visible = false;
                        dataGridView1.Columns[13].Visible = false;
                        dataGridView1.Columns[15].Visible = false;

                        foreach (DataGridViewColumn column in dataGridView1.Columns)
                        {
                            column.SortMode = DataGridViewColumnSortMode.NotSortable;
                        }

                        //Sets column width for better visibility
                        int r = 0;
                        while (r < 16)
                        {
                            DataGridViewColumn column = dataGridView1.Columns[r];
                            column.Width = 60;
                            r++;
                            r++;
                        }

                        //Sets the row value identifier                 
                        foreach (DataGridViewRow s in dataGridView1.Rows)
                        {
                            dataGridView1.Rows[s.Index].HeaderCell.Value =
                                                ("0" + s.Index).ToString();
                        }
                        //Tiers are shown in hexadecimal within the WallMarket tool so row-names
                        //are converted to 'hex' (cosmetically) here for ease of reference between tools.
                        dataGridView1.Rows[10].HeaderCell.Value = ("0A").ToString();
                        dataGridView1.Rows[11].HeaderCell.Value = ("0B").ToString();
                        dataGridView1.Rows[12].HeaderCell.Value = ("0C").ToString();
                        dataGridView1.Rows[13].HeaderCell.Value = ("0D").ToString();
                        dataGridView1.Rows[14].HeaderCell.Value = ("0E").ToString();
                        dataGridView1.Rows[15].HeaderCell.Value = ("0F").ToString();
                        dataGridView1.Rows[16].HeaderCell.Value = ("10").ToString();
                        dataGridView1.Rows[17].HeaderCell.Value = ("11").ToString();
                        dataGridView1.Rows[18].HeaderCell.Value = ("12").ToString();
                        dataGridView1.Rows[19].HeaderCell.Value = ("13").ToString();
                        dataGridView1.RowHeadersWidth = 60;

                        //Target File Path
                        lblFileName.Text = openFileDialog1.FileName;
                    }
                    catch
                    {
                        MessageBox.Show("An error has occurred; please check that a valid ff7.exe was loaded", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Invalid File; please select a FF7 IMG, BIN, or ISO", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
