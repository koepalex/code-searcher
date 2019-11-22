### Prepare Machine

* Run powershell script [`CreateTestEnvironment.ps1`](./../CodeSearcherTests/CreateTestEnvironment.ps1) **once** 
  * you may need to allow execute powershell scripts locally see [Microsoft Docs](https://docs.microsoft.com/de-de/powershell/module/microsoft.powershell.security/set-executionpolicy)

```powershell
 Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope CurrentUser
```

* copy content of [AddThisToPostBuildEvent.txt](./../CodeSearcherTests/AddThisToPostBuildEvent.txt) into **PostBuildEvent** of CodeSearcher.Tests project
  * don't forget to remove before check-in, otherwise it will break the CI pipeline 