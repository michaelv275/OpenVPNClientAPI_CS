Param(
	$ProjectDir,
	$Platform
)

Write-Output ""
Write-Output "============================= OpenvpnNetClient post-build ============================="

#Build nuget package
$csprojLocation = Join-Path -Path "$ProjectDir" -ChildPath "openvpnNetClient.csproj"
$packageLocation = Join-Path -Path "$ProjectDir" -ChildPath "PackageOutput"

dotnet pack $csprojLocation -c "$Platform/Release" --no-build -o "$packageLocation"

#Remove cpp_lib folder
$source = Join-Path -Path "$ProjectDir" -ChildPath "lib\CPP_lib"

if ((Test-Path -Path $source -PathType Container)) {
	Write-Output "Removing generated C++ library files"

	Get-ChildItem -Path "$source" | Remove-Item -Filter Force
	Get-Item -Path $source | Remove-Item -Force
}

Write-Output "======================== OpenvpnNetClient post-build completed ========================"
Write-Output ""