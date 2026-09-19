namespace PCCheck;
public sealed class SettingsForm:Form
{
 readonly AppSettings settings;readonly TextBox newPass=new(),confirm=new();
 public SettingsForm(AppSettings s)
 {
  settings=s;if(!ParentSecurity.Ask(this,s,"부모 관리 열기")){Shown+=(a,b)=>Close();return;}
  Text="부모 관리";ClientSize=new Size(620,520);StartPosition=FormStartPosition.CenterParent;FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;AutoScaleMode=AutoScaleMode.Dpi;Font=new Font("Malgun Gothic",12);Build();
 }
 void Build()
 {
  Controls.Add(new Label{Text="부모 관리",Location=new Point(35,25),Size=new Size(400,45),Font=new Font("Malgun Gothic",23,FontStyle.Bold)});
  var status=new Panel{Location=new Point(35,85),Size=new Size(550,85),BackColor=Color.FromArgb(238,249,244)};Controls.Add(status);status.Controls.Add(new Label{Text="자동 실행 및 보호 기능 작동 중",Location=new Point(22,15),Size=new Size(490,30),Font=new Font("Malgun Gothic",14,FontStyle.Bold),ForeColor=Color.FromArgb(25,145,95)});status.Controls.Add(new Label{Text="PC를 켜면 자동으로 기록을 시작합니다.",Location=new Point(23,49),Size=new Size(480,24),ForeColor=Color.DimGray});
  Controls.Add(new Label{Text="부모 비밀번호 변경",Location=new Point(38,205),Size=new Size(400,35),Font=new Font("Malgun Gothic",16,FontStyle.Bold)});AddRow("새 비밀번호",newPass,255);AddRow("한 번 더",confirm,310);
  var save=Button("비밀번호 변경",35,375,260,50,Color.FromArgb(42,116,245),Color.White);save.Click+=(s,e)=>Save();var clear=Button("모든 기록 삭제",325,375,260,50,Color.White,Color.Firebrick);clear.Click+=(s,e)=>Clear();var uninstall=Button("PC Check 프로그램 제거",35,442,550,45,Color.White,Color.Firebrick);uninstall.Click+=(s,e)=>Uninstall();
 }
 void AddRow(string label,TextBox box,int y){Controls.Add(new Label{Text=label,Location=new Point(40,y+5),Size=new Size(150,30)});box.Location=new Point(195,y);box.Size=new Size(390,35);box.UseSystemPasswordChar=true;Controls.Add(box);}
 Button Button(string text,int x,int y,int w,int h,Color back,Color fore){var b=new Button{Text=text,Location=new Point(x,y),Size=new Size(w,h),BackColor=back,ForeColor=fore,FlatStyle=FlatStyle.Flat,Font=new Font("Malgun Gothic",12,FontStyle.Bold)};Controls.Add(b);return b;}
 void Save(){if(newPass.Text.Length<6||newPass.Text!=confirm.Text){MessageBox.Show("새 비밀번호를 6자 이상 동일하게 입력해주세요.");return;}ParentSecurity.SetPassword(settings,newPass.Text);MessageBox.Show("비밀번호를 변경했습니다.");DialogResult=DialogResult.OK;Close();}
 void Clear(){if(!ParentSecurity.Ask(this,settings,"모든 기록 삭제"))return;if(MessageBox.Show("모든 기록을 완전히 삭제할까요?","최종 확인",MessageBoxButtons.YesNo,MessageBoxIcon.Warning)==DialogResult.Yes){UsageData.Load().Clear();MessageBox.Show("기록을 삭제했습니다.");DialogResult=DialogResult.OK;}}
 void Uninstall(){if(!ParentSecurity.Ask(this,settings,"PC Check 프로그램 제거"))return;var u=Path.Combine(AppContext.BaseDirectory,"unins000.exe");if(File.Exists(u)){ParentSecurity.LogAttempt("프로그램 제거 실행",true);System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(u,"/PARENTREMOVE=PCCHK-71A9"){UseShellExecute=true});Environment.Exit(0);}else MessageBox.Show("제거 프로그램을 찾지 못했습니다.");}
}
