=== Download (Gdoc -> RESX)

TranslationTool.Standalone.exe -r . -f "UTIME TRADS" -n

"-r": spécifie le répertoire dans lequel vont être crées les RESX. 
Par défaut c'est "D:\Serveurs\Sites\iLucca\ iLuccaEntities\Resources", je spécifie ici le répertoire actuel pour tester.

"-f": le nom du fichier sur notre Google Drive. (Vaut mieux qu'il soit unique sur le drive Lucca, mais on peut spécifier le répertoire sur le gdrive avec le paramètre "-g") et le "-n" une option pour qu'il prenne le nom de chaque tableau comme nom de module, c'est-à-dire comme nom de RESX différent. Si ce flag n'est pas spécifié il génère qu'un seul jeu de RESX avec comme nom le nom du fichier.


=== Upload (RESX -> GDoc)

Prerequis: Installer LibreOffice (https://www.libreoffice.org/download/libreoffice-fresh/)
Par défaut l'outil s'attend à LibreOffice accessible sous "C:\Program Files (x86)\LibreOffice 4.0\program\soffice.exe". Si tel n'est pas le cas sur votre machine, on peut spécifier l'emplacement exact avec le paramètre "-s"

Pour effectuer un upload, utilisez la commande suivante:

TranslationTool.Standalone.exe -r . -u -m WFIGGO -g "Lucca Translation"

"-r": spécifie le répertoire dans lequel résident les RESX. 
Par défaut c'est "D:\Serveurs\Sites\iLucca\ iLuccaEntities\Resources", je spécifie ici le répertoire actuel pour tester.

"-u": met le logiciel en mode "upload"

"-m": Le nom du modul à uploader. Si vous ne spécifiez pas "-m", tous les modules vont étré uploadés.

"-g": Le nom du répertoire dans le GDrive


