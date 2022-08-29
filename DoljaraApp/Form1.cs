using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Forms;
using WebSocket4Net;

namespace _DoljaraApp
{
    public partial class Form1 : Form
    {
        Settings settings;

        Dictionary<string, Producer> producers;

        PictureBox[] bottomPScouted;
        PictureBox[] bottomPUnselected;

        PictureBox[] leftPScouted;
        PictureBox[] leftPUnselected;

        PictureBox[] topPScouted;
        PictureBox[] topPUnselected;

        PictureBox[] rightPScouted;
        PictureBox[] rightPUnselected;

        Audition audition;
        string roomId = "";
        int rev = -1;

        private static HttpClient httpClient = new HttpClient();
        private static WebSocket ws;
        private static SoundPlayer soundEffect = new SoundPlayer(Properties.Resources.free_sound12);

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string settingsJsonText = File.ReadAllText("settings.json");

            settings = JsonSerializer.Deserialize<Settings>(settingsJsonText);

            timer1.Interval = settings.reloadSec * 1000;

            ws = new WebSocket(settings.WSserverURL);
            ws.Closed += Ws_Closed;
            ws.MessageReceived += Ws_MessageReceived;
            ws.Open();

            bottomPScouted = new PictureBox[] {
                pictureBox1, pictureBox2, pictureBox3, pictureBox4,
                pictureBox5, pictureBox6, pictureBox7, pictureBox8, pictureBox9 };

            bottomPUnselected = new PictureBox[] {
                pictureBox37, pictureBox38, pictureBox39, pictureBox40, pictureBox41,
                pictureBox42, pictureBox43, pictureBox44, pictureBox45, pictureBox46,
                pictureBox47, pictureBox48, pictureBox49, pictureBox50 };

            leftPScouted = new PictureBox[] {
                pictureBox10, pictureBox11, pictureBox12, pictureBox13,
                pictureBox14, pictureBox15, pictureBox16, pictureBox17, pictureBox18 };

            leftPUnselected = new PictureBox[] {
                pictureBox51, pictureBox52, pictureBox53, pictureBox54, pictureBox55,
                pictureBox56, pictureBox57, pictureBox58, pictureBox59, pictureBox60,
                pictureBox61, pictureBox62, pictureBox63, pictureBox64 };

            topPScouted = new PictureBox[] {
                pictureBox19, pictureBox20, pictureBox21, pictureBox22,
                pictureBox23, pictureBox24, pictureBox25, pictureBox26, pictureBox27 };

            topPUnselected = new PictureBox[] {
                pictureBox65, pictureBox66, pictureBox67, pictureBox68, pictureBox69,
                pictureBox70, pictureBox71, pictureBox72, pictureBox73, pictureBox74,
                pictureBox75, pictureBox76, pictureBox77, pictureBox78 };

            rightPScouted = new PictureBox[] {
                pictureBox28, pictureBox29, pictureBox30, pictureBox31,
                pictureBox32, pictureBox33, pictureBox34, pictureBox35, pictureBox36 };

            rightPUnselected = new PictureBox[] {
                pictureBox79, pictureBox80, pictureBox81, pictureBox82, pictureBox83,
                pictureBox84, pictureBox85, pictureBox86, pictureBox87, pictureBox88,
                pictureBox89, pictureBox90, pictureBox91, pictureBox92 };

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                switch (args[1].ToUpper())
                {
                    case "W":
                        ChangeSide("西");
                        break;
                    case "S":
                        ChangeSide("南");
                        break;
                    case "N":
                        ChangeSide("北");
                        break;
                }
            }

            timer1_Tick(null, null);
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.Size.Width < this.Size.Height)
            {
                int newWidth = 180;
                int marginLeft = (panel9.Size.Width - newWidth) / 2;

                panel9.Size = new Size(newWidth, panel9.Size.Height);
                panel9.Location = new Point(panel9.Location.X + marginLeft, panel9.Location.Y);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            MethodInvoker method = () =>
            {
                timer1.Enabled = false;

                GetJson();

                producers = new Dictionary<string, Producer>();
                producers.Add("東", audition.eastP);
                producers.Add("西", audition.westP);
                producers.Add("南", audition.southP);
                producers.Add("北", audition.northP);

                if (!自分のアイドルを非表示ToolStripMenuItem.Checked)
                {
                    for (int i = 0; i < producers[label3.Text].scoutedIdols.Count; i++)
                    {
                        string imgPath = "image/bottom_" + producers[label3.Text].scoutedIdols[i].imagePath;

                        if (bottomPScouted[i].ImageLocation == null ||
                            !bottomPScouted[i].ImageLocation.Equals(imgPath))
                        {
                            bottomPScouted[i].ImageLocation = imgPath;
                            bottomPScouted[i].BackColor = Color.White;
                        }

                        bottomPScouted[i].Visible = true;
                    }
                }
                else
                {
                    for (int i = 0; i < producers[label3.Text].scoutedIdols.Count; i++)
                    {
                        bottomPScouted[i].ImageLocation = null;
                        bottomPScouted[i].BackColor = Color.Moccasin;
                        bottomPScouted[i].Visible = true;
                    }
                }

                // 手牌の数より多い表示枠を非表示にする
                for (int i = producers[label3.Text].scoutedIdols.Count; i < 9; i++)
                {
                    bottomPScouted[i].Visible = false;
                }

                if (producers[label4.Text].debut || 観客モードToolStripMenuItem.Checked)
                {
                    for (int i = 0; i < producers[label4.Text].scoutedIdols.Count; i++)
                    {
                        string imgPath = "image/left_" + producers[label4.Text].scoutedIdols[i].imagePath;

                        if (leftPScouted[i].ImageLocation == null ||
                        !leftPScouted[i].ImageLocation.Equals(imgPath))
                        {
                            leftPScouted[i].ImageLocation = imgPath;

                            leftPScouted[i].BackColor = Color.White;
                        }

                        leftPScouted[i].Visible = true;
                    }
                }
                else
                {
                    for (int i = 0; i < producers[label4.Text].scoutedIdols.Count; i++)
                    {
                        leftPScouted[i].ImageLocation = null;

                        leftPScouted[i].BackColor = Color.Moccasin;

                        leftPScouted[i].Visible = true;
                    }
                }

                for (int i = producers[label4.Text].scoutedIdols.Count; i < 9; i++)
                {
                    leftPScouted[i].Visible = false;
                }

                if (producers[label5.Text].debut || 観客モードToolStripMenuItem.Checked)
                {
                    for (int i = 0; i < producers[label5.Text].scoutedIdols.Count; i++)
                    {
                        string imgPath = "image/top_" + producers[label5.Text].scoutedIdols[i].imagePath;

                        if (topPScouted[i].ImageLocation == null ||
                        !topPScouted[i].ImageLocation.Equals(imgPath))
                        {
                            topPScouted[i].ImageLocation = imgPath;

                            topPScouted[i].BackColor = Color.White;
                        }

                        topPScouted[i].Visible = true;
                    }
                }
                else
                {
                    for (int i = 0; i < producers[label5.Text].scoutedIdols.Count; i++)
                    {
                        topPScouted[i].ImageLocation = null;

                        topPScouted[i].BackColor = Color.Moccasin;

                        topPScouted[i].Visible = true;
                    }
                }

                for (int i = producers[label5.Text].scoutedIdols.Count; i < 9; i++)
                {
                    topPScouted[i].Visible = false;
                }

                if (producers[label6.Text].debut || 観客モードToolStripMenuItem.Checked)
                {
                    for (int i = 0; i < producers[label6.Text].scoutedIdols.Count; i++)
                    {
                        string imgPath = "image/right_" + producers[label6.Text].scoutedIdols[i].imagePath;

                        if (rightPScouted[i].ImageLocation == null ||
                        !rightPScouted[i].ImageLocation.Equals(imgPath))
                        {
                            rightPScouted[i].ImageLocation = imgPath;

                            rightPScouted[i].BackColor = Color.White;
                        }

                        rightPScouted[i].Visible = true;
                    }
                }
                else
                {
                    for (int i = 0; i < producers[label6.Text].scoutedIdols.Count; i++)
                    {
                        rightPScouted[i].ImageLocation = null;

                        rightPScouted[i].BackColor = Color.Moccasin;

                        rightPScouted[i].Visible = true;
                    }
                }

                for (int i = producers[label6.Text].scoutedIdols.Count; i < 9; i++)
                {
                    rightPScouted[i].Visible = false;
                }

                for (int i = 0; i < producers[label3.Text].unselectedIdols.Count; i++)
                {
                    string imgPath = producers[label3.Text].unselectedIdols[i].imagePath;

                    if (bottomPUnselected[i].ImageLocation == null ||
                        !bottomPUnselected[i].ImageLocation.Equals(imgPath))
                    {
                        bottomPUnselected[i].ImageLocation = "image/bottom_" + imgPath;

                        bottomPUnselected[i].BackColor = Color.White;

                        if (producers[label3.Text].unselectedIdols[i].isReached)
                        {
                            bottomPUnselected[i].BackColor = Color.LightCoral;
                        }

                        if (producers[label3.Text].unselectedIdols[i].isDrawn)
                        {
                            bottomPUnselected[i].BackColor = Color.Gray;
                        }
                    }
                }
                for (int i = producers[label3.Text].unselectedIdols.Count; i < 14; i++)
                {
                    bottomPUnselected[i].ImageLocation = null;

                    bottomPUnselected[i].BackColor = Color.Transparent;
                }

                for (int i = 0; i < producers[label4.Text].unselectedIdols.Count; i++)
                {
                    string imgPath = producers[label4.Text].unselectedIdols[i].imagePath;

                    if (leftPUnselected[i].ImageLocation == null ||
                        !leftPUnselected[i].ImageLocation.Equals(imgPath))
                    {
                        leftPUnselected[i].ImageLocation = "image/left_" + imgPath;

                        leftPUnselected[i].BackColor = Color.White;

                        if (producers[label4.Text].unselectedIdols[i].isReached)
                        {
                            leftPUnselected[i].BackColor = Color.LightCoral;
                        }

                        if (producers[label4.Text].unselectedIdols[i].isDrawn)
                        {
                            leftPUnselected[i].BackColor = Color.Gray;
                        }
                    }
                }
                for (int i = producers[label4.Text].unselectedIdols.Count; i < 14; i++)
                {
                    leftPUnselected[i].ImageLocation = null;

                    leftPUnselected[i].BackColor = Color.Transparent;
                }

                for (int i = 0; i < producers[label5.Text].unselectedIdols.Count; i++)
                {
                    string imgPath = producers[label5.Text].unselectedIdols[i].imagePath;

                    if (topPUnselected[i].ImageLocation == null ||
                        !topPUnselected[i].ImageLocation.Equals(imgPath))
                    {
                        topPUnselected[i].ImageLocation = "image/top_" + imgPath;

                        topPUnselected[i].BackColor = Color.White;

                        if (producers[label5.Text].unselectedIdols[i].isReached)
                        {
                            topPUnselected[i].BackColor = Color.LightCoral;
                        }

                        if (producers[label5.Text].unselectedIdols[i].isDrawn)
                        {
                            topPUnselected[i].BackColor = Color.Gray;
                        }
                    }
                }
                for (int i = producers[label5.Text].unselectedIdols.Count; i < 14; i++)
                {
                    topPUnselected[i].ImageLocation = null;

                    topPUnselected[i].BackColor = Color.Transparent;
                }

                for (int i = 0; i < producers[label6.Text].unselectedIdols.Count; i++)
                {
                    string imgPath = producers[label6.Text].unselectedIdols[i].imagePath;

                    if (rightPUnselected[i].ImageLocation == null ||
                        !rightPUnselected[i].ImageLocation.Equals(imgPath))
                    {
                        rightPUnselected[i].ImageLocation = "image/right_" + imgPath;

                        rightPUnselected[i].BackColor = Color.White;

                        if (producers[label6.Text].unselectedIdols[i].isReached)
                        {
                            rightPUnselected[i].BackColor = Color.LightCoral;
                        }

                        if (producers[label6.Text].unselectedIdols[i].isDrawn)
                        {
                            rightPUnselected[i].BackColor = Color.Gray;
                        }
                    }
                }
                for (int i = producers[label6.Text].unselectedIdols.Count; i < 14; i++)
                {
                    rightPUnselected[i].ImageLocation = null;

                    rightPUnselected[i].BackColor = Color.Transparent;
                }

                label2.Text = audition.applicants.Count.ToString();

                timer1.Enabled = true;
            };

            if (InvokeRequired)
            {
                Invoke(method);
            }
            else
            {
                method();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // スカウト
            timer1.Enabled = false;

            if (producers[label3.Text].scoutedIdols.Count < 9 &&
                audition.applicants.Count > 0)
            {
                producers[label3.Text].scoutedIdols.Add(audition.applicants[0]);
                audition.applicants.RemoveAt(0);

                PutJson();
            }
            
            if (ws.State != WebSocketState.Open)
            {
                timer1_Tick(null, null);
            }
        }

        private void scoutedIdol_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;

            Producer p = producers[label3.Text];
            int index = Array.IndexOf(bottomPScouted, sender);

            if (p.unselectedIdols.Count < 14)
            {
                p.unselectedIdols.Add(p.scoutedIdols[index]);
                p.scoutedIdols.RemoveAt(index);

                p.scoutedIdols = p.scoutedIdols.OrderBy(idol => idol.id).ToList();

                PutJson();
            }

            if (ws.State != WebSocketState.Open)
            {
                timer1_Tick(null, null);
            }
        }

        private void unselectedIdol_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;

            Producer p = producers[label3.Text];

            if (p.scoutedIdols.Count < 8)
            {
                int index;

                index = Array.IndexOf(bottomPUnselected, sender);
                if (index != -1 && !bottomPUnselected[index].BackColor.Equals(Color.Transparent) &&
                    !producers[label3.Text].unselectedIdols[index].isDrawn)
                {
                    Idol clone = new Idol();

                    clone.id = producers[label3.Text].unselectedIdols[index].id;
                    clone.imagePath = producers[label3.Text].unselectedIdols[index].imagePath;
                    clone.isReached = false;
                    clone.isDrawn = false;

                    p.scoutedIdols.Add(clone);
                    p.scoutedIdols = p.scoutedIdols.OrderBy(idol => idol.id).ToList();

                    producers[label3.Text].unselectedIdols[index].isDrawn = true;

                    PutJson();
                }

                index = Array.IndexOf(leftPUnselected, sender);
                if (index != -1 && !leftPUnselected[index].BackColor.Equals(Color.Transparent) &&
                    !producers[label4.Text].unselectedIdols[index].isDrawn)
                {
                    Idol clone = new Idol();

                    clone.id = producers[label4.Text].unselectedIdols[index].id;
                    clone.imagePath = producers[label4.Text].unselectedIdols[index].imagePath;
                    clone.isReached = false;
                    clone.isDrawn = false;

                    p.scoutedIdols.Add(clone);
                    p.scoutedIdols = p.scoutedIdols.OrderBy(idol => idol.id).ToList();

                    producers[label4.Text].unselectedIdols[index].isDrawn = true;

                    PutJson();
                }

                index = Array.IndexOf(topPUnselected, sender);
                if (index != -1 && !topPUnselected[index].BackColor.Equals(Color.Transparent) &&
                    !producers[label5.Text].unselectedIdols[index].isDrawn)
                {
                    Idol clone = new Idol();

                    clone.id = producers[label5.Text].unselectedIdols[index].id;
                    clone.imagePath = producers[label5.Text].unselectedIdols[index].imagePath;
                    clone.isReached = false;
                    clone.isDrawn = false;

                    p.scoutedIdols.Add(clone);
                    p.scoutedIdols = p.scoutedIdols.OrderBy(idol => idol.id).ToList();

                    producers[label5.Text].unselectedIdols[index].isDrawn = true;

                    PutJson();
                }

                index = Array.IndexOf(rightPUnselected, sender);
                if (index != -1 && !rightPUnselected[index].BackColor.Equals(Color.Transparent) &&
                    !producers[label6.Text].unselectedIdols[index].isDrawn)
                {
                    Idol clone = new Idol();

                    clone.id = producers[label6.Text].unselectedIdols[index].id;
                    clone.imagePath = producers[label6.Text].unselectedIdols[index].imagePath;
                    clone.isReached = false;
                    clone.isDrawn = false;

                    p.scoutedIdols.Add(clone);
                    p.scoutedIdols = p.scoutedIdols.OrderBy(idol => idol.id).ToList();

                    producers[label6.Text].unselectedIdols[index].isDrawn = true;

                    PutJson();
                }
            }
            else
            {
                int index = Array.IndexOf(bottomPUnselected, sender);

                if (index != -1 && !bottomPUnselected[index].BackColor.Equals(Color.Transparent))
                {
                    p.unselectedIdols[index].isReached = !p.unselectedIdols[index].isReached;

                    PutJson();
                }
            }

            if (ws.State != WebSocketState.Open)
            {
                timer1_Tick(null, null);
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            // 新しいオーディション
            timer1.Enabled = false;

            DialogResult result = MessageBox.Show(
                "現在のオーディションを終了してもよろしいですか？",
                "新しいオーディション",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.OK)
            {
                audition = new Audition();

                audition.eastP = new Producer();
                audition.eastP.debut = false;
                audition.eastP.scoutedIdols = new List<Idol>();
                audition.eastP.unselectedIdols = new List<Idol>();

                audition.westP = new Producer();
                audition.westP.debut = false;
                audition.westP.scoutedIdols = new List<Idol>();
                audition.westP.unselectedIdols = new List<Idol>();

                audition.southP = new Producer();
                audition.southP.debut = false;
                audition.southP.scoutedIdols = new List<Idol>();
                audition.southP.unselectedIdols = new List<Idol>();

                audition.northP = new Producer();
                audition.northP.debut = false;
                audition.northP.scoutedIdols = new List<Idol>();
                audition.northP.unselectedIdols = new List<Idol>();

                audition.applicants = settings.allIdols.OrderBy(idol => Guid.NewGuid()).ToList();

                for (int i = 0; i < 8; i++)
                {
                    audition.eastP.scoutedIdols.Add(audition.applicants[0]);
                    audition.applicants.RemoveAt(0);

                    audition.westP.scoutedIdols.Add(audition.applicants[0]);
                    audition.applicants.RemoveAt(0);

                    audition.southP.scoutedIdols.Add(audition.applicants[0]);
                    audition.applicants.RemoveAt(0);

                    audition.northP.scoutedIdols.Add(audition.applicants[0]);
                    audition.applicants.RemoveAt(0);
                }

                audition.eastP.scoutedIdols = audition.eastP.scoutedIdols.OrderBy(idol => idol.id).ToList();
                audition.westP.scoutedIdols = audition.westP.scoutedIdols.OrderBy(idol => idol.id).ToList();
                audition.southP.scoutedIdols = audition.southP.scoutedIdols.OrderBy(idol => idol.id).ToList();
                audition.northP.scoutedIdols = audition.northP.scoutedIdols.OrderBy(idol => idol.id).ToList();

                PutJson();
            }

            if (ws.State != WebSocketState.Open)
            {
                timer1_Tick(null, null);
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            // デビュー
            timer1.Enabled = false;

            DialogResult result = MessageBox.Show(
                "他のプロデューサーにアイドルを公開します。よろしいですか？",
                "デビュー",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.OK)
            {
                switch (label3.Text)
                {
                    case "東":
                        audition.eastP.debut = true;
                        break;
                    case "西":
                        audition.westP.debut = true;
                        break;
                    case "南":
                        audition.southP.debut = true;
                        break;
                    case "北":
                        audition.northP.debut = true;
                        break;
                }

                PutJson();
            }

            if (ws.State != WebSocketState.Open)
            {
                timer1_Tick(null, null);
            }
        }

        private void toolStripSplitButton1_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            timer1.Enabled = false;

            ChangeSide(e.ClickedItem.Text);

            timer1_Tick(null, null);
        }

        private void ChangeSide(string side)
        {
            switch (side)
            {
                case "東":
                    label3.Text = "東";
                    label4.Text = "南";
                    label5.Text = "西";
                    label6.Text = "北";
                    break;
                case "西":
                    label3.Text = "西";
                    label4.Text = "北";
                    label5.Text = "東";
                    label6.Text = "南";
                    break;
                case "南":
                    label3.Text = "南";
                    label4.Text = "西";
                    label5.Text = "北";
                    label6.Text = "東";
                    break;
                case "北":
                    label3.Text = "北";
                    label4.Text = "東";
                    label5.Text = "南";
                    label6.Text = "西";
                    break;
                case "観客モード":
                    観客モードToolStripMenuItem.Checked = !観客モードToolStripMenuItem.Checked;
                    if (観客モードToolStripMenuItem.Checked)
                    {
                        自分のアイドルを非表示ToolStripMenuItem.Checked = false;
                    }
                    break;
                case "自分のアイドルを非表示":
                    自分のアイドルを非表示ToolStripMenuItem.Checked = !自分のアイドルを非表示ToolStripMenuItem.Checked;
                    if (自分のアイドルを非表示ToolStripMenuItem.Checked)
                    {
                        観客モードToolStripMenuItem.Checked = false;
                    }
                    break;
            }

            button1.Enabled = !(観客モードToolStripMenuItem.Checked | 自分のアイドルを非表示ToolStripMenuItem.Checked);
        }

        private void GetJson()
        {
            var result = httpClient.GetAsync(settings.APIserverURL).Result;
            if (result.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show(
                    "サーバからデータを取得できませんでした。",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                this.Close();
            }

            var response = JsonSerializer.Deserialize<Response>(result.Content.ReadAsStringAsync().Result);
            audition = response.room.raw.data;

            roomId = response.room.code;

            if (rev != response.room.raw.rev)
            {
                rev = response.room.raw.rev;

                soundEffect.Play();
            }
        }

        private void PutJson()
        {
            Request req = new Request();
            req.data = audition;
            StringContent content = new StringContent(JsonSerializer.Serialize(req));
            content.Headers.ContentType.MediaType = "application/json";

            var result = httpClient.PutAsync(
                settings.APIserverURL + "&origrev=" + rev.ToString(), content).Result;

            if (result.StatusCode == HttpStatusCode.Conflict)
            {
                MessageBox.Show(
                    "更新が衝突しました。再度操作を行ってください。",
                    "コンフリクト",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (result.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show(
                    "サーバにデータを登録できませんでした。",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                this.Close();
            }

            if (ws.State == WebSocketState.Open)
            {
                var messageJson = new Room();
                messageJson.code = roomId;
                ws.Send(JsonSerializer.Serialize(messageJson));
            }
        }

        private void Ws_Closed(object sender, EventArgs e)
        {
            ws.Open();
        }

        private void Ws_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var messageJson = JsonSerializer.Deserialize<Room>(e.Message);
            if (messageJson.code.Equals(roomId))
            {
                timer1_Tick(null, null);
            }
        }
    }
}
