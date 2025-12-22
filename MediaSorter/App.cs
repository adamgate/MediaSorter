using System.Reflection;

using MediaSorter.Models;
using MediaSorter.Services.Interfaces;
using MediaSorter.Utils;

using Microsoft.Extensions.Logging;

using Spectre.Console;

namespace MediaSorter
{
    /// <summary>
    /// Entry point for the program.
    /// </summary>
    public class App
    {
        private readonly IDateParser _dateParser;
        private readonly IDirectoryProvider _directoryProvider;
        private readonly IFileSorter _fileSorter;
        private readonly ILogger<App> _logger;
        private readonly IMediaScanner _mediaScanner;
        private readonly IMetadataProvider _metadataProvider;
        private readonly string _version = Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString() ?? "X.X.X.X";

        public App(
            IDateParser dateParser,
            IDirectoryProvider directoryProvider,
            IFileSorter fileSorter,
            ILogger<App> logger,
            IMediaScanner mediaScanner,
            IMetadataProvider metadataProvider)
        {
            _dateParser = dateParser;
            _directoryProvider = directoryProvider;
            _fileSorter = fileSorter;
            _logger = logger;
            _metadataProvider = metadataProvider;
            _mediaScanner = mediaScanner;
        }

        public void Run(string[] args)
        {
            try
            {
                var rule = new Rule($"[teal]MEDIA SORTER[/] v{_version}").LeftJustified();
                AnsiConsole.Write(rule);
                //AnsiConsole.MarkupLine($":copyright: {DateTime.Now:MMMM yyyy}");

                var sourceDirectory = GetSourceDirectory();
                var mediaPaths = GetMediaPaths(sourceDirectory);
                var mediaWithMetadata = LoadMediaMetadata(mediaPaths);

                var outputDirectory = GetOutputDirectory();
                if (outputDirectory.Equals(sourceDirectory))
                {
                    CliUtils.DisplayMessageAndExit("The output folder cannot be the same as the source folder. Exiting...", "yellow", 0);
                }

                AnsiConsole.WriteLine();
                var shouldProceed = CliUtils.GetYesNoFromUser("Do you want to [green]proceed[/]?");
                if (!shouldProceed)
                {
                    CliUtils.DisplayMessageAndExit("Exiting...", "yellow", 0);
                }

                var mediaWithDatesTaken = ParseMediaDatesTaken(mediaWithMetadata);
                SortMediaFiles(outputDirectory, mediaWithDatesTaken);
                _logger.LogDebug("Done sorting {count} files.", mediaWithDatesTaken.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                CliUtils.DisplayMessageWithColor($"An error occurred: {ex.Message}", "red");
                CliUtils.DisplayMessageAndExit("Exiting...", "red", 1);
            }
        }

        private IEnumerable<string> GetMediaPaths(string sourceDirectory)
        {
            var mediaPaths = new List<string>();

            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .Start("[orange1]Scanning for media...[/]", ctx =>
                {
                    Thread.Sleep(3000);
                    mediaPaths.AddRange(_mediaScanner.GetMediaInPath(sourceDirectory));
                });

            if (!mediaPaths.Any())
            {
                CliUtils.DisplayMessageAndExit("No media files were found. Exiting...", "yellow", 0);
            }

            _logger.LogDebug("Found {count} media file(s) to sort.", mediaPaths.Count());

            var table = new Table()
                .Title("Media Files Found").LeftAligned()
                .AddColumn("Media Files")
                .Caption($"{mediaPaths.Count} Files Found").LeftAligned();

            foreach (var media in mediaPaths)
            {
                var textPath = new TextPath(media)
                    .RootStyle(new Style(foreground: Color.Green))
                    .SeparatorStyle(new Style(foreground: Color.Red))
                    .StemStyle(new Style(foreground: Color.Yellow))
                    .LeafStyle(new Style(foreground: Color.Purple));
                table.AddRow(textPath);
            }

            AnsiConsole.Write(table);

            return mediaPaths;
        }

        private string GetOutputDirectory()
        {
            AnsiConsole.WriteLine();
            var outputDirectory = _directoryProvider.GetValidDirectory("Please enter the path of the folder where you wish to save the sorted files:");
            if (outputDirectory is null)
            {
                CliUtils.DisplayMessageAndExit("Exiting...", "yellow", 0);
            }

            _logger.LogDebug("Output Directory: \"{directory}\"", outputDirectory);

            return outputDirectory;
        }

        private string GetSourceDirectory()
        {
            AnsiConsole.WriteLine();
            var sourceDirectory = _directoryProvider.GetValidDirectory("Please enter the path of the folder you wish to sort:");
            if (sourceDirectory is null)
            {
                CliUtils.DisplayMessageAndExit("Exiting...", "yellow", 0);
            }

            _logger.LogDebug("Source Directory: \"{directory}\"", sourceDirectory);

            return sourceDirectory;
        }

        private IDictionary<string, IEnumerable<RawMetadata>> LoadMediaMetadata(IEnumerable<string> mediaPaths)
        {
            IDictionary<string, IEnumerable<RawMetadata>>? mediaWithMetadata = null;

            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .Start("[orange1]Processing date metadata...[/]", ctx =>
                {
                    Thread.Sleep(3000);
                    mediaWithMetadata = _metadataProvider.EvaluateMediaMetadata(mediaPaths);
                });

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("Date metadata loaded.");

            return mediaWithMetadata;
        }

        private IDictionary<string, DateMetadata> ParseMediaDatesTaken(IDictionary<string, IEnumerable<RawMetadata>> mediaWithMetadata)
        {
            IDictionary<string, DateMetadata>? mediaWithDatesTaken = null;

            AnsiConsole.WriteLine();
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .Start("[orange1]Processing dates...[/]", ctx =>
                {
                    Thread.Sleep(3000);
                    mediaWithDatesTaken = _dateParser.Parse(mediaWithMetadata);
                });

            AnsiConsole.MarkupLine("Done processing dates.");

            return mediaWithDatesTaken;
        }

        private void SortMediaFiles(string outputDirectory, IDictionary<string, DateMetadata> mediaWithDatesTaken)
        {
            var copyStatus = new List<(string, bool, string)>();

            AnsiConsole.WriteLine();

            AnsiConsole.Progress()
                .Columns(
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn()
                )
                .Start(ctx =>
                {
                    var task = ctx.AddTask($"Sorting [yellow]{mediaWithDatesTaken.Count}[/] files...", maxValue: mediaWithDatesTaken.Count);

                    var count = 0;
                    foreach (var result in _fileSorter.SortMediaFilesByDate(outputDirectory, mediaWithDatesTaken))
                    {
                        count++;
                        task.Description = $"[yellow]Sorted [green]{count}[/]/{mediaWithDatesTaken.Count} items[/]";
                        copyStatus.Add(result);
                        task.Increment(1);
                        //task.Percentage = 100 / mediaWithDatesTaken.Count;
                        Thread.Sleep(15);
                    }

                    Thread.Sleep(2000);
                    task.StopTask();
                });

            var table = new Table()
                .Title("Status of Sorted Files").LeftAligned()
                .AddColumn("[orange1]File[/]")
                .AddColumn("[orange1]Copy Status[/]")
                .AddColumn("[orange1]Message[/]");

            foreach (var status in copyStatus)
            {
                var mediaTextPath = new TextPath(status.Item1)
                    .RootStyle(new Style(foreground: Color.Green))
                    .SeparatorStyle(new Style(foreground: Color.Red))
                    .StemStyle(new Style(foreground: Color.Yellow))
                    .LeafStyle(new Style(foreground: Color.Purple));

                var successStatus = status.Item2 ? "[green]Successful[/]" : "[red]Unsuccessful[/]";

                table.AddRow(mediaTextPath, new Markup(successStatus), new Text(status.Item3));
            }

            AnsiConsole.Write(table);

            CliUtils.DisplayMessageAndExit($"Successfully sorted [yellow]{mediaWithDatesTaken.Count}[/] files. Exiting...", "green", 0);
        }
    }
}