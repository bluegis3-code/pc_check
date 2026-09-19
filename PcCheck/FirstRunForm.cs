namespace PCCheck;
public sealed class FirstRunForm:Form
{
 readonly TextBox first=new(),second=new();readonly AppSettings settings;
 public FirstRunForm(AppSettings s){settings=s;Text="PC Check 처음 설정";Size=new Size(560,430);StartPosition=FormStartPosition.CenterScreen;FormBorderStyle=FormBorderStyle.FixedDialog;ControlBox=false;Font=new Font("맑은 고딕",12);Build();}
 void Build(){Controls.Add(new Label{Text="부모 비밀번호 설정",Location=new Point(35,28),AutoSize=true,Font=new Font("맑은 고딕",22,FontStyle.Bold)});Controls.Add(new Label{Text="종료, 기록 삭제, 프로그램 제거에 사용합니다.\r\n아이에게 알려주지 않을 비밀번호를 입력하세요.",Location=new Point(38,82),Size=new Size(470,62),ForeColor=Color.DimGray});Add("비밀번호",first,160);Add("한 번 더 입력",second,225);var b=new Button{Text="설정 완료",Location=new Point(38,305),Size=new Size(470,50),BackColor=Color.FromArgb(42,116,245),ForeColor=Color.White,FlatStyle=FlatStyle.Flat,Font=new Font("맑은 고딕",13,FontStyle.Bold)};b.Click+=(s,e)=>Save();Controls.Add(b);AcceptButton=b;}
 void Add(string label,TextBox box,int y){Controls.Add(new Label{Text=label,Location=new Point(38,y+5),Size=new Size(150,30)});box.Location=new Point(195,y);box.Size=new Size(310,34);box.UseSystemPasswordChar=true;Controls.Add(box);}
 void Save(){if(first.Text.Length<6){MessageBox.Show("비밀번호는 6자 이상으로 설정해주세요.");return;}if(first.Text!=second.Text){MessageBox.Show("두 비밀번호가 다릅니다.");return;}ParentSecurity.SetPassword(settings,first.Text);AppSettings.SetAutoStart(true);DialogResult=DialogResult.OK;Close();}
}
