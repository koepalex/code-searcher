# Hint: you may need to execute 'set-executionpolicy Unrestricted -scope CurrentUser' from powershell

# Script to create Test Environment, this needs only to be done once for the repository
# Don't run this script multiple times, this could end in IP ban from archive.org

# We use books from project gutenberg, via archive.org as test Data
$books  = @{
	"Alice Adventure in Wonderland" = "https://archive.org/download/alicesadventures19033gut/19033-h/19033-h.htm"
	"Fifteen Thousand Useful Phrases" = "https://archive.org/download/fifteenthousandu18362gut/18362.txt"
	"Romeo and Julia" = "https://ia800205.us.archive.org/21/items/romeoundjulia06996gut/7gs1610a.txt"
	"Lady of the Lake" = "https://ia800301.us.archive.org/15/items/ladyofthelake28287gut/28287-8.txt"
	"The Tale of Johnny Town Mouse" = "https://archive.org/download/thetaleofjohnnyt15284gut/thetaleofjohnnyt15284gut_files.xml"
	"The Count of Monte Cristo" = "https://archive.org/download/thecountofmontec01184gut/crsto12.txt"
	"The History of Rome" = "https://archive.org/download/thehistoryofrome10706gut/10706.txt"
}

Write-Host "Create Test Environment started ..."

# Read path where this script is stored
$Path = $PSScriptRoot

$books.Keys | % { 
    $folder = [IO.Path]::combine($Path, "IntegrationTests", "DownloadedTestData", $_)
    
	# Create Folder for Test Data
	try {
		New-Item -Path $folder -ItemType Directory -Force
		Write-Host $folder " sucessfull created"
	} catch {
		Write-Host "Cant create directory " $folder
		continue
	}

	# Download the content of the book
    $bookUri = $books.Item($_)
    try {
        $book = Invoke-WebRequest -UseBasicParsing -Uri $bookUri
		Write-Host $bookUri " sucessfull downloaded"
    } catch {
        Write-Host "Cant load URL:" $bookUri
        continue
    }

	# Save the content of the book locally
	$ext = [System.IO.Path]::GetExtension($bookUri)
    $fileName = [String]::Format("{0}{1}", $_, $ext)
    $fullPath = [IO.Path]::Combine($folder, $fileName)
    $Stream = [IO.StreamWriter]::new($fullPath, $false, [Text.Encoding]::)
    try {
        $Stream.Write($book.Content)
    } catch {
		Write-Host "Error occured while writing test file to disk " $fullPath
		continue
	}
    finally {
        $Stream.Dispose()
		Write-Host $fullPath " sucessfull written to disk"
    }
}

Write-Host "Create Test Environment finished!"