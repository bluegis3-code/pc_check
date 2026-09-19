namespace PCCheck;

public sealed class MainForm:Form
{
    AppSettings settings=AppSettings.Load();readonly UsageData data=UsageData.Load();readonly UsageTracker tracker;readonly NotifyIcon tray=new();
    readonly Label powered=new(),active=new(),started=new(),allTime=new(),state=new();readonly ListView appList=new(),dailyList=new(),sessionList=new(),securityList=new();Button pause=new();bool allowExit;
    readonly Color dark=Color.FromArgb(27,38,59),page=Color.FromArgb(242,245,250),blue=Color.FromArgb(45,112,240);
    public MainForm()
    {
        Text="PC Check - PC 사용시간";ClientSize=new Size(1120,780);MinimumSize=new Size(940,680);StartPosition=FormStartPosition.CenterScreen;BackColor=page;Font=new Font("Malgun Gothic",11);AutoScaleMode=AutoScaleMode.Dpi;Icon=SystemIcons.Shield;
        Build();tracker=new UsageTracker(settings,data);tracker.Updated+=()=>BeginInvoke(RefreshView);
        var timer=new System.Windows.Forms.Timer{Interval=15000};timer.Tick+=(s,e)=>RefreshView();timer.Start();
        tray.Icon=SystemIcons.Shield;tray.Text="PC Check - 사용시간 기록 중";tray.Visible=true;tray.DoubleClick+=(s,e)=>ShowWindow();tray.ContextMenuStrip=TrayMenu();
        Shown+=(s,e)=>{RefreshView();AppSettings.SetAutoStart(true);if(Environment.GetCommandLineArgs().Contains("--background"))Hide();};FormClosing+=OnClosing;
    }
    void Build()
    {
        var header=new Panel{Dock=DockStyle.Top,Height=105,BackColor=dark};Controls.Add(header);
        header.Controls.Add(new Label{Text="PC 사용시간",ForeColor=Color.White,Font=new Font("Malgun Gothic",26,FontStyle.Bold),Location=new Point(34,15),Size=new Size(400,48)});
        state.Text="● 정상 기록 중";state.ForeColor=Color.FromArgb(92,225,164);state.Font=new Font("Malgun Gothic",13,FontStyle.Bold);state.Location=new Point(38,66);state.Size=new Size(380,30);header.Controls.Add(state);
        var manage=BigButton("관리 설정",150);manage.Anchor=AnchorStyles.Top|AnchorStyles.Right;manage.Location=new Point(930,27);manage.Click+=(s,e)=>OpenSettings();header.Controls.Add(manage);header.Resize+=(s,e)=>manage.Left=header.ClientSize.Width-184;
        var tabs=new TabControl{Dock=DockStyle.Fill,Font=new Font("Malgun Gothic",12,FontStyle.Bold),Padding=new Point(20,9)};Controls.Add(tabs);
        var dash=Page("한눈에 보기"),apps=Page("프로그램별 기록"),daily=Page("일별 기록"),sessions=Page("PC 켠 기록"),protect=Page("보호 기록");tabs.TabPages.AddRange([dash,apps,daily,sessions,protect]);
        BuildDashboard(dash);BuildListPage(apps,appList,"오늘 사용한 프로그램",[("사용한 프로그램",600),("실제 사용시간",280)]);BuildListPage(daily,dailyList,"최근 30일 사용 기록",[("날짜",170),("PC 켠 시간",220),("실제 사용시간",220),("처음 켠 시각",180),("마지막 종료",180)]);BuildListPage(sessions,sessionList,"PC를 켜고 끈 기록",[("날짜",150),("켠 시각",180),("끈 시각",180),("켜져 있던 시간",230)]);BuildListPage(protect,securityList,"종료·삭제·설정 변경 시도 기록",[("날짜와 시각",230),("시도한 작업",380),("결과",300)]);
    }
    TabPage Page(string text)=>new(text){BackColor=page,Padding=new Padding(22)};
    void BuildDashboard(TabPage pageTab)
    {
        var outer=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=3,Padding=new Padding(2)};outer.RowStyles.Add(new RowStyle(SizeType.Absolute,340));outer.RowStyles.Add(new RowStyle(SizeType.Absolute,145));outer.RowStyles.Add(new RowStyle(SizeType.Percent,100));pageTab.Controls.Add(outer);
        var cards=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=2};cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));cards.RowStyles.Add(new RowStyle(SizeType.Percent,50));cards.RowStyles.Add(new RowStyle(SizeType.Percent,50));outer.Controls.Add(cards,0,0);
        cards.Controls.Add(Card("오늘 PC 사용시간",powered,"오늘 PC Check가 실행된 전체 시간"),0,0);cards.Controls.Add(Card("오늘 프로그램 기록시간",active,"프로그램별 사용시간의 합계"),1,0);cards.Controls.Add(Card("현재 PC 시작 시각",started,"Windows 로그인 후 기록 시작 시각"),0,1);cards.Controls.Add(Card("전체 누적 PC 사용시간",allTime,"PC Check 설치 후 누적 기록"),1,1);
        var info=new Panel{Dock=DockStyle.Fill,Margin=new Padding(8,10,8,8),BackColor=Color.White};outer.Controls.Add(info,0,1);info.Controls.Add(new Label{Text="현재 정상적으로 기록하고 있습니다",Location=new Point(28,22),Size=new Size(760,38),Font=new Font("Malgun Gothic",17,FontStyle.Bold),ForeColor=dark});info.Controls.Add(new Label{Text="창을 닫아도 기록은 계속되며 PC를 다시 켜면 자동으로 실행됩니다.",Location=new Point(30,70),Size=new Size(970,32),Font=new Font("Malgun Gothic",12),ForeColor=Color.DimGray});
        var actions=new FlowLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(8,20,0,0)};outer.Controls.Add(actions,0,2);pause=BigButton("기록 일시정지",200);pause.Click+=(s,e)=>ToggleTracking();var settingsBtn=BigButton("관리 설정 열기",200);settingsBtn.Click+=(s,e)=>OpenSettings();actions.Controls.AddRange([pause,settingsBtn]);
    }
    Panel Card(string title,Label value,string note)
    {
        var p=new Panel{Dock=DockStyle.Fill,Margin=new Padding(8),BackColor=Color.White};
        p.Controls.Add(new Label{Text=title,Location=new Point(26,18),Size=new Size(470,30),Font=new Font("Malgun Gothic",14,FontStyle.Bold),ForeColor=Color.FromArgb(90,98,112)});
        value.Text="확인 중";value.Location=new Point(25,55);value.Size=new Size(480,52);value.Font=new Font("Malgun Gothic",25,FontStyle.Bold);value.ForeColor=dark;value.AutoEllipsis=true;p.Controls.Add(value);
        p.Controls.Add(new Label{Text=note,Location=new Point(27,118),Size=new Size(470,26),Font=new Font("Malgun Gothic",10),ForeColor=Color.Gray,AutoEllipsis=true});return p;
    }
    void BuildListPage(TabPage tab,ListView list,string title,(string,int)[] columns)
    {
        var heading=new Label{Text=title,Dock=DockStyle.Top,Height=58,Font=new Font("Malgun Gothic",19,FontStyle.Bold),ForeColor=dark};tab.Controls.Add(heading);list.Dock=DockStyle.Fill;list.View=View.Details;list.FullRowSelect=true;list.GridLines=true;list.HideSelection=false;list.Font=new Font("Malgun Gothic",13);foreach(var c in columns)list.Columns.Add(c.Item1,c.Item2);tab.Controls.Add(list);list.BringToFront();
    }
    Button BigButton(string text,int width)=>new(){Text=text,Width=width,Height=50,FlatStyle=FlatStyle.Flat,BackColor=Color.White,ForeColor=dark,Font=new Font("Malgun Gothic",12,FontStyle.Bold),Margin=new Padding(0,0,14,0)};
    void RefreshView()
    {
        var today=data.ForDay(DateTime.Today);powered.Text=Format(data.PoweredSeconds(DateTime.Today));active.Text=Format(today.Values.Sum());var current=data.Sessions.LastOrDefault(x=>x.End==null);started.Text=current==null?"기록 없음":current.Start.ToString("HH:mm:ss");allTime.Text=Format(data.TotalPoweredSeconds());
        appList.BeginUpdate();appList.Items.Clear();foreach(var x in today.OrderByDescending(x=>x.Value)){var i=new ListViewItem(DisplayName(x.Key));i.SubItems.Add(Format(x.Value));appList.Items.Add(i);}appList.EndUpdate();
        dailyList.BeginUpdate();dailyList.Items.Clear();for(int n=0;n<30;n++){var day=DateTime.Today.AddDays(-n);var ss=data.SessionsForDay(day).OrderBy(x=>x.Start).ToList();var row=new ListViewItem(day.ToString("yyyy-MM-dd (ddd)"));row.SubItems.Add(Format(data.PoweredSeconds(day)));row.SubItems.Add(Format(data.ForDay(day).Values.Sum()));row.SubItems.Add(ss.Count==0?"-":ss.First().Start.ToString("HH:mm:ss"));var last=ss.LastOrDefault();row.SubItems.Add(last==null?"-":last.End?.ToString("HH:mm:ss")??"현재 사용 중");dailyList.Items.Add(row);}dailyList.EndUpdate();
        sessionList.BeginUpdate();sessionList.Items.Clear();foreach(var x in data.Sessions.OrderByDescending(x=>x.Start).Take(200)){var i=new ListViewItem(x.Start.ToString("yyyy-MM-dd"));i.SubItems.Add(x.Start.ToString("HH:mm:ss"));i.SubItems.Add(x.End?.ToString("HH:mm:ss")??"현재 사용 중");i.SubItems.Add(Format(((x.End??x.LastSeen)-x.Start).TotalSeconds));sessionList.Items.Add(i);}sessionList.EndUpdate();LoadSecurity();
    }
    static string Format(double sec){var t=TimeSpan.FromSeconds(Math.Max(0,sec));return t.TotalHours>=1?$"{(int)t.TotalHours}시간 {t.Minutes}분":$"{Math.Max(0,(int)Math.Round(t.TotalMinutes))}분";}
    static string DisplayName(string name)=>name.ToLowerInvariant() switch{"msedge"=>"Microsoft Edge","chrome"=>"Google Chrome","robloxplayerbeta"=>"Roblox","overwatch"=>"Overwatch","discord"=>"Discord","explorer"=>"Windows 탐색기","applicationframehost"=>"Windows 앱","searchhost"=>"Windows 검색",_=>name};
    void LoadSecurity(){securityList.Items.Clear();var path=Path.Combine(AppSettings.Folder,"보호기록.txt");if(!File.Exists(path))return;foreach(var line in File.ReadLines(path).Reverse().Take(100)){var p=line.Split('|');if(p.Length<3)continue;var i=new ListViewItem(p[0].Trim());i.SubItems.Add(p[1].Trim());i.SubItems.Add(p[2].Trim());securityList.Items.Add(i);}}
    void ToggleTracking(){if(!ParentSecurity.Ask(this,settings,tracker.Running?"사용시간 기록 일시정지":"사용시간 기록 다시 시작"))return;if(tracker.Running){tracker.Pause();pause.Text="기록 다시 시작";state.Text="● 부모가 기록을 일시정지함";state.ForeColor=Color.Orange;}else{tracker.Resume();pause.Text="기록 일시정지";state.Text="● 정상 기록 중";state.ForeColor=Color.FromArgb(92,225,164);}}
    void OpenSettings(){using var f=new SettingsForm(settings);if(f.ShowDialog()==DialogResult.OK){settings=AppSettings.Load();tracker.UpdateSettings(settings);RefreshView();}}
    ContextMenuStrip TrayMenu(){var m=new ContextMenuStrip{Font=new Font("Malgun Gothic",11)};m.Items.Add("PC Check 열기",null,(s,e)=>ShowWindow());m.Items.Add("프로그램 종료",null,(s,e)=>TryExit());return m;}
    void ShowWindow(){Show();WindowState=FormWindowState.Normal;Activate();}
    void TryExit(){if(ParentSecurity.Ask(this,settings,"PC Check 종료")){allowExit=true;Close();}}
    void OnClosing(object? s,FormClosingEventArgs e){if(!allowExit){e.Cancel=true;Hide();return;}tray.Visible=false;tracker.Dispose();}
}
