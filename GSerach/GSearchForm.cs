using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GSerach
{
    public partial class GSearchForm : Form
    {
        public GSearchForm()
        {
            InitializeComponent();

            dt = new DataTable();
            dt.Columns.Add(new DataColumn("文件名", typeof(string)));
            dt.Columns.Add(new DataColumn("行列", typeof(string)));
            dt.Columns.Add(new DataColumn("匹配内容", typeof(string)));
            dt.Columns.Add(new DataColumn("创建时间", typeof(DateTime)));
            dt.Columns.Add(new DataColumn("最后修改时间", typeof(DateTime)));
            dt.Columns.Add(new DataColumn("路径", typeof(string)));
        }

        private string lastFolder = "";
        private DataTable dt;
        private string selectPath;
        private string fullPath;

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Select Folder To Search";
            if (string.IsNullOrEmpty(lastFolder))
                dialog.RootFolder = Environment.SpecialFolder.Desktop;
            else
                dialog.SelectedPath = lastFolder;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                path.Text = dialog.SelectedPath;
                lastFolder = dialog.SelectedPath;
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(path.Text))
            {
                MessageBox.Show("Not Exist Directory!");
                return;
            }
            lastFolder = path.Text;

            if (string.IsNullOrEmpty(searchContent.Text.Trim()))
            {
                MessageBox.Show("Search Content is NULL!");
                return;
            }

            Dictionary<string, int> searchEtds = new Dictionary<string, int>();
            if(lua.Checked)
                searchEtds.Add(".lua", 1);
            if(cs.Checked)
                searchEtds.Add(".cs", 1);
            if(txt.Checked)
                searchEtds.Add(".txt", 1);

            if (!string.IsNullOrEmpty(other.Text))
            {
                string[] sps = other.Text.Split('|');
                foreach (var sp in sps)
                {
                    if(!searchEtds.ContainsKey(sp.ToLower()))
                        searchEtds.Add(sp.ToLower(), 1);
                }
            }
            
            dt.Clear();

            string[] paths = Directory.GetFiles(lastFolder, "*.*", SearchOption.AllDirectories);
            foreach (var p in paths)
            {
                string etd = Path.GetExtension(p).ToLower();
                if (searchEtds.Count == 0 || searchEtds.ContainsKey(etd))
                {
                    string[] fileTxt = File.ReadAllLines(p);
                    FileInfo file = new FileInfo(p);
                    string name = Path.GetFileName(p);

                    for (int i = 0; i < fileTxt.Length; i++)
                    {
                        MatchCollection matchs = Regex.Matches(fileTxt[i], searchContent.Text);
                        foreach (Match match in matchs)
                        {
                            DataRow row = dt.NewRow();
                            row[0] = name;
                            row[1] = string.Format("{0}:{1}", i + 1, match.Index + 1);
                            row[2] = match.Value;
                            row[3] = file.CreationTime;
                            row[4] = file.LastWriteTime;
                            row[5] = p.Replace("\\", "/");
                            dt.Rows.Add(row);
                        }
                    }
                }
            }

            dataGridView1.DataSource = dt;
            dataGridView1.Refresh();
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right && e.ColumnIndex > -1 && e.RowIndex > -1)  //点击的是鼠标右键，并且不是表头
            {
                selectPath = this.dataGridView1.Rows[e.RowIndex].Cells[5].Value.ToString();
                var name = this.dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                fullPath = selectPath;
                selectPath = selectPath.Replace(name, ""); 
                //右键选中单元格
                this.dataGridView1.Rows[e.RowIndex].Selected = true;
                this.contextMenuStrip1.Show(MousePosition.X, MousePosition.Y); //MousePosition.X, MousePosition.Y 是为了让菜单在所选行的位置显示
            }
        }

        private void tool1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(fullPath);
        }

        private void tool2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(selectPath);
        }

        private void dataGridView1_RowPosPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            Rectangle rectangle = new Rectangle(e.RowBounds.Location.X,
                                                e.RowBounds.Location.Y,
                                                dgv.RowHeadersWidth - 4,
                                                e.RowBounds.Height);


            TextRenderer.DrawText(e.Graphics, (e.RowIndex + 1).ToString(),
                                    dgv.RowHeadersDefaultCellStyle.Font,
                                    rectangle,
                                    dgv.RowHeadersDefaultCellStyle.ForeColor,
                                    TextFormatFlags.VerticalCenter | TextFormatFlags.Right);
        }

        private void other_MouseEnter(object sender, EventArgs e)
        {
            toolTip1.Show("自定义后缀，多个‘|’分隔，如 .txt|.csv", other);
        }
    }
}
