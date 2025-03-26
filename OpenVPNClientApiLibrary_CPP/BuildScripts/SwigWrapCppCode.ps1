Param (
	$SolutionDir
)

Write-Output ""
Write-Output "=================== openvpncli.i Swig run ==================="

Write-Output "Wrapping OpenVPN C++ exposed objects in C# wrapper"

if (!(Test-Path -Path "C:\Dispel\Tools\Swig\swigwin-4.3.0\swig.exe" -PathType Any))
{
	Write-Error "Swig tool not found. You need to install the latest version of swigwin"

	exit 1
}

$NetInvokableFilesLocation = Join-Path -Path "$SolutionDir" -ChildPath "OpenvpnNetClient\lib\OpenVPNInvokableFiles"

C:\Dispel\Tools\Swig\swigwin-4.3.0\swig.exe -csharp -c++ -outdir "$NetInvokableFilesLocation" openvpncli.i


Write-Output "============== openvpncli.i Swig run completed =============="
Write-Output ""
