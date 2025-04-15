$basePath = Get-ItemPropertyValue -Path "HKCU:\Software\Bohemia Interactive\Publisher" -Name "path"
& "$basePath\PublisherCmd.exe" update /id:3016661145 /changeNote:"(release notes will be available in a few minutes)" /path:"$PSScriptRoot\.hemttout\release"
