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

#Handle Vcpkg dependencies
Write-Output "Moving vcpkg dependencies"
$source = Join-Path -Path "$ProjectDir" -ChildPath "Dependencies\$Configuration"
$vcpkgDestination = Join-Path -Path "$ProjectDir" -ChildPath "$PlatformName\$Configuration"

Copy-Item -Path "$source" -Destination "$vcpkgDestination" -Recurse -Force

Write-Output "============== OpenVPN C++ pre-build completed =============="
Write-Output ""