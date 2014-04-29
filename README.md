TranslationTool
==============

Swiss army knife for software translation file formats. Allows easy export of Google Spreadsheets to Visual Studio RESX files and angular-translation files.

**tt.exe** assumes a file called "client_secret.json" for authentication with Google API in the same directory. Go and create one in the [Google API Console](https://developers.google.com/console/help/) if you don't have one yet!


----
### Command line options


#### tt.exe
```
  upload     upload translation files
  download   download translation files
  diff       show difference between local file and online gdoc
  patch      patch local files with patch file from stdin
```
#### tt.exe upload
```
  -s, --soffice       (Default: C:\Program Files (x86)\LibreOffice 4.0\program\soffice.exe) Path to local SOffice 
                                installation used for upload.
                                
  -b, --batch         (Default: False) 
  
  -m, --moduleName    Name of the RESX file. E.g. FIGGO in case of FIGGO.en.resx
  
  -g, --gdrive        Required. Name of Google Drive folder for upload
  
  -r, --resxdir       (Default: D:\Serveurs\Sites\iLucca\iLuccaEntities\Resources\) 
                      Location of ResX files
                      
  -v, --verbose       Be verbose
```


####tt.exe download
```
  -a, --angular             (Default: False) Output files in Angular translate 
                            format instead of RESX

  -n, --multiSpreadsheet    Use the names of the spreadsheets in the file as 
                            the module name. Otherwise only the first 
                            spreadsheet is processed.

  -m, --moduleName          Name of the RESX file. Used in case only the first 
                            spreadsheet is processed for download. If option -n
                            is used only export this specific module.
                            
  -t, --tags                (Default: ) Filter by tags. Tags are given in gdoc 
                            file by a special line before a block, starting 
                            with the hash-bang: #js
  
  -f, --file                Required. Name of file to download

  -g, --gdrive              Name of Google Drive folder for restricted search

  -r, --resxdir             (Default: 
                            D:\Serveurs\Sites\iLucca\iLuccaEntities\Resources\)
                            Location of ResX files

  -v, --verbose             Be verbose
```

#### tt.exe diff
```
  --html                    (Default: False) show diff as formatted HTML (opens in browser)

  -n, --multiSpreadsheet    Use the names of the spreadsheets in the file as the module name. 
                            Otherwise only the first spreadsheet is processed.

  -m, --moduleName          Name of the RESX file. Used in case only the first spreadsheet is 
                            processed for download. If option -n is used only export this specific module.
                            
  -t, --tags                (Default: ) Filter by tags. Tags are given in gdoc 
                            file by a special line before a block, starting 
                            with the hash-bang: #js
  
  -f, --file                Required. Name of file to download

  -g, --gdrive              Name of Google Drive folder for restricted search

  -r, --resxdir             (Default:  D:\Serveurs\Sites\iLucca\iLuccaEntities\Resources\)
                            Location of ResX files

  -v, --verbose             Be verbose
```

#### tt.exe patch
```
  -m, --moduleName    Required. Name of the RESX file. E.g. FIGGO in case of 
                      FIGGO.en.resx

  -r, --resxdir       (Default: D:\Serveurs\Sites\iLucca\iLuccaEntities\Resources\) 
                      Location of ResX files

  -v, --verbose       Be verbose
```
