[![License](https://img.shields.io/github/license/koepalex/code-searcher?style=flat-square)](https://github.com/koepalex/code-searcher/blob/master/LICENSE)
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/koepalex/code-searcher)
![GitHub Build](https://img.shields.io/github/workflow/status/koepalex/code-searcher/Ensure%20Build%20success?style=flat-square)
![CodeFactor](https://img.shields.io/codefactor/grade/github/koepalex/code-searcher?style=flat-square)
![Github Lines Code](https://img.shields.io/tokei/lines/github/koepalex/code-searcher?style=flat-square)

# code-searcher
-------
A tool to index source code, for faster searches

## Why ?
In my daily work I deal with a lot of different artifacts (source code, meta file, domain specific languages) and almost every week I've to search something over all artifacts. With the fastest search tools it took me around **45min** on an average search. So I was looking for a tool to speed up.
There are some tools available but all have some disadvantages / nogo's, like:
* only work language dependent like csearch, ctags
* only work on linux/mac (codesearch)
* don't work on local machine
* can't handle huge code basis
* indexing of the code, take days

Not finding any tool, which solves my use case, was the reason to start this project. 

## Command Line Version (CodeSearcher)
This chapter describes how to use the command line version of the tool.

### Use Command Line Menu
To improve the usability code-searcher provides an interactive menu within console. You should Start with this option if you are a new user.

```powershell
CodeSearcher.exe auto
```

### Index a new source code folder
First we have to analyse all the files we want to have searchable. To create a lucene index out of the files you can use the following commands: 
```batchfile
REM CodeSearcher.exe index --indexPath PathToStoreIndex --sourcePath PathOfSourceCode

REM Index files of type (cs, csproj, xml) of folder "D:\repository\project" 
REM and store resulting index in folder "D:\Index"
CodeSearcher.exe index --indexPath D:\Index --sourcePath D:\repository\project
REM Index files of Type (json and xml) of folder "D:\repository\project"
REM and store resulting index in folder "D:\IndexJsonOnly"
CodeSearcher.exe index --indexPath D:\IndexJsonXmlOnly --sourcePath D:\repository\project --fileExtensions .json,.xml
```

### Use index to search containing word
After indexing we can use the index to find the files and line numbers containing the searched word, very fast.
```batchfile
REM CodeSearcher.exe search --indexPath PathToStoreIndex --searchWord WordToSearch

REM search word "class" in index stored under "D:\Index"
CodeSearcher.exe search --indexPath D:\Index --searchWord class
REM search word "port" in index stored under "D:\IndexJsonXmlOnly" show first 100 hits
CodeSearcher.exe search --indexPath C:\IndexXmlOnly --searchWord port --numberOfHits 100
```
To see all options use CodeSearcher.exe search --help

## Web API
The CodeSearcher.WebAPI provides an REST ful API to use the code searcher. The swagger (OpenAPI) webpage is available under [http://localhost:5000](http://localhost:5000), this also the port to use the interface.
The [swagger (OpenAPI) definition](_docs/webapi/code-searcher.v1.swagger.json), that can be used to generate access code in different languages (e.g. by using [AutoRest](https://github.com/Azure/AutoRest)), it is also served under [http://localhost:5000/swagger/v1/swagger.json](http://localhost:5000/swagger/v1/swagger.json).

## Development
### Testing
See [Test documentation](./_docs/tests.md).

## Resources
**Icons:**
* AppIcon from [freeicons.io](https://www.freeicons.io/business-seo-elements/head-magnifying-glass-mind-search-icon-38244) - Creative Commons(Attribution 3.0 unported)
