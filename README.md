# azure-media-redactor-visualizer
Use this with the Azure Media Analytics Redactor service: https://azure.microsoft.com/en-us/documentation/articles/media-services-face-redaction/

After submitting an Analyze pass in the cloud, download the output and use this tool to visualize and preview the output, and help select IDs to select for Redact phase in the cloud.

To compile, you must download and place FFMPEG into the project folder.

1. Build the entire solution
2. Download FFMPEG from:https://ffmpeg.org/download.html. This was tested with build version be1d324 (2016-10-04). Use the static package.
3. Copy ffmpeg.exe and ffprobe.exe to the same output folder as AzureMediaRedactor.exe.
4. Run AzureMediaRedactor.exe

To use:
1. Process your video in your Azure Media Services account with the Redactor MP on Analyze mode.
2. Download both the original video file and the output of the Redaction - Analyze job.
3. Run the visualizer application and choose the files above.
4. Preview your file. Select which faces you'd like to blur via the sidebar on the right.
5. The bottom text field will update with the face IDs. Create a file called "idlist.txt" with these IDs as a newline delimited list.
6. Upload this file to the output asset from step 1. Upload the original video to this asset as well and set as primary asset.
7. Run Redaction job on this asset with "Redact" mode to get the final redacted video.
