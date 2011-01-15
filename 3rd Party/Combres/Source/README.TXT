During the build process, if Combres finds ILMerge command exist at C:\Program Files (x86)\Microsoft\ILMerge\ILMerge.exe, 
it will merge Combres.dll and all referenced libraries into a single assembly in the bin/merged folder.

If you install ILMerge at another location, go to the post build event of Combrse and modify the 'command' variable.
You can download ILMerge at http://www.microsoft.com/downloads/details.aspx?FamilyID=22914587-B4AD-4EAE-87CF-B14AE6A939B0&displaylang=en 
