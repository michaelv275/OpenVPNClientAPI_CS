Param(
	$ProjectDir
)

Write-Output ""
Write-Output "============================= OpenvpnNetClient post-build ============================="

$source = Join-Path -Path "$ProjectDir" -ChildPath "lib\CPP_lib"

if ((Test-Path -Path $source -PathType Container)) {
	Write-Output "Removing generated C++ library files"

	Get-ChildItem -Path "$source" | Remove-Item -Filter Force
	Get-Item -Path $source | Remove-Item -Force
}

Write-Output "======================== OpenvpnNetClient post-build completed ========================"
Write-Output ""