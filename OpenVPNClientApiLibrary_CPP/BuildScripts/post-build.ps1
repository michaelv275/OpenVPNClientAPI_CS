Param (
	$SolutionDir,
	$TargetDir
)

Write-Output ""
Write-Output "=================== OpenVPN C++ post-build ==================="

$NetProjectCPPLibDirectory = Join-Path -Path "$SolutionDir" -ChildPath "OpenvpnNetClient\lib\CPP_lib"

if (!(Test-Path -Path "$NetProjectCPPLibDirectory" -PathType Container))
{
	Write-Output ".NET project lib folder missing C++ lib subfolder. Creating"
	New-Item -Path "$NetProjectCPPLibDirectory" -ItemType Directory | Out-Null
}
else {
	#Clear out old files
	Get-ChildItem -Path "$NetProjectCPPLibDirectory" | Remove-Item
}

#Copying C++ dlls and lib to solution Output folder
Write-Output "Copying C++ library files to temp folder ($NetProjectCPPLibDirectory) for .NET project to use"
Get-ChildItem -Path "$TargetDir" -Recurse -Include *.dll*, *.lib, *.obj, *.exp | Copy-Item -Destination "$NetProjectCPPLibDirectory" -Force

Write-Output "============== OpenVPN C++ post-build completed =============="
Write-Output ""