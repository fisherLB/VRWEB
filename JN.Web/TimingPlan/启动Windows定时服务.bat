%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\installutil.exe F:\wp18080102\web\JN.Web\TimingPlan\JN.WindowsService.exe
Net Start wp18080102
sc config wp18080102 start= auto
pause