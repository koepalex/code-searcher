#local variables
$testDataPath = Join-Path $pwd "\Kernel"
$sourceUrl = "https://github.com/torvalds/linux/archive/master.zip"
$zipPath = Join-Path $pwd "\master.zip"

#function definitions
Add-Type -AssemblyName System.IO.Compression.FileSystem
function Unzip {
    param([string]$zipfile, [string]$outpath)

	"[Info] start unzipping file"
    [System.IO.Compression.ZipFile]::ExtractToDirectory($zipfile, $outpath)
	"[Info] unzipping file finished"
}

function DelteOldTestDataFolder {
	If (Test-Path $testDataPath) {
		"[Info] start removing old test data folder"
		Remove-Item $testDataPath
		"[Info] removing test data folder finished"
	} Else {
		"[Info] test data folder dosn't exist"
	}
}

function DownloadFile {
	If (Test-Path $zipPath) {
		"[Info] zip already exist, using it instead of downloading new one"
	} Else {
		"[Info] start downloading file"
		Invoke-WebRequest $sourceUrl -OutFile $zipPath
		"[Info] downloading file finished"
	}
}

function ProcessDownloadedFile {
	If (Test-Path $zipPath) {
		Unzip $zipPath $testDataPath
		If (Test-Path $testDataPath) {
			"[Success] test data folder successfully created"
		} Else {
			"[Error] test data not found after unzipping"
		}
	} Else {
		"[Error] file not found after download"
	}
}

function Info {
    "[Info] Program will fail on case-insensitive file system, e.g. based on file xt_connmark.h and xt_CONNMARK.h"
    "[Info] If you get the error from [System.IO.Compression.ZipFile]::ExtractToDirectory($zipfile, $outpath) please unzip file yourself"
}

# main program starts here
Info
DelteOldTestDataFolder
DownloadFile
ProcessDownloadedFile