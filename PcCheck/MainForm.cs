namespace PCCheck;

public sealed class MainForm:Form
{
    AppSettings settings=AppSettings.Load();readonly UsageData data=UsageData.Load();readonly UsageTracker tracker;readonly NotifyIcon tray=new();
    readonly Label today=new(),week=new(),state=new(),topApp=new(),attempts=new();readonly ListView appList=new(),securityList=new();Button pause=new();bool allowExit;
    readonly Color dark=Color.FromArgb(27,38,59),blue=Color.FromArgb(45,112,240),green=Color.FromArgb(28,167,112),page=Color.FromArgb(242,245,250);
    public MainForm()
    {
        Text="PC Check - PC 사용시간";Size=new Size(1080,760);MinimumSize=new Size(900,650);StartPosition=FormStartPosition.CenterScreen;BackColor=page;Font=new Font("맑은 고딕",12);Icon=SystemIcons.Shield;
        Build();tracker=new UsageTracker(settings,data);tracker.Updated+=()=>BeginInvoke(RefreshView);
        var timer=new System.Windows.Forms.Timer{Interval=20000};timer.Tick+=(s,e)=>RefreshView();timer.Start();
        tray.Icon=SystemIcons.Shield;tray.Text="PC Check - 사용시간 기록 중";tray.Visible=true;tray.DoubleClick+=(s,e)=>ShowWindow();tray.ContextMenuStrip=TrayMenu();
        Shown+=(s,e)=>{RefreshView();AppSettings.SetAutoStart(true);if(Environment.GetCommandLineArgs().Contains("--background"))Hide();};FormClosing+=OnClosing;
    }
    void Build()
    {
        var header=new Panel{Dock=DockStyle.Top,Height=112,BackColor=dark};Controls.Add(header);
        header.Controls.Add(new Label{Text="PC 사용시간",ForeColor=Color.White,Font=new Font("맑은 고딕",27,FontStyle.Bold),Location=new Point(34,19),AutoSize=true});
        state.Text="● 정상 기록 중";state.ForeColor=Color.FromArgb(92,225,164);state.Font=new Font("맑은 고딕",13,FontStyle.Bold);state.Location=new Point(38,70);state.AutoSize=true;header.Controls.Add(state);
        var manage=BigButton("관리 설정",150);manage.Location=new Point(880,31);manage.Anchor=AnchorStyles.Top|AnchorStyles.Right;manage.Click+=(s,e)=>OpenSettings();header.Controls.Add(manage);header.Resize+=(s,e)=>manage.Left=header.ClientSize.Width-184;
        var tabs=new TabControl{Dock=DockStyle.Fill,Font=new Font("맑은 고딕",13,FontStyle.Bold),Padding=new Point(22,10)};Controls.Add(tabs);
        var dash=new TabPage("한눈에 보기"){BackColor=page,Padding=new Padding(24)};var details=new TabPage("프로그램별 기록"){BackColor=page,Padding=new Padding(24)};var protect=new TabPage("보호 기록"){BackColor=page,Padding=new Padding(24)};tabs.TabPages.AddRange([dash,details,protect]);
        var cards=new TableLayoutPanel{Dock=DockStyle.Top,Height=190,ColumnCount=3,Padding=new Padding(0,8,0,14)};for(int i=0;i<3;i++)cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,33.33f));dash.Controls.Add(cards);
        cards.Controls.Add(Card("오늘 사용시간",today,"실제로 화면을 사용한 시간"),0,0);cards.Controls.Add(Card("최근 7일",week,"지난 7일간 누적 시간"),1,0);cards.Controls.Add(Card("가장 많이 사용",topApp,"오늘 가장 오래 쓴 프로그램"),2,0);
        var guide=new Panel{Dock=DockStyle.Top,Height=145,BackColor=Color.White,Padding=new Padding(26)};dash.Controls.Add(guide);guide.BringToFront();guide.Controls.Add(new Label{Text="현재 PC Check가 정상적으로 작동하고 있습니다",Dock=DockStyle.Top,Height=40,Font=new Font("맑은 고딕",17,FontStyle.Bold),ForeColor=dark});guide.Controls.Add(new Label{Text="창을 닫아도 기록은 계속됩니다. 5분 이상 키보드나 마우스를 사용하지 않으면 사용시간에서 자동으로 제외됩니다.",Dock=DockStyle.Bottom,Height=55,Font=new Font("맑은 고딕",12),ForeColor=Color.DimGray});
        var buttons=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=82,Padding=new Padding(0,18,0,0)};dash.Controls.Add(buttons);pause=BigButton("기록 일시정지",190);pause.Click+=(s,e)=>ToggleTracking();var settingsBtn=BigButton("관리 설정 열기",190);settingsBtn.Click+=(s,e)=>OpenSettings();buttons.Controls.AddRange([pause,settingsBtn]);
        appList.Dock=DockStyle.Fill;appList.View=View.Details;appList.FullRowSelect=true;appList.Font=new Font("맑은 고딕",14);appList.Columns.Add("사용한 프로그램",600);appList.Columns.Add("오늘 사용시간",260);details.Controls.Add(appList);details.Controls.Add(new Label{Text="오늘 사용한 프로그램",Dock=DockStyle.Top,Height=52,Font=new Font("맑은 고딕",20,FontStyle.Bold),ForeColor=dark});
        securityList.Dock=DockStyle.Fill;securityList.View=View.Details;securityList.FullRowSelect=true;securityList.Font=new Font("맑은 고딕",13);securityList.Columns.Add("날짜와 시각",230);securityList.Columns.Add("시도한 작업",350);securityList.Columns.Add("결과",300);protect.Controls.Add(securityList);protect.Controls.Add(new Label{Text="종료·삭제·설정 변경 시도 기록",Dock=DockStyle.Top,Height=52,Font=new Font("맑은 고딕",20,FontStyle.Bold),ForeColor=dark});
    }
    Panel Card(string title,Label value,string note){var p=new Panel{Margin=new Padding(8),BackColor=Color.White,Padding=new Padding(24)};p.Controls.Add(new Label{Text=title,Dock=DockStyle.Top,Height=34,Font=new Font("맑은 고딕",13,FontStyle.Bold),ForeColor=Color.DimGray});value.Text="0분";value.Dock=DockStyle.Top;value.Height=62;value.Font=new Font("맑은 고딕",25,FontStyle.Bold);value.ForeColor=dark;p.Controls.Add(value);value.BringToFront();p.Controls.Add(new Label{Text=note,Dock=DockStyle.Bottom,Height=30,ForeColor=Color.Gray});return p;}
    Button BigButton(string text,int width)=>new(){Text=text,Width=width,Height=48,FlatStyle=FlatStyle.Flat,BackColor=Color.White,ForeColor=dark,Font=new Font("맑은 고딕",12,FontStyle.Bold),Margin=new Padding(0,0,14,0)};
    void RefreshView(){var d=data.ForDay(DateTime.Today);var total=d.Values.Sum();today.Text=Format(total);week.Text=Format(data.LastDays(7).Values.Sum());var top=d.OrderByDescending(x=>x.Value).FirstOrDefault();topApp.Text=top.Key??"아직 없음";appList.BeginUpdate();appList.Items.Clear();foreach(var x in d.OrderByDescending(x=>x.Value)){var i=new ListViewItem(x.Key);i.SubItems.Add(Format(x.Value));appList.Items.Add(i);}appList.EndUpdate();LoadSecurity();}
    static string Format(double sec){var t=TimeSpan.FromSeconds(sec);return t.TotalHours>=1?$"{(int)t.TotalHours}시간 {t.Minutes}분":$"{Math.Max(0,(int)Math.Round(t.TotalMinutes))}분";}
    void LoadSecurity(){securityList.Items.Clear();var path=Path.Combine(AppSettings.Folder,"보호기록.txt");if(!File.Exists(path))return;foreach(var line in File.ReadLines(path).Reverse().Take(100)){var p=line.Split('|');if(p.Length<3)continue;var i=new ListViewItem(p[0].Trim());i.SubItems.Add(p[1].Trim());i.SubItems.Add(p[2].Trim());securityList.Items.Add(i);}}
    void ToggleTracking(){if(!ParentSecurity.Ask(this,settings,tracker.Running?"사용시간 기록 일시정지":"사용시간 기록 다시 시작"))return;if(tracker.Running){tracker.Pause();pause.Text="기록 다시 시작";state.Text="● 부모가 기록을 일시정지함";state.ForeColor=Color.Orange;}else{tracker.Resume();pause.Text="기록 일시정지";state.Text="● 정상 기록 중";state.ForeColor=Color.FromArgb(92,225,164);}}
    void OpenSettings(){using var f=new SettingsForm(settings);if(f.ShowDialog()==DialogResult.OK){settings=AppSettings.Load();tracker.UpdateSettings(settings);RefreshView();}}
    ContextMenuStrip TrayMenu(){var m=new ContextMenuStrip{Font=new Font("맑은 고딕",11)};m.Items.Add("PC Check 열기",null,(s,e)=>ShowWindow());m.Items.Add("프로그램 종료",null,(s,e)=>TryExit());return m;}
    void ShowWindow(){Show();WindowState=FormWindowState.Normal;Activate();}
    void TryExit(){if(ParentSecurity.Ask(this,settings,"PC Check 종료")){allowExit=true;Close();}}
    void OnClosing(object? s,FormClosingEventArgs e){if(!allowExit){e.Cancel=true;Hide();return;}tray.Visible=false;tracker.Dispose();}
}
