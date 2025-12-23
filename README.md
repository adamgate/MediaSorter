# MediaSorter
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](License.txt)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple?style=flat-square)]()
[![Windows](https://img.shields.io/badge/Windows-gray?style=flat-square)]()

A command-line application that organizes image and video files into folders based on the date they were taken.

## Features

- Extracts date information from media metadata
- Organizes files into YYYY/MM Month folder structure
- Copies files safely, preserving original files
- Stores files without date metadata in an "unknown" folder

## Supported File Types

### Images
`png` `jpg` `jpeg` `heic` `heif` `avif` `bmp` `dng` `gif` `ico` `jfif` `webp`

### Videos
`avci` `avi` `mov` `mp4`

## Requirements

.NET 8.0 Runtime

## Usage

1. Run the application
2. Enter the source folder path containing media files
3. Enter the destination folder path for organized files
4. Confirm to begin sorting

Files are scanned recursively and organized by their capture date.

## Output Structure
```
destination
├── 2024
│   ├── 01 January
│   │   ├── 20240115_image.jpg
│   │   └── 20240120_video.mp4
|   └── ...
│   └── 12 December
│       └── 20241225_photo.png
└── unknown
    └── no_metadata.jpg
```

## License

MIT License - see LICENSE file for details
