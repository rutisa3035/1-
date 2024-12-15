//fib bundle --output D:\folder\bundlefile.txt

using System.CommandLine;

var bundlecomand = new Command("bundle", "bundle code files to a single file Use 'all' to include all code files in the directory.");
//bundle

var bundleoption = new Option<FileInfo>(new[] { "--output", "-o" }, "file path and name");  //option to bandle comand
var languageoption = new Option<string[]>(new[] { "--language", "-l" }, "Unification of the selected language files")
{
    IsRequired = true,
};
var authorOption = new Option<string>(new[] { "--author", "-a" }, "The author of the bundle");
var empty_linesoption = new Option<bool>(new[] { "--remove", "-r" }, "remove-empty-lines");
var noteoption = new Option<bool>(new[] { "--note", "-n" }, "Do list the source of the file");
var sortoption = new Option<string>(new[] { "--sort", "-s" }, () => "name", "sort by names fule or code");



bundlecomand.AddOption(languageoption);
bundlecomand.AddOption(bundleoption); // הוספה האופציה לבנדל
bundlecomand.AddOption(noteoption);
bundlecomand.AddOption(sortoption);
bundlecomand.AddOption(empty_linesoption);
bundlecomand.AddOption(authorOption);

bundlecomand.SetHandler((output, language, note,sort,remove,author) => //עריכת הפקודה output
{
  

    var allFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), ".");
    //sort
    // מיון לפי השם או הסיומת
    if (sort == "name")
    {
        allFiles = allFiles.OrderBy(file => Path.GetFileName(file)).ToArray();
    }
    else if (sort == "languege")
    {
        allFiles = allFiles.OrderBy(file => Path.GetExtension(file).ToLower()).ToArray();
    }

    // הגדרת תיקיות שיש להחריג (כגון bin, obj, debug, release)
    var excludedDirectories = new[] { "bin", "obj", "debug", "release" };

    // סינון הקבצים כך שלא יכללו קבצים שנמצאים בתיקיות המוחרגות
    allFiles = allFiles
        .Where(file => !excludedDirectories.Any(dir => file.Contains(Path.DirectorySeparatorChar + dir + Path.DirectorySeparatorChar)))
        .ToArray();


    string bundleFilePath = output.FullName;//פתיחת קובץ
    using (StreamWriter writer = new StreamWriter(bundleFilePath, true)) // true מאפשר הוספה
    {
        if (!string.IsNullOrEmpty(author))
    {
        writer.WriteLine($"// Author: {author}");
    }
     
        for (var i = 0; i < allFiles.Length; i++)
        {
            string extension = Path.GetExtension(allFiles[i]).ToLower().TrimStart('.');
        
        string languages = extension switch
        {
            "cs" => "c#",
            "py" => "python",
            "js" => "javascript",
            "html" => "html",
            "css" => "css",
            _ => "unknown" // אם השפה לא ידועה
        };
            if (language.Contains("all") || language.Contains(languages))
            {
                Console.WriteLine($"Including file: {Path.GetFileName(allFiles[i])} with language: {language}");

                 var lines = File.ReadAllLines(allFiles[i]);
                //note
                if (note)
                {
                    string relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), allFiles[i]);
                    writer.WriteLine($"// Source: {Path.GetFileName(allFiles[i])}, Path: {relativePath}");
                }
                    if (remove)
                    {
                        foreach (var line in lines)
                        {
                            // בדיקה אם השורה אינה ריקה או מכילה רק רווחים
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                writer.WriteLine(line); // כתיבת השורה לקובץ ה-bundle
                            }
                        }
                    }
                    else
                    {
                        foreach (var line in lines)
                        {
                           
                                writer.WriteLine(line); // כתיבת השורה לקובץ ה-bundle
                        }
                    }
                    
                }
              
            }
    }
},bundleoption, languageoption, noteoption,sortoption,empty_linesoption,authorOption);

var create_rsp = new Command("create_rsp", "Creating a response file with a ready command");

create_rsp.SetHandler(async () =>
{
    // קלט עבור השפות
    Console.Write("Enter languages (comma separated or 'all'): ");
    var languagesInput = Console.ReadLine();
    

    // קלט עבור נתיב הפלט
    Console.Write("Enter output file path: ");
    var outputFilePath = Console.ReadLine();

    // קלט עבור אם לרשום הערות מקור
    Console.Write("Do you want to note the source? (true/false): ");
    var noteInput = Console.ReadLine();
    bool note = bool.TryParse(noteInput, out bool parsedNote) && parsedNote;

    // קלט עבור מיון
    Console.Write("Sort by (name/language): ");
    var sort = Console.ReadLine();

    // קלט עבור אם למחוק שורות ריקות
    Console.Write("Do you want to remove empty lines? (true/false): ");
    var removeInput = Console.ReadLine();
    bool remove = bool.TryParse(removeInput, out bool parsedRemove) && parsedRemove;

    // קלט עבור שם היוצר
    Console.Write("Enter author name: ");
    var author = Console.ReadLine();

    // בנה פקודת bundle
    string command = $"fib bundle --language {languagesInput} --output {outputFilePath} " +
                     $"{(note ? "--note " : "")}" +
                     $"--sort {sort} " +
                     $"{(remove ? "--remove " : "")}" +
                     $"{(string.IsNullOrWhiteSpace(author) ? "" : $"--author \"{author}\"")}".Trim();

    // שמירה לקובץ תגובה
    string responseFilePath = "response.rsp"; 
    await File.WriteAllTextAsync(responseFilePath, command);

    Console.WriteLine($"Response file created: {responseFilePath}");
});

// הוספת הפקודה ל-root command

var rootcomand = new RootCommand("root comand for file bundler cli");
rootcomand.AddCommand(bundlecomand);
rootcomand.AddCommand(create_rsp);
await rootcomand.InvokeAsync(args); 

