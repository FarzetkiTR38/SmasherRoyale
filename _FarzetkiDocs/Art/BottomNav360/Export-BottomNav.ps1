$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -ReferencedAssemblies @([System.Drawing.Bitmap].Assembly.Location, [System.Drawing.Rectangle].Assembly.Location, (Join-Path $PSHOME 'System.Private.Windows.GdiPlus.dll'), (Join-Path $PSHOME 'System.Private.Windows.Core.dll')) -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
public static class BottomNavExport {
  public static string Export(string source, string destination) {
    using (var original = new Bitmap(source)) {
      var bits = original.LockBits(new Rectangle(0, 0, original.Width, original.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
      int left=original.Width, top=original.Height, right=-1, bottom=-1, transparent=0;
      try {
        var bytes=new byte[Math.Abs(bits.Stride)*original.Height]; Marshal.Copy(bits.Scan0,bytes,0,bytes.Length);
        for(int y=0;y<original.Height;y++) for(int x=0;x<original.Width;x++) {
          byte alpha=bytes[y*Math.Abs(bits.Stride)+x*4+3]; if(alpha==0) transparent++;
          if(alpha<128) continue;
          left=Math.Min(left,x); top=Math.Min(top,y); right=Math.Max(right,x); bottom=Math.Max(bottom,y);
        }
      } finally { original.UnlockBits(bits); }
      if(transparent==0 || right<left) throw new InvalidOperationException("Generated source has no usable transparency: "+source);
      using (var output = new Bitmap(360,180,PixelFormat.Format32bppArgb))
      using (var graphics = Graphics.FromImage(output))
      using (var attributes = new ImageAttributes()) {
        graphics.CompositingMode=CompositingMode.SourceCopy;
        graphics.InterpolationMode=InterpolationMode.HighQualityBicubic;
        graphics.PixelOffsetMode=PixelOffsetMode.HighQuality;
        graphics.Clear(Color.Transparent);
        attributes.SetWrapMode(WrapMode.TileFlipXY);
        graphics.DrawImage(original,new Rectangle(0,0,360,180),left,top,right-left+1,bottom-top+1,GraphicsUnit.Pixel,attributes);
        output.Save(destination,ImageFormat.Png);
      }
      return string.Format("360x180 RGBA; source frame {0},{1},{2},{3}",left,top,right-left+1,bottom-top+1);
    }
  }
}
'@
$navManifest = Get-Content -Raw (Join-Path $PSScriptRoot 'generation-manifest.json') | ConvertFrom-Json
$navDestination = Join-Path (Get-Location) 'Assets/_Project/Art/_FarzetkiArts/mainmenu/BottomNav_360x180'
New-Item -ItemType Directory -Path $navDestination -Force | Out-Null
$navResults = foreach ($navSource in $navManifest.sources) {
  $navOutput = Join-Path $navDestination ('Nav_' + $navSource.name + '.png')
  [PSCustomObject]@{file=$navOutput;result=[BottomNavExport]::Export($navSource.path,$navOutput)}
}
$navResults | ConvertTo-Json | Set-Content -Encoding utf8 (Join-Path $PSScriptRoot 'export-verification.json')
$navResults | ConvertTo-Json
