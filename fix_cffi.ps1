# PowerShell script to fix cffi/model.py
$cffiFilePath = "C:\repos\nafila\venv\Lib\site-packages\cffi\model.py"

Write-Host "Fixing cffi/model.py file at: $cffiFilePath"

if (Test-Path $cffiFilePath) {
    $content = Get-Content -Path $cffiFilePath -Raw -Encoding UTF8
    
    # Locate the PointerType.__init__ method and fix it
    $pointerTypePattern = @'
  class PointerType\(BaseType\):
      _attrs_ = \('totype', 'quals'\)

      def __init__\(self, totype, quals=0\):
          self\.totype = totype
          self\.quals = quals
          extra = " \*&"
          if totype\.is_array_type:
              extra = "\(%s\)" % \(extra\.lstrip\(\),\)
          extra = qualify\(quals, extra\)
'@

    $brokenLine = "          self.c_name_with_marker = isinstance(c_name_with_marker, str) and c_name_with_marker.replace('&', extra)"
    
    # Find where in the content the __init__ method appears
    $matches = [regex]::Matches($content, $pointerTypePattern)
    
    if ($matches.Count -gt 0) {
        Write-Host "Found PointerType.__init__ method"
        
        # Get the position right after the match
        $pos = $matches[0].Index + $matches[0].Length
        
        # Find the next line using regex
        $nextLineMatch = [regex]::Match($content.Substring($pos), "[\r\n]+(.+?)[\r\n]")
        
        if ($nextLineMatch.Success) {
            $nextLine = $nextLineMatch.Groups[1].Value.Trim()
            Write-Host "Found the line to replace: $nextLine"
            
            # Replace the broken line with the fixed one - using totype.c_name_with_marker
            $fixedContent = $content.Replace($nextLine, 
                "          self.c_name_with_marker = totype.c_name_with_marker.replace('&', extra) if hasattr(totype, 'c_name_with_marker') and isinstance(totype.c_name_with_marker, str) else 'void' + extra")
            
            # Write the fixed content back
            $fixedContent | Set-Content -Path $cffiFilePath -Encoding UTF8
            
            Write-Host "Successfully fixed cffi/model.py"
        } else {
            Write-Host "Could not find the line to replace after the __init__ method"
        }
    } else {
        Write-Host "Could not find the PointerType.__init__ method in the file"
    }
} else {
    Write-Host "File does not exist: $cffiFilePath"
} 