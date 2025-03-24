Param(
	$SolutionDir,
	$PlatformName,
	$Configuration,
	$TargetDir
)

Write-Output "============================= OpenVPNClientApi_CS pre-build ============================="

$source = Join-Path -Path "$SolutionDir" -ChildPath "CPP_DLL_Lib_Output\$PlatformName\$Configuration"

if (!(Test-Path -Path $source -PathType Container)) {
	Write-Output "Creating Output directory. You may need to run the C++ generation tool"

	New-Item -ItemType Directory -Path "$source"
}

copy /Y "$source\*" "$TargetDir"

Write-Output "======================== OpenVPNClientApi_CS pre-build completed ========================"