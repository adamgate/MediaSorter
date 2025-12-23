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
                AnsiConsole.Clear();
                var rule = new Rule($"[bold orange1]MEDIA SORTER[/] [dim]v{_version}[/]")
                    .RuleStyle("orange1")
                    .LeftJustified();
                AnsiConsole.Write(rule);

                var sourceDirectory = GetSourceDirectory();
                var mediaPaths = GetMediaPaths(sourceDirectory);
                var mediaWithMetadata = LoadMediaMetadata(mediaPaths);

                var outputDirectory = GetOutputDirectory();
                if (outputDirectory.Equals(sourceDirectory))
                {
                    CliUtils.DisplayMessageAndExit("The output folder cannot be the same as the source folder. Exiting...", "yellow", 0);
                }

                AnsiConsole.WriteLine();
                var shouldProceed = CliUtils.GetYesNoFromUser("Do you want to [bold orange1]proceed[/]?");
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
                .SpinnerStyle("orange1")
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
                .BorderColor(Color.Orange1)
                .LeftAligned()
                .AddColumn($"[bold orange1]Media Files Found [dim]({mediaPaths.Count})[/][/]")
                .LeftAligned();
            table.Columns[0].LeftAligned();

            foreach (var media in mediaPaths)
            {
                var textPath = new TextPath(media)
                    .RootStyle(new Style(foreground: Color.Orange3))
                    .SeparatorStyle(new Style(foreground: Color.Grey))
                    .StemStyle(new Style(foreground: Color.Yellow))
                    .LeafStyle(new Style(foreground: Color.White));
                table.AddRow(textPath);
            }

            AnsiConsole.Write(table);

            return mediaPaths;
        }

        private string GetOutputDirectory()
        {
            AnsiConsole.WriteLine();
            var outputDirectory = _directoryProvider.GetValidDirectory("[yellow]Please enter the path of the folder where you wish to save the sorted files:[/]");
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
            var sourceDirectory = _directoryProvider.GetValidDirectory("[yellow]Please enter the path of the folder you wish to sort:[/]");
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
                .SpinnerStyle("orange1")
                .Start("[orange1]Processing date metadata...[/]", ctx =>
                {
                    Thread.Sleep(3000);
                    mediaWithMetadata = _metadataProvider.EvaluateMediaMetadata(mediaPaths);
                });

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold green]Date metadata loaded.[/]");

            return mediaWithMetadata;
        }

        private IDictionary<string, DateMetadata> ParseMediaDatesTaken(IDictionary<string, IEnumerable<RawMetadata>> mediaWithMetadata)
        {
            IDictionary<string, DateMetadata>? mediaWithDatesTaken = null;

            AnsiConsole.WriteLine();
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle("orange1")
                .Start("[orange1]Processing dates...[/]", ctx =>
                {
                    Thread.Sleep(3000);
                    mediaWithDatesTaken = _dateParser.Parse(mediaWithMetadata);
                });

            AnsiConsole.MarkupLine("[bold green]Done processing dates.[/]");

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
                    new PercentageColumn(),
                    new SpinnerColumn(Spinner.Known.Dots)
                )
                .Start(ctx =>
                {
                    var task = ctx.AddTask($"[orange1]Sorting[/] [yellow]{mediaWithDatesTaken.Count}[/] [orange1]files...[/]", maxValue: mediaWithDatesTaken.Count);

                    var count = 0;
                    foreach (var result in _fileSorter.SortMediaFilesByDate(outputDirectory, mediaWithDatesTaken))
                    {
                        count++;
                        task.Description = $"[orange1]Sorted [gray]{count}[/][dim]/[/][bold orange1]{mediaWithDatesTaken.Count}[/] files[/]";
                        copyStatus.Add(result);
                        task.Increment(1);
                    }

                    Thread.Sleep(2000);
                    task.StopTask();
                });

            var table = new Table()
                .Title("[bold orange1]Status of Sorted Files[/]")
                .BorderColor(Color.Orange1)
                .AddColumn("[orange1]File[/]")
                .AddColumn("[orange1]Copy Status[/]")
                .AddColumn("[orange1]Message[/]");

            foreach (var status in copyStatus)
            {
                var mediaTextPath = new TextPath(status.Item1)
                    .RootStyle(new Style(foreground: Color.Orange3))
                    .SeparatorStyle(new Style(foreground: Color.Grey))
                    .StemStyle(new Style(foreground: Color.Yellow))
                    .LeafStyle(new Style(foreground: Color.White));

                var successStatus = status.Item2 ? "[green]Successful[/]" : "[red]Failed[/]";

                table.AddRow(mediaTextPath, new Markup(successStatus), new Text(status.Item3));
            }

            AnsiConsole.Write(table);

            CliUtils.DisplayMessageAndExit($"Successfully sorted [orange1]{mediaWithDatesTaken.Count}[/] files. Exiting...", "green", 0);
        }
    }
}