Param(
	$SolutionDir,
	$PlatformName,
	$Configuration,
	$ProjectDir
)

Write-Output ""
Write-Output "=================== OpenVPN C++ pre-build ==================="

$NetProjectInvokableFilesFolder = Join-Path -Path "$SolutionDir" -ChildPath "OpenvpnNetClient\lib\OpenVPNInvokableFiles"

if (!(Test-Path -Path "$NetProjectInvokableFilesFolder" -PathType Container))
{
	Write-Output ".NET project lib folder missing invokable files subfolder. Creating"
	New-Item -Path "$NetProjectInvokableFilesFolder" -ItemType Directory | Out-Null
}

Write-Output "============== OpenVPN C++ pre-build completed =============="
Write-Output ""