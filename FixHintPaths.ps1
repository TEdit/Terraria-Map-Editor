$hintPathPattern = @"
<HintPath>(\d|\w|\s|\.|\\)*packages
"@

ls -Recurse -include *.csproj, *.sln, *.fsproj, *.vbproj |
  foreach {
    $content = cat $_.FullName | Out-String
    $origContent = $content
    $content = $content -replace $hintPathPattern, "<HintPath>`$(SolutionDir)packages"
    if ($origContent -ne $content)
    {	
        $content | out-file -encoding "UTF8" $_.FullName
        write-host messed with $_.Name
    }		    
}