=== Download (Gdoc -> RESX)

TranslationTool.Standalone.exe -r . -f "UTIME TRADS" -n

"-r": sp�cifie le r�pertoire dans lequel vont �tre cr�es les RESX. 
Par d�faut c'est "D:\Serveurs\Sites\iLucca\ iLuccaEntities\Resources", je sp�cifie ici le r�pertoire actuel pour tester.

"-f": le nom du fichier sur notre Google Drive. (Vaut mieux qu'il soit unique sur le drive Lucca, mais on peut sp�cifier le r�pertoire sur le gdrive avec le param�tre "-g") et le "-n" une option pour qu'il prenne le nom de chaque tableau comme nom de module, c'est-�-dire comme nom de RESX diff�rent. Si ce flag n'est pas sp�cifi� il g�n�re qu'un seul jeu de RESX avec comme nom le nom du fichier.


=== Upload (RESX -> GDoc)

Prerequis: Installer LibreOffice (https://www.libreoffice.org/download/libreoffice-fresh/)
Par d�faut l'outil s'attend � LibreOffice accessible sous "C:\Program Files (x86)\LibreOffice 4.0\program\soffice.exe". Si tel n'est pas le cas sur votre machine, on peut sp�cifier l'emplacement exact avec le param�tre "-s"

Pour effectuer un upload, utilisez la commande suivante:

TranslationTool.Standalone.exe -r . -u -m WFIGGO -g "Lucca Translation"

"-r": sp�cifie le r�pertoire dans lequel r�sident les RESX. 
Par d�faut c'est "D:\Serveurs\Sites\iLucca\ iLuccaEntities\Resources", je sp�cifie ici le r�pertoire actuel pour tester.

"-u": met le logiciel en mode "upload"

"-m": Le nom du modul � uploader. Si vous ne sp�cifiez pas "-m", tous les modules vont �tr� upload�s.

"-g": Le nom du r�pertoire dans le GDrive


