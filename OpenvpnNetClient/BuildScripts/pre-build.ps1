Param(
	$ProjectDir,
	$OutDir
)

Write-Output ""
Write-Output "============================= OpenvpnNetClient pre-build ============================="

$source = Join-Path -Path "$ProjectDir" -ChildPath "lib\CPP_lib"

if (!(Test-Path -Path $source -PathType Container)) {
	Write-Output "Creating Output directory. You may need to run the C++ generation tool"

	New-Item -ItemType Directory -Path "$source"
}

$outputDestination = Join-Path -Path "$ProjectDir" -ChildPath "$OutDir"

if (!(Test-Path -Path "$outputDestination" -PathType Any)) {
		Write-Output "Something is wrong with outputDestination."
	Write-Output "It equals $outputDestination."
}

Get-ChildItem -Path "$source" -Recurse | Copy-Item -Destination "$outputDestination" -Force


Write-Output "======================== OpenvpnNetClient pre-build completed ========================"
Write-Output ""