# Docs to Git Flavored Markdown (Docx2GFMD)

The tool has a dependency on [Pandoc](https://github.com/jgm/pandoc)

## Examples

- Will convert the current directory

```cmd
Docx2GFMD.exe  
```

- Will convert specified directory

```cmd
Docx2GFMD.exe C:\Directory\To\Prep
```

The tool does the following: 

1.	Creates the following folder structure
```cmd
C:\Directory\To\Prep\Converted
			\.attachments
			\Prep
```
2.	Converts either the current directory or a specified directory to github flavored markdown, preferred by VSTS Wiki
3.	Prepares an index folder with VSTS compatible reference links to the content, you may need to add the rest of the link from Azure-Wiki-Home location for example.
4.	Extracts all media to the .attachments folder
5.	Prepends all images with the name of the directory being converted, in this example image1.png becomes prep_image1.png
6.	Updates the links to images on all the converted markdown pages.  

This should give everyone a head start if you have any mass migrations of documents.  
