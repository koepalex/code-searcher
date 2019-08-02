# code-searcher
-------
A tool to index source code, for faster searches

## Why ?
In my daily work I deal with a lot of different artifacts (source code, meta file, domain specific languages) and almost every week I've to search something over all artifacts. With the fastes search tools it took me around **45min** on an average search. So I was looking for a tool to speed up.
There are some tools available but all have some disadvantages / nogo's, like:
* only work language dependend like csearch, ctags
* only work on linux/mac (codesearch)
* don't work on local machine
* can't handle huge code basis
* indexing of the code, take days

Not finding any tool, which solves my use case, was the reason to start this project. 

## Command Line Version (CodeSearcher)
This chapter describes how to use the command line version of the tool.
### Index a new source code folder
First we have to analyse all the files we want to have searchable. To create a lucene index out of the files you can use the following commands: 
```batchfile
REM CodeSearcher.exe -m=index --ip=PathToStoreIndex --sp=PathOfSourceCode
REM Index files of type (cs, csproj, xml) of folder "D:\repository\project" 
REM and store resulting index in folder "D:\Index"
CodeSearcher.exe -m=index --ip=D:\Index --sp=D:\repository\project
REM Index files of Type (json and xml) of folder "D:\repository\project"
REM and store resulting index in folder "D:\IndexJsonOnly"
CodeSearcher.exe -m=index --ip=D:\IndexJsonXmlOnly --sp=D:\repository\project --fe=".json,.xml"
```

### Use index to search containing word
After indexing we can use the index to find the files and linenumbers containing the searched word, very fast.
```batchfile
REM CodeSearcher.exe -m==search --ip=PathToStoreIndex --sw=WordToSearch
REM search word "class" in index stored under "D:\Index"
CodeSearcher.exe -m=search --ip=D:\Index --sw="class"
REM search word "port" in index stored under "D:\IndexJsonXmlOnly" show first 100 hits
CodeSearcher.exe -m=search --ip="C:\IndexXmlOnly" --sw="port" --hits=100
```