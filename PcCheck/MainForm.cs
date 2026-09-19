using System.Drawing.Drawing2D;

namespace PCCheck;
public sealed class MainForm:Form
{
    AppSettings settings=AppSettings.Load();readonly UsageData data=UsageData.Load();readonly UsageTracker tracker;readonly NotifyIcon tray=new();
    readonly Label todayValue=new(),weekValue=new(),status=new();readonly ListView list=new();readonly Button pause=new(); readonly System.Windows.Forms.Timer refresh=new(){Interval=30000};
    bool allowExit;string lastSummary="";
    readonly Color navy=Color.FromArgb(24,34,54),blue=Color.FromArgb(42,116,245),bg=Color.FromArgb(245,247,251);
    public MainForm()
    {
        Text="PC Check";Size=new Size(860,640);MinimumSize=new Size(760,560);StartPosition=FormStartPosition.CenterScreen;BackColor=bg;Font=new Font("맑은 고딕",10);Icon=SystemIcons.Application;
        BuildUi();tracker=new UsageTracker(settings,data);tracker.Updated+=()=>BeginInvoke(RefreshData);refresh.Tick+=(s,e)=>{RefreshData();CheckSummary();};refresh.Start();
        tray.Icon=SystemIcons.Application;tray.Text="PC Check - 사용시간 기록 중";tray.Visible=true;tray.DoubleClick+=(s,e)=>ShowWindow();tray.ContextMenuStrip=TrayMenu();
        FormClosing+=Closing;Shown+=async(s,e)=>{RefreshData();if(Environment.GetCommandLineArgs().Contains("--background"))Hide();if(settings.SendStartMail&&settings.Sender!="")await SafeMail($"[{settings.DeviceName}] PC 시작",$"PC가 시작되었습니다.\r\n시각: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");};
    }
    void BuildUi()
    {
        var header=new Panel{Dock=DockStyle.Top,Height=86,BackColor=navy};Controls.Add(header);
        header.Controls.Add(new Label{Text="PC Check",ForeColor=Color.White,Font=new Font("맑은 고딕",22,FontStyle.Bold),AutoSize=true,Location=new Point(28,17)});
        status.Text="● 기록 중";status.ForeColor=Color.FromArgb(82,220,155);status.AutoSize=true;status.Location=new Point(31,58);header.Controls.Add(status);
        var settingsBtn=ButtonOf("설정",110,30);settingsBtn.Anchor=AnchorStyles.Top|AnchorStyles.Right;settingsBtn.Location=new Point(ClientSize.Width-138,25);settingsBtn.Click+=(s,e)=>OpenSettings();header.Controls.Add(settingsBtn);header.Resize+=(s,e)=>settingsBtn.Left=header.ClientSize.Width-138;
        var cards=new TableLayoutPanel{Dock=DockStyle.Top,Height=150,Padding=new Padding(24,20,24,10),ColumnCount=2};cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));Controls.Add(cards);
        cards.Controls.Add(Card("오늘 사용시간",todayValue),0,0);cards.Controls.Add(Card("최근 7일",weekValue),1,0);
        var actions=new Panel{Dock=DockStyle.Top,Height=62,Padding=new Padding(24,8,24,8)};Controls.Add(actions);
        pause=ButtonOf("기록 일시정지",145,38);pause.Click+=(s,e)=>Toggle();actions.Controls.Add(pause);
        var mail=ButtonOf("오늘 요약 메일",155,38);mail.Left=158;mail.Click+=async(s,e)=>await SendToday();actions.Controls.Add(mail);
        var clear=ButtonOf("기록 삭제",115,38);clear.Left=326;clear.Click+=(s,e)=>ClearData();actions.Controls.Add(clear);
        var body=new Panel{Dock=DockStyle.Fill,Padding=new Padding(24,8,24,24)};Controls.Add(body);
        body.Controls.Add(new Label{Text="오늘 프로그램별 사용시간",Dock=DockStyle.Top,Height=34,Font=new Font("맑은 고딕",13,FontStyle.Bold),ForeColor=navy});
        list.Dock=DockStyle.Fill;list.View=View.Details;list.FullRowSelect=true;list.GridLines=false;list.BorderStyle=BorderStyle.FixedSingle;list.Columns.Add("프로그램",480);list.Columns.Add("사용시간",180);body.Controls.Add(list);list.BringToFront();
    }
    Panel Card(string title,Label value){var p=new Panel{Margin=new Padding(8),BackColor=Color.White};p.Paint+=(s,e)=>{using var pen=new Pen(Color.FromArgb(225,230,240));e.Graphics.DrawRectangle(pen,0,0,p.Width-1,p.Height-1);};p.Controls.Add(new Label{Text=title,Location=new Point(22,18),AutoSize=true,ForeColor=Color.DimGray});value.Text="0분";value.Location=new Point(20,50);value.AutoSize=true;value.Font=new Font("맑은 고딕",25,FontStyle.Bold);value.ForeColor=navy;p.Controls.Add(value);return p;}
    Button ButtonOf(string text,int w,int h)=>new(){Text=text,Width=w,Height=h,FlatStyle=Flat,BackColor=Color.White,ForeColor=navy};
    void RefreshData(){var today=data.ForDay(DateTime.Today);todayValue.Text=MailService.Duration(today.Values.Sum());weekValue.Text=MailService.Duration(data.LastDays(7).Values.Sum());list.BeginUpdate();list.Items.Clear();foreach(var x in today.OrderByDescending(x=>x.Value)){var i=new ListViewItem(x.Key);i.SubItems.Add(MailService.Duration(x.Value));list.Items.Add(i);}list.EndUpdate();}
    void Toggle(){if(tracker.Running){tracker.Pause();pause.Text="기록 다시 시작";status.Text="● 일시정지";status.ForeColor=Color.Orange;}else{tracker.Resume();pause.Text="기록 일시정지";status.Text="● 기록 중";status.ForeColor=Color.FromArgb(82,220,155);}}
    async Task SendToday(){try{await MailService.Send(settings,$"[{settings.DeviceName}] 오늘 PC 사용 {MailService.Duration(data.ForDay(DateTime.Today).Values.Sum())}",MailService.Summary(settings,data,DateTime.Today));MessageBox.Show("요약 메일을 보냈습니다.","PC Check");}catch(Exception ex){MessageBox.Show("메일 발송 실패\r\n"+ex.Message,"PC Check");}}
    async Task SafeMail(string subject,string body){try{await MailService.Send(settings,subject,body);}catch{}}
    async void CheckSummary(){if(!settings.SendDailySummary||settings.Sender=="")return;if(TimeSpan.TryParse(settings.SummaryTime,out var t)&&DateTime.Now.TimeOfDay>=t&&lastSummary!=DateTime.Today.ToString("yyyy-MM-dd")){lastSummary=DateTime.Today.ToString("yyyy-MM-dd");await SafeMail($"[{settings.DeviceName}] 오늘 PC 사용 {MailService.Duration(data.ForDay(DateTime.Today).Values.Sum())}",MailService.Summary(settings,data,DateTime.Today));}}
    void ClearData(){if(MessageBox.Show("모든 사용 기록을 삭제할까요?","기록 삭제",MessageBoxButtons.YesNo,MessageBoxIcon.Warning)==DialogResult.Yes){data.Clear();RefreshData();}}
    void OpenSettings(){using var f=new SettingsForm(settings);if(f.ShowDialog()==DialogResult.OK){settings=AppSettings.Load();tracker.UpdateSettings(settings);}}
    ContextMenuStrip TrayMenu(){var m=new ContextMenuStrip();m.Items.Add("PC Check 열기",null,(s,e)=>ShowWindow());m.Items.Add("기록 일시정지/시작",null,(s,e)=>Toggle());m.Items.Add("종료",null,(s,e)=>{allowExit=true;Close();});return m;}
    void ShowWindow(){Show();WindowState=FormWindowState.Normal;Activate();}
    async void Closing(object? s,FormClosingEventArgs e){if(!allowExit&&e.CloseReason==CloseReason.UserClosing){e.Cancel=true;Hide();return;}tray.Visible=false;tracker.Dispose();if(settings.Sender!="")await SafeMail($"[{settings.DeviceName}] PC 종료",$"PC 사용이 종료되었습니다.\r\n시각: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");}
}
